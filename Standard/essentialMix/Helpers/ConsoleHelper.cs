using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Exceptions.IO;

namespace essentialMix.Helpers
{
	public static class ConsoleHelper
	{
		private const string FONT_DEF = "Lucida Console";

		private static FieldInfo __fiOut;
		private static FieldInfo __fiError;
		private static MethodInfo __fiInitializeStdOutError;

		public static bool IsInputRedirected => FileType.Char != Win32.GetFileType(GetInputHandle());

		public static bool IsOutputRedirected => FileType.Char != Win32.GetFileType(GetOutputHandle());

		public static bool IsErrorRedirected => FileType.Char != Win32.GetFileType(GetErrorHandle());

		public static bool HasConsole => !GetConsoleWindow().IsInvalidHandle();

		public static IntPtr AttachConsole() { return AttachConsole(out _); }

		public static IntPtr AttachConsole(out bool consoleCreated, ConsoleModesEnum remove = ConsoleModesEnum.NONE, ConsoleModesEnum add = ConsoleModesEnum.NONE)
		{
			consoleCreated = false;
			IntPtr hConsole = GetConsoleWindow();

			if (hConsole.IsInvalidHandle())
			{
				if (!Win32.AllocConsole()) return IntPtr.Zero;

				consoleCreated = true;
				hConsole = GetConsoleWindow();

				if (hConsole.IsInvalidHandle())
				{
					consoleCreated = false;
					FreeConsole();
					return IntPtr.Zero;
				}

				ResetInputRedirection();
				ResetOutputRedirection();
				ResetErrorRedirection();
			}

			if (hConsole.IsInvalidHandle())
			{
				if (consoleCreated)
				{
					consoleCreated = false;
					FreeConsole();
				}

				return IntPtr.Zero;
			}

			if (remove != ConsoleModesEnum.NONE || add != ConsoleModesEnum.NONE) SetMode(remove, add);
			return hConsole;
		}

		public static bool AttachConsole(IntPtr hWndParent, out bool consoleCreated, bool visible = true, ConsoleModesEnum remove = ConsoleModesEnum.NONE, 
			ConsoleModesEnum add = ConsoleModesEnum.NONE)
		{
			consoleCreated = false;
			if (hWndParent.IsInvalidHandle() || !Win32.IsWindow(hWndParent)) return false;

			IntPtr hConsole = AttachConsole(out consoleCreated, remove, add);
			if (hConsole.IsInvalidHandle()) return false;

			IntPtr hParent = Win32.GetParent(hConsole);

			if (hParent != hWndParent)
			{
				IntPtr prevParent = Win32.SetParent(hConsole, hWndParent);

				if (prevParent.IsInvalidHandle())
				{
					if (consoleCreated)
					{
						consoleCreated = false;
						FreeConsole();
					}

					return false;
				}
			}

			uint style = Win32.GetWindowLong(hConsole, WindowLongSettingIndexEnum.GWL_STYLE);
			style &= (uint)~(WindowStylesEnum.WS_POPUP | WindowStylesEnum.WS_BORDER);
			style |= (uint)WindowStylesEnum.WS_EMBEDED;
			
			if (visible)
				style |= (uint)WindowStylesEnum.WS_VISIBLE;
			else
				style &= (uint)~WindowStylesEnum.WS_VISIBLE;

			Win32.SetWindowLong(hConsole, WindowLongSettingIndexEnum.GWL_STYLE, style);
			if (Win32.GetClientRect(hWndParent, out RECT rc)) Win32.SetWindowPos(hConsole, IntPtr.Zero, 0, 0, rc.Width, rc.Height, WindowPositionFlagsEnum.SWP_FRAMECHANGED);
			Win32.SendMessage(hWndParent, Win32.WM_CHANGEUISTATE, (IntPtr)Win32.UIS_INITIALIZE, IntPtr.Zero);
			return true;
		}

		public static void Show()
		{
			if (HasConsole || AttachConsole().IsInvalidHandle()) return;
			InvalidateOutAndError();
		}

		public static void Hide()
		{
			if (!HasConsole) return;
			SetOutAndErrorNull();
			FreeConsole();
		}

		public static void ResizeConsole(IntPtr hWndParent)
		{
			if (hWndParent.IsInvalidHandle() || !Win32.IsWindow(hWndParent)) throw new InvalidHandleException("hWndParent is not a handle of a window or a control.");

			IntPtr hConsole = GetConsoleWindow();
			if (hConsole.IsInvalidHandle()) throw new InvalidOperationException("No console window is attached to this process.");

			IntPtr hParent = Win32.GetParent(hConsole);
			if (hParent != hWndParent) throw new InvalidOperationException("hWndParent is different from the console parent.");

			if (!Win32.GetClientRect(hWndParent, out RECT rc)) return;
			Win32.MoveWindow(hConsole, 0, 0, rc.Width, rc.Height, true);
		}

		public static void FreeConsole()
		{
			Win32.FreeConsole();
		}

		public static IntPtr GetConsoleWindow() { return Win32.GetConsoleWindow(); }

		public static IntPtr GetInputHandle() { return Win32.GetStdHandle(Win32.STD_INPUT_HANDLE); }

		public static IntPtr GetOutputHandle() { return Win32.GetStdHandle(Win32.STD_OUTPUT_HANDLE); }

		public static IntPtr GetErrorHandle() { return Win32.GetStdHandle(Win32.STD_ERROR_HANDLE); }

		public static IntPtr GetDefaultInputHandle()
		{
			return Win32.CreateFile("CONIN$", FileAccessEnum.GenericRead | FileAccessEnum.GenericWrite, FileShareEnum.Read, IntPtr.Zero,
									CreationDispositionEnum.OpenExisting, 0, IntPtr.Zero);
		}

		public static IntPtr GetDefaultOutputHandle()
		{
			return Win32.CreateFile("CONOUT$", FileAccessEnum.GenericRead | FileAccessEnum.GenericWrite, FileShareEnum.Write, IntPtr.Zero,
									CreationDispositionEnum.OpenExisting, 0, IntPtr.Zero);
		}

		public static IntPtr GetDefaultErrorHandle()
		{
			return Win32.CreateFile("CONERR$", FileAccessEnum.GenericRead | FileAccessEnum.GenericWrite, FileShareEnum.Write, IntPtr.Zero,
									CreationDispositionEnum.OpenExisting, 0, IntPtr.Zero);
		}

		public static void ResetInputRedirection()
		{
			if (!Console.IsInputRedirected) return;

			IntPtr handle = GetDefaultInputHandle();
			if (handle.IsInvalidHandle()) return;
			Win32.SetStdHandle((IntPtr)Win32.STD_INPUT_HANDLE, handle);

			TextReader inReader = new StreamReader(Console.OpenStandardInput());
			Console.SetIn(inReader);
		}

		public static void ResetOutputRedirection()
		{
			if (!Console.IsOutputRedirected) return;

			IntPtr handle = GetDefaultOutputHandle();
			if (handle.IsInvalidHandle()) return;
			Win32.SetStdHandle((IntPtr)Win32.STD_OUTPUT_HANDLE, handle);

			TextWriter outWriter = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
			Console.SetOut(outWriter);
		}

		public static void ResetErrorRedirection()
		{
			if (!Console.IsErrorRedirected) return;

			IntPtr handle = GetDefaultErrorHandle();
			if (handle.IsInvalidHandle()) return;
			Win32.SetStdHandle((IntPtr)Win32.STD_ERROR_HANDLE, handle);

			TextWriter errWriter = new StreamWriter(Console.OpenStandardError()) {AutoFlush = true};
			Console.SetError(errWriter);
		}

		public static ConsoleModesEnum GetMode() { return GetMode(GetInputHandle()); }

		public static bool SetMode(ConsoleModesEnum mode) { return SetMode(GetInputHandle(), mode); }

		public static bool SetMode(ConsoleModesEnum remove, ConsoleModesEnum add)
		{
			if (remove == ConsoleModesEnum.NONE && add == ConsoleModesEnum.NONE) return true;

			IntPtr hInput = GetInputHandle();
			ConsoleModesEnum consoleMode = GetMode(hInput);

			if (remove != ConsoleModesEnum.NONE)
			{
				if (remove.FastHasFlag(ConsoleModesEnum.ENABLE_QUICK_EDIT_MODE))
				{
					remove &= ~ConsoleModesEnum.ENABLE_QUICK_EDIT_MODE;
					remove |= ConsoleModesEnum.ENABLE_EXTENDED_FLAGS;
				}

				consoleMode &= ~remove;
			}

			if (add != ConsoleModesEnum.NONE)
			{
				if (add.FastHasFlag(ConsoleModesEnum.ENABLE_QUICK_EDIT_MODE)) add |= ConsoleModesEnum.ENABLE_EXTENDED_FLAGS;
				consoleMode |= add;
			}

			return SetMode(consoleMode);
		}

		public static bool SetIcon(IntPtr hIcon) { return Win32.SetConsoleIcon(hIcon); }

		public static void HideScrollbars()
		{
			int cx = Console.WindowWidth;
			int cy = Console.WindowHeight;
			Console.SetWindowSize(1, 1);
			Console.SetBufferSize(cx, cy);
			Console.SetWindowSize(cx, cy);
		}


		public static CONSOLE_FONT_INFO? GetCurrentFont() { return GetCurrentFont(GetOutputHandle()); }

		public static COORD? GetFontSize()
		{
			IntPtr hConsole = GetOutputHandle();
			CONSOLE_FONT_INFO? fontInfo = GetCurrentFont(hConsole);
			if (fontInfo == null) return null;

			//Use that index to obtain font size
			return Win32.GetConsoleFontSize(hConsole, fontInfo.Value.nFont);
		}

		public static int GetFontCount() { return (int)Win32.GetNumberOfConsoleFonts(); }

		[NotNull]
		public static ConsoleFont[] GetConsoleFonts()
		{
			IntPtr hConsole = GetOutputHandle();
			if (hConsole.IsInvalidHandle()) return Array.Empty<ConsoleFont>();

			int c = GetFontCount();
			if (c == 0) return Array.Empty<ConsoleFont>();

			ConsoleFont[] fonts = new ConsoleFont[c];
			Win32.GetConsoleFontInfo(hConsole, false, (uint)fonts.Length, fonts);
			return fonts;
		}

		public static bool SetFont(uint index)
		{
			IntPtr hConsole = GetOutputHandle();
			return !hConsole.IsInvalidHandle() && Win32.SetConsoleFont(hConsole, index) != 0;
		}

		public static CONSOLE_FONT_INFO_EX GetCurrentFontEx() { return GetCurrentFontEx(GetOutputHandle()); }

		public static bool SetCurrentFontEx([NotNull] string fontName = FONT_DEF, FontMask fontFamily = FontMask.TRUETYPE_FONTTYPE, short size = 11, FontWeight weight = FontWeight.FW_DONTCARE)
		{
			CONSOLE_FONT_INFO_EX fontInfoEx = new CONSOLE_FONT_INFO_EX
			{
				lpszFaceName = fontName,
				nFamily = fontFamily,
				nWidth = size,
				nHeight = size,
				nWeight = weight
			};
			
			return SetCurrentFontEx(fontInfoEx);
		}

		public static bool SetCurrentFontEx([NotNull] CONSOLE_FONT_INFO_EX fontInfoEx)
		{
			IntPtr hConsole = GetOutputHandle();
			return !hConsole.IsInvalidHandle() && Win32.SetCurrentConsoleFontEx(hConsole, false, ref fontInfoEx);
		}
		
		public static bool IsConsoleFontTrueType()
		{
			CONSOLE_FONT_INFO_EX fontInfoEx = GetCurrentFontEx();
			return fontInfoEx != null && fontInfoEx.nFamily.FastHasFlag(FontMask.TRUETYPE_FONTTYPE);
		}

		public static void RegisterHandler([NotNull] ConsoleCtrlDelegate handler) { Win32.SetConsoleCtrlHandler(handler, true); }
		public static void UnregisterHandler([NotNull] ConsoleCtrlDelegate handler) { Win32.SetConsoleCtrlHandler(handler, false); }

		public static void Pause()
		{
			Console.WriteLine();
			Console.Write("Press any key to continue...");
			Console.ReadKey(true);
		}

		public static void RunServicesInteractively([NotNull] params ServiceBase[] servicesToRun)
		{
			Type type = typeof(ServiceBase);
			MethodInfo onStartMethod = type.GetMethod("OnStart", Constants.BF_NON_PUBLIC_INSTANCE) ?? throw new InvalidOperationException("Could not obtain a reference to OnStart method.");
			MethodInfo onStopMethod = type.GetMethod("OnStop", Constants.BF_NON_PUBLIC_INSTANCE) ?? throw new InvalidOperationException("Could not obtain a reference to OnStop method.");

			Console.WriteLine();
			Console.WriteLine("Starting the service(s) in interactive mode...");
			Console.WriteLine();

			// Start services loop
			foreach (ServiceBase service in servicesToRun)
			{
				Console.Write($"Starting {service.ServiceName}... ");
				onStartMethod.Invoke(service, Array.Empty<object>());
				Console.WriteLine("Started");
			}

			// Waiting the end
			Console.WriteLine();
			Console.WriteLine("Press a key to stop...");
			Console.ReadKey(true);
			Console.WriteLine();

			// Stop loop
			foreach (ServiceBase service in servicesToRun)
			{
				Console.Write("Stopping {0}... ", service.ServiceName);
				onStopMethod.Invoke(service, null);
				Console.WriteLine("Stopped");
			}

			Console.WriteLine();
			if (Debugger.IsAttached) Pause();
		}

		private static ConsoleModesEnum GetMode(IntPtr hInput)
		{
			if (hInput.IsInvalidHandle()) return ConsoleModesEnum.NONE;
			return Win32.GetConsoleMode(hInput, out ConsoleModesEnum mode)
						? mode
						: ConsoleModesEnum.NONE;
		}

		private static bool SetMode(IntPtr hInput, ConsoleModesEnum mode) { return !hInput.IsInvalidHandle() && Win32.SetConsoleMode(hInput, mode); }

		private static CONSOLE_FONT_INFO? GetCurrentFont(IntPtr hConsole)
		{
			if (hConsole.IsInvalidHandle()) return null;

			//Obtain the current console font index
			return !Win32.GetCurrentConsoleFont(hConsole, false, out CONSOLE_FONT_INFO fontInfo) ? null : fontInfo;
		}

		private static CONSOLE_FONT_INFO_EX GetCurrentFontEx(IntPtr hConsole)
		{
			if (hConsole.IsInvalidHandle()) return null;

			//Obtain the current console font index
			CONSOLE_FONT_INFO_EX fontInfo = new CONSOLE_FONT_INFO_EX();
			return !Win32.GetCurrentConsoleFontEx(hConsole, false, fontInfo) ? null : fontInfo;
		}

		private static void InvalidateOutAndError()
		{
			if (__fiOut == null || __fiError == null || __fiInitializeStdOutError == null)
			{
				Type type = typeof(Console);
				__fiOut ??= type.GetField("_out", Constants.BF_NON_PUBLIC_STATIC);
				__fiError ??= type.GetField("_error", BindingFlags.Static | BindingFlags.NonPublic);
				__fiInitializeStdOutError ??= type.GetMethod("InitializeStdOutError", BindingFlags.Static | BindingFlags.NonPublic);
			}

			__fiOut?.SetValue(null, null);
			__fiError?.SetValue(null, null);
			__fiInitializeStdOutError?.Invoke(null, new object[] { true });
		}

		private static void SetOutAndErrorNull()
		{
			Console.SetOut(TextWriter.Null);
			Console.SetError(TextWriter.Null);
		}
	}
}