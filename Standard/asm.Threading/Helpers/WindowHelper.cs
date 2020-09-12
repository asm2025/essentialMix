using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using asm.Extensions;
using asm.Threading.Extensions;
using JetBrains.Annotations;

namespace asm.Threading.Helpers
{
	public class WindowHelper
	{
		private IntPtr _handle;
		private WindowHelper _mainWindow;
		private int? _pid;
		private int? _threadId;
		private bool? _isDialog;
		private string _title;
		private string _messageBoxText;
		private string _className;

		public WindowHelper(IntPtr handle)
		{
			Handle = handle;
		}

		public IntPtr Handle
		{
			get
			{
				AssertHandle();
				return _handle;
			}
			private set => _handle = value;
		}

		public int ProcessId
		{
			get
			{
				_pid ??= GetWindowId(false);
				return _pid.Value;
			}
		}

		public int ThreadId
		{
			get
			{
				_threadId ??= GetWindowId(true);
				return _threadId.Value;
			}
		}

		public bool IsDialog => _isDialog ?? (_isDialog = GetWindowStyle().HasFlag((uint)Win32.WindowStylesEnum.WS_DLGFRAME)).Value;

		public WindowHelper MainWindow => _mainWindow ??= GetWindowMain();

		[NotNull]
		public string Title => _title ??= GetWindowText();

		public string MessageBoxText => _messageBoxText ??= GetMessageBoxText();

		[NotNull]
		public string ClassName => _className ??= GetWindowClassName();

		public bool HasValidHandle
		{
			get
			{
				try
				{
					AssertHandle();
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public uint GetStyle() { return Win32.GetWindowLong(Handle, Win32.WindowLongSettingIndexEnum.GWL_STYLE); }

		public uint SetStyle(uint value) { return Win32.SetWindowLong(Handle, Win32.WindowLongSettingIndexEnum.GWL_STYLE, value); }

		public uint GetExtStyle() { return Win32.GetWindowLong(Handle, Win32.WindowLongSettingIndexEnum.GWL_EXSTYLE); }

		public uint SetExtStyle(uint value) { return Win32.SetWindowLong(Handle, Win32.WindowLongSettingIndexEnum.GWL_EXSTYLE, value); }

		public int SendMessage(uint message, IntPtr wParam, IntPtr lParam) { return (int)Win32.SendMessage(Handle, message, wParam, lParam); }

		private void AssertHandle()
		{
			if (Win32.IsWindow(_handle)) return;
			throw new InvalidOperationException("The handle is no longer valid.");
		}

		public static bool EnumerateWindows([NotNull] Func<WindowHelper, bool> callback)
		{
			return Win32.EnumWindows((hWnd, param) => callback(new WindowHelper(hWnd)), IntPtr.Zero);
		}

		public static bool EnumerateChildWindows(IntPtr hWndParent, [NotNull] Func<WindowHelper, bool> callback)
		{
			return Win32.EnumChildWindows(hWndParent, (hWnd, param) => callback(new WindowHelper(hWnd)), IntPtr.Zero);
		}

		public static WindowHelper FindWindow([NotNull] Predicate<WindowHelper> predicate)
		{
			WindowHelper target = null;
			Win32.EnumWindows((hWnd, param) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				if (!predicate(current)) return true;
				target = current;
				return false;
			}, IntPtr.Zero);
			return target;
		}

		public static WindowHelper FindChildWindow(IntPtr hWndParent, [NotNull] Predicate<WindowHelper> predicate)
		{
			WindowHelper target = null;
			Win32.EnumChildWindows(hWndParent, (hWnd, lParam) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				if (!predicate(current)) return true;
				target = current;
				return false;
			}, IntPtr.Zero);
			return target;
		}

		[NotNull]
		public static IList<WindowHelper> FindWindows([NotNull] Predicate<WindowHelper> predicate)
		{
			IList<WindowHelper> target = new List<WindowHelper>();
			Win32.EnumWindows((hWnd, lParam) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				if (predicate(current)) target.Add(current);
				return true;
			}, IntPtr.Zero);
			return target;
		}

		[NotNull]
		public static IList<WindowHelper> FindChildWindows(IntPtr hWndParent, [NotNull] Predicate<WindowHelper> predicate)
		{
			List<WindowHelper> target = new List<WindowHelper>();
			Win32.EnumChildWindows(hWndParent, (hWnd, lParam) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				if (predicate(current)) target.Add(current);
				return true;
			}, IntPtr.Zero);
			return target.ToArray();
		}

		public static WindowHelper FindProcessWindow([NotNull] Process process, [NotNull] Predicate<WindowHelper> predicate)
		{
			if (!process.IsAwaitable()) return null;

			WindowHelper target = null;

			foreach (ProcessThread thread in process.Threads
				.CastTo<ProcessThread>()
				.TakeWhile(p => process.IsAwaitable()))
			{
				target = FindThreadWindow(thread, predicate);
				if (target != null) break;
			}

			return target;
		}

		public static IEnumerable<WindowHelper> FindProcessWindows([NotNull] Process process, [NotNull] Predicate<WindowHelper> predicate)
		{
			if (!process.IsAwaitable()) yield break;

			foreach (ProcessThread thread in process.Threads
				.CastTo<ProcessThread>()
				.TakeWhile(p => process.IsAwaitable()))
			{
				foreach (WindowHelper windowHelper in FindThreadWindows(thread, predicate))
					yield return windowHelper;
			}
		}

		public static WindowHelper FindThreadWindow([NotNull] ProcessThread thread, [NotNull] Predicate<WindowHelper> predicate)
		{
			if (!thread.IsAwaitable()) return null;

			WindowHelper target = null;
			Win32.EnumThreadWindows(thread.Id, (hWnd, lParam) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				
				if (predicate(current))
				{
					target = current;
					return false;
				}

				return thread.IsAwaitable();
			}, IntPtr.Zero);
			return target;
		}

		[NotNull]
		public static IEnumerable<WindowHelper> FindThreadWindows([NotNull] ProcessThread thread, [NotNull] Predicate<WindowHelper> predicate)
		{
			if (!thread.IsAwaitable()) return Enumerable.Empty<WindowHelper>();

			IList<WindowHelper> list = new List<WindowHelper>();
			Win32.EnumThreadWindows(thread.Id, (hWnd, lParam) =>
			{
				WindowHelper current = new WindowHelper(hWnd);
				if (predicate(current)) list.Add(current);
				return true;
			}, IntPtr.Zero);
			return list;
		}

		private int GetWindowId(bool returnThread)
		{
			uint tid = Win32.GetWindowThreadProcessId(Handle, out uint pid);
			return (int)(returnThread ? tid : pid);
		}

		private uint GetWindowStyle() { return Win32.GetWindowLong(Handle, Win32.WindowLongSettingIndexEnum.GWL_STYLE); }

		private WindowHelper GetWindowMain()
		{
			WindowHelper value;

			try
			{
				Process proc = Process.GetProcessById(ProcessId);
				value = proc.MainWindowHandle == Handle ? this : new WindowHelper(proc.MainWindowHandle);
			}
			catch { value = null; }

			return value;
		}

		[NotNull]
		private string GetWindowText()
		{
			IntPtr handle = Handle;
			int length = Win32.GetWindowTextLength(handle);
			if (length == 0) return string.Empty;

			StringBuilder buffer = new StringBuilder(length + 1);
			Win32.GetWindowText(handle, buffer, buffer.Capacity);
			return buffer.ToString();
		}

		private string GetMessageBoxText()
		{
			if (!IsDialog) return null;

			IntPtr handle = Handle;
			IntPtr dlgHandle = Win32.GetDlgItem(handle, 0xFFFF);
			if (dlgHandle == IntPtr.Zero) return null;

			int length = Win32.GetWindowTextLength(dlgHandle);
			if (length == 0) return string.Empty;

			StringBuilder buffer = new StringBuilder(length + 1);
			Win32.GetWindowText(dlgHandle, buffer, buffer.Capacity);
			return buffer.ToString();
		}

		[NotNull]
		private string GetWindowClassName()
		{
			IntPtr handle = Handle;
			StringBuilder buffer = new StringBuilder(Win32.CLASS_NAME_MAX_LENGTH);
			Win32.GetClassName(handle, buffer, buffer.Capacity);
			return buffer.ToString();
		}
	}
}
