using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using asm.Extensions;
using asm.Helpers;
using asm.IO;
using asm.Threading;
using asm.Windows.Actipro.Properties;
using asm.Windows.Design;
using asm.Windows.Extensions;
using asm.Drawing.Extensions;
using ActiproSoftware.SyntaxEditor;
using JetBrains.Annotations;

namespace asm.Windows.Actipro.Controls
{
	[Designer(typeof(EmptyStringControlDesigner))]
	public class SyntaxConsole : SyntaxEditor
	{
		private const string CAT_MODE = "Mode";
		private const string CAT_BEHAVIOUR = "Behaviour";

		private readonly IList<KeyMapping> _keyMappings = new List<KeyMapping>();

		private int _inputStart = -1;
		private int _skipStart;
		private ProcessInterface _processInterface;
		private BufferedWriter _writer;

		public SyntaxConsole()
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;

			InitializeComponent();
			InitializeKeyMappings();

			_writer = new BufferedWriter(FlushBuffer);

			_processInterface = new ProcessInterface();
			_processInterface.Output += (sender, args) => OnOutput(args);
			_processInterface.Error += (sender, args) => OnError(args);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_processInterface.Stop(false);
				ObjectHelper.Dispose(ref _processInterface);
				ObjectHelper.Dispose(ref _writer);
			}

			try
			{
				base.Dispose(disposing);
			}
			catch (InvalidOperationException)
			{
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			IsLoading = false;
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			FreeConsole();
		}

		protected new void OnPaintBackground([NotNull] PaintEventArgs e)
		{
			if (InDesignMode)
			{
				e.Graphics.FillRectangle(SystemBrushes.ControlDark, e.ClipRectangle);
				return;
			}
			
			base.OnPaintBackground(e);
		}

		/// <inheritdoc />
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (ProcessedInput && HasInputProcess)
			{
				IEnumerable<KeyMapping> mappings = _keyMappings.Where(k => k == e);

				//  Go through each mapping, send the message.
				//foreach (var mapping in mappings)
				//{
				//SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
				//inputWriter.WriteLine(mapping.StreamMapping);
				//WriteInput("\x3", Color.White, false);
				//}

				// If we handled a mapping, we're done here.
				if (mappings.Any())
				{
					e.SuppressKeyPress = true;
					return;
				}
			}

			// If we're at the input point and it's backspace, bail.
			if (Caret.Offset <= _inputStart && e.KeyCode.IsBackspace())
			{
				e.SuppressKeyPress = true;
				return;
			}

			// Are we in the read-only zone?
			if (Caret.Offset < _inputStart)
			{
				// Allow arrows and Ctrl-C.
				if (!e.KeyCode.IsFunction()
					&& !e.KeyCode.IsNavigation()
					&& !(e.KeyCode == Keys.C && e.Control))
				{
					e.SuppressKeyPress = true;
					return;
				}
			}

			//  Is it the return key?
			if (e.KeyCode.IsEnter())
			{
				//  Get the input.
				Caret.Offset = Text.Length;
				string input = Text.Substring(_inputStart, Caret.Offset - _inputStart);
				//  Write the input (without echoing).
				WriteToProcess(input);
				e.SuppressKeyPress = true;
				return;
			}
			base.OnKeyDown(e);
		}

		/// <inheritdoc />
		protected override void OnDocumentSyntaxLanguageLoaded(SyntaxLanguageEventArgs e)
		{
			Document.LanguageData = null;
			base.OnDocumentSyntaxLanguageLoaded(e);
		}

		/// <inheritdoc />
		protected override void OnPasteDragDrop([NotNull] PasteDragDropEventArgs e)
		{
			if (e.DataObject.GetDataPresent(DataFormats.FileDrop))
			{
				if (e.DataObject.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
				{
					string fileName = files[0];

					if (e.Source == PasteDragDropSource.DragDrop && Path.GetExtension(fileName).IsSame(".snippet"))
					{
						CodeSnippet[] codeSnippets = CodeSnippet.LoadFromXml(fileName);

						if (codeSnippets.Length > 0)
						{
							e.Text = string.Empty;
							Caret.Offset = SelectedView.LocationToOffset(PointToClient(new Point(e.DragEventArgs.X, e.DragEventArgs.Y)), LocationToPositionAlgorithm.BestFit);
							IntelliPrompt.CodeSnippets.Activate(codeSnippets[0]);
							return;
						}
					}

					e.Text = fileName;
				}
			}

			// If a paste occurred, show a smart tag for more options
			if (e.Source == PasteDragDropSource.PasteComplete && Caret.Offset > 0)
			{
				IntelliPrompt.SmartTag.Add(new SmartTag("Paste", null, "Options for <b>post-paste</b> operations."),
											new TextRange(Math.Max(0, Caret.Offset - e.Text.Length), Caret.Offset));
			}

			base.OnPasteDragDrop(e);
		}

		public event EventHandler<string> Output;
		public event EventHandler<string> Error;
		public event EventHandler<(string Name, DateTime? ExitTime, int? ExitCode)> Exit;

		[Category(CAT_BEHAVIOUR)]
		public bool InputEnabled { get; set; } = true;

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"Characters read by the ReadFile or ReadConsole function are written to the active screen buffer as they are read. This mode can be used only if the LineInput mode is also enabled.")]
		public bool EchoInput { get; set; } = true;

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"The ReadFile or ReadConsole function returns only when a carriage return character is read. If this mode is disabled, the functions return when one or more characters are available.")]
		public bool LineInput { get; set; } = true;

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"When enabled, text entered in a console window will be inserted at the current cursor location and all text following that location will not be overwritten. When disabled, all following text will be overwritten.")]
		public bool InsertMode { get; set; } = true;

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"If the mouse pointer is within the borders of the console window and the window has the keyboard focus, mouse events generated by mouse movement and button presses are placed in the input buffer. These events are discarded by ReadFile or ReadConsole, even when this mode is enabled.")]
		public bool MouseInput { get; set; } = true;

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"User interactions that change the size of the console screen buffer are reported in the console's input buffer. Information about these events can be read from the input buffer by applications using the ReadConsoleInput function, but not by those using ReadFile or ReadConsole.")]
		public bool WindowInput { get; set; }

		[Category(CAT_BEHAVIOUR)]
		[Description(
			"CTRL+C is processed by the system and is not placed in the input buffer. If the input buffer is being read by ReadFile or ReadConsole, other control keys are processed by the system and are not returned in the ReadFile or ReadConsole buffer. If the LineInput mode is also enabled, backspace, carriage return, and line feed characters are handled by the global::System.")]
		public bool ProcessedInput { get; set; }

		[Category(CAT_MODE)]
		[Description("This flag enables the user to use the mouse to select and edit text.")]
		public bool QuickEdit { get; set; } = true;

		[Browsable(false)]
		public int BufferSize
		{
			get => _writer.BufferSize;
			set => _writer.BufferSize = value;
		}

		[Browsable(false)]
		public ProcessInterface Interface => _processInterface;

		[Browsable(false)]
		public bool HasInputProcess => _processInterface.HasInputProcess;

		protected bool InDesignMode { get; }
		protected bool IsLoading { get; set; }
		protected bool ConsoleCreated { get; private set; }
		protected bool ConsoleAttached { get; private set; }

		public bool CaptureConsole()
		{
			if (InDesignMode || ConsoleAttached) return true;

			try
			{
				if (ConsoleHelper.AttachConsole(Handle, out bool consoleCreated, false))
				{
					ConsoleCreated = consoleCreated;
					ConsoleAttached = true;
					Console.InputEncoding = EncodingHelper.Default;
					Console.SetOut(_writer);
					Console.SetError(_writer);
					UpdateMode();
					return true;
				}
			}
			catch
			{
				FreeConsole();
			}

			return false;
		}

		public void FreeConsole()
		{
			if (InDesignMode || !ConsoleCreated) return;
			ConsoleHelper.FreeConsole();
			ConsoleCreated = ConsoleAttached = false;
		}

		public bool StartShell() { return StartShell(null); }

		public bool StartShell(RunSettingsBase settings) { return StartInputProcess("cmd.exe", null, settings); }

		public bool StartInputProcess([NotNull] string fileName) { return StartInputProcess(fileName, null, null); }

		public bool StartInputProcess([NotNull] string fileName, string arguments) { return StartInputProcess(fileName, arguments, null); }

		public bool StartInputProcess([NotNull] string fileName, RunSettingsBase settings) { return StartInputProcess(fileName, null, settings); }

		public bool StartInputProcess([NotNull] string fileName, string arguments, RunSettingsBase settings)
		{
			bool result = _processInterface.StartInputProcess(fileName, arguments, settings);
			if (!result) return false;
			this.InvokeIf(() =>
			{
				if (!InputEnabled) return;
				Document.ReadOnly = false;
			});
			return true;
		}

		public void Stop() { Stop(true); }

		public void Stop(bool waitForProcess)
		{
			_processInterface.Stop(waitForProcess);
		}

		public bool WriteToProcess(string value)
		{
			if (!_processInterface.WriteInput(value)) return false;
			this.InvokeIf(() =>
			{
				_inputStart = Caret.Offset + 1;
				_skipStart = value.Length;
			});
			return true;
		}

		public bool WriteInput(string value)
		{
			if (!WriteToProcess(value)) return false;
			_writer.Write(value);
			return true;
		}

		public void Write(char value) { _writer.Write(value); }

		public void Write(string value) { _writer.Write(value); }

		public void WriteLine(string value) { _writer.Write(value); }

		public void WriteError(string value) { Write(value); }
		public void WriteErrorLine(string value) { WriteLine(value); }

		public void Clear()
		{
			this.InvokeIf(() =>
			{
				_writer.Clear();
				Document.Text = string.Empty;
				_inputStart = 0;
				_skipStart = 0;
			});
		}

		public void UpdateMode()
		{
			Win32.ConsoleModesEnum add = Win32.ConsoleModesEnum.NONE;
			Win32.ConsoleModesEnum remove = Win32.ConsoleModesEnum.NONE;

			if (EchoInput) add |= Win32.ConsoleModesEnum.ENABLE_ECHO_INPUT;
			else remove |= Win32.ConsoleModesEnum.ENABLE_ECHO_INPUT;

			if (LineInput) add |= Win32.ConsoleModesEnum.ENABLE_LINE_INPUT;
			else remove |= Win32.ConsoleModesEnum.ENABLE_LINE_INPUT;

			if (InsertMode) add |= Win32.ConsoleModesEnum.ENABLE_INSERT_MODE;
			else remove |= Win32.ConsoleModesEnum.ENABLE_INSERT_MODE;

			if (MouseInput) add |= Win32.ConsoleModesEnum.ENABLE_MOUSE_INPUT;
			else remove |= Win32.ConsoleModesEnum.ENABLE_MOUSE_INPUT;

			if (WindowInput) add |= Win32.ConsoleModesEnum.ENABLE_WINDOW_INPUT;
			else remove |= Win32.ConsoleModesEnum.ENABLE_WINDOW_INPUT;

			if (ProcessedInput) add |= Win32.ConsoleModesEnum.ENABLE_PROCESSED_INPUT;
			else remove |= Win32.ConsoleModesEnum.ENABLE_PROCESSED_INPUT;

			if (QuickEdit) add |= Win32.ConsoleModesEnum.ENABLE_QUICK_EDIT_MODE;
			else remove |= Win32.ConsoleModesEnum.ENABLE_QUICK_EDIT_MODE;

			ConsoleHelper.SetMode(remove, add);
		}

		protected virtual void OnOutput(string e)
		{
			this.InvokeIf(() =>
			{
				if (_skipStart > 0)
				{
					if (e != null && e.Length >= _skipStart)
					{
						int n = Math.Min(_skipStart, e.Length);
						_skipStart -= n;
						e = e.Right(e.Length - n);
					}
				}

				Write(e);
				Output?.Invoke(this, e);
			});
		}

		protected virtual void OnError(string e)
		{
			this.InvokeIf(() =>
			{
				WriteError(e);
				Error?.Invoke(this, e);
			});
		}

		protected virtual void OnExit((string Name, DateTime? ExitTime, int? ExitCode) e)
		{
			this.InvokeIf(() =>
			{
				Document.ReadOnly = true;
				Exit?.Invoke(this, e);
			});
		}

		protected void FlushBuffer(string value)
		{
			this.InvokeIf(() =>
			{
				Document.AppendText(value);
				_inputStart = Caret.Offset;
			});
		}

		private void InitializeComponent()
		{
			SuspendLayout();
			AllowDrop = true;
			AcceptsTab = true;
			Dock = DockStyle.Fill;
			HideSelection = false;
			Font = new Font("Consolas", 11F);
			ForeColor = Console.ForegroundColor.ToColor();
			BackColor = Console.BackgroundColor.ToColor();
			VirtualSpaceAtLineEndEnabled = false;
			VirtualSpaceAtDocumentEndEnabled = false;
			CurrentLineHighlightingVisible = true;
			CutCopyBlankLineWhenNoSelection = false;
			WordWrap = WordWrapType.Character;
			MoveCaretToPreviousLineAtLineStart = true;
			MoveCaretToNextLineAtLineEnd = true;
			BracketHighlightingInclusive = false;
			BracketHighlightingVisible = false;
			ContentDividersVisible = false;
			LineNumberMarginVisible = false;
			IndentType = IndentType.None;
			IndentationGuidesVisible = false;
			IndentationGuidesVisible = false;
			IndicatorMarginVisible = false;
			UserMarginVisible = false;
			ScrollBarType = ScrollBarType.Vertical;
			ScrollPastDocumentEnd = false;
			SelectionModesEnabled = SelectionModes.ContinuousStream;
			SplitType = SyntaxEditorSplitType.DualHorizontal;

			IntelliPrompt.DropShadowEnabled = true;
			IntelliPrompt.SmartTag.ClearOnDocumentModification = true;
			IntelliPrompt.SmartTag.MultipleSmartTagsEnabled = false;

			Document = new Document
			{
				AutoCaseCorrectEnabled = false,
				AutoCaseCorrectOnlyOnModification = true,
				AutoCharacterCasing = CharacterCasing.Normal,
				AutoConvertTabsToSpaces = false,
				AutoTrimTrailingWhitespaceOnPaste = true,
				Filename = string.Empty,
				LineModificationMarkingEnabled = false,
				SemanticParsingEnabled = false,
				//LexicalParsingEnabled = false,
				Multiline = true,
				TabSize = 4,
				ReadOnly = true,
				Outlining =
				{
					Mode = OutliningMode.None
				}
				//UndoRedo =
				//{
				//	UndoStack =
				//	{
				//		Capacity = 0
				//	},
				//	RedoStack =
				//	{
				//		Capacity = 0
				//	}
				//}
			};

			LoadLanguageDefinition();

			ResumeLayout(false);
		}

		private void InitializeKeyMappings()
		{
			//  Map 'tab'.
			_keyMappings.Add(new KeyMapping(Keys.Tab, "{TAB}", "\t"));

			//  Map 'Ctrl-C'.
			_keyMappings.Add(new KeyMapping(Keys.C, "^(c)", "\x03\r\n"));
		}

		private void LoadLanguageDefinition()
		{
			// DON'T use the extension method because the asm assembly might not be available yet at design time
			using (MemoryStream stream = new MemoryStream())
			{
				using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode, StreamHelper.BUFFER_DEFAULT, true))
				{
					writer.Write(Resources.LexerBatchFile);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					Document.LoadLanguageFromXml(stream, 0);
				}
			}
		}
	}
}