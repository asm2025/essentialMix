using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using essentialMix.Threading.Helpers;

namespace essentialMix.Threading
{
	public sealed class ProcessInterface : Disposable
	{
		private ManualResetEventSlim _manualResetEventSlim;
		private Process _inputProcess;
		private StreamWriter _input;
		private AsyncStreamReader _output;
		private AsyncStreamReader _error;
		private CancellationTokenSource _cts;
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private CancellationToken _token;

		/// <inheritdoc />
		public ProcessInterface(CancellationToken token = default(CancellationToken))
		{
			_token = token;

			DefaultRunSettings = RunSettingsBase.Default;
			DefaultRunSettings.CreateNoWindow = true;
			DefaultRunSettings.RedirectInput = true;
			DefaultRunSettings.RedirectOutput = true;
			DefaultRunSettings.RedirectError = true;

			_manualResetEventSlim = new ManualResetEventSlim(false);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop(WaitForProcess);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
			}
			base.Dispose(disposing);
		}

		public event EventHandler<string> Output;
		public event EventHandler<string> Error;
		public event EventHandler<(string Name, DateTime? ExitTime, int? ExitCode)> Exit;

		public bool WaitForProcess { get; set; } = true;
		public CancellationToken Token { get; private set; }
		public RunSettingsBase DefaultRunSettings { get; set; }
		public string FileName { get; private set; }
		public string Arguments { get; private set; }
		public Exception LastException { get; private set; }
		public Process InputProcess => _inputProcess;

		public bool HasInputProcess => _inputProcess.IsAwaitable();

		public bool WriteInput(string input)
		{
			if (!HasInputProcess || input == null) return false;
			/*
			* Bug: Microsoft!!
			* Neither the InputStream nor its BaseStream flush their buffer
			* The InputStream pos is stuck at 0 and its BaseStream is at the length
			* of the written input and there is no output from their console
			* cmd.exe! Why?! even after trying to close the stream, or make another one
			* from it.
			*
			* edit: I got it to work with a workaround
			*/
			_input.WriteLine(input);
			return true;
		}

		public bool StartInputProcess([NotNull] string fileName) { return StartInputProcess(fileName, null, null); }

		public bool StartInputProcess([NotNull] string fileName, string arguments) { return StartInputProcess(fileName, arguments, null); }

		public bool StartInputProcess([NotNull] string fileName, RunSettingsBase settings) { return StartInputProcess(fileName, null, settings); }

		public bool StartInputProcess([NotNull] string fileName, string arguments, RunSettingsBase settings)
		{
			if (IsDisposed || _token.IsCancellationRequested) return false;
			LastException = null;
			settings ??= DefaultRunSettings;

			if (_inputProcess.IsAwaitable())
			{
				ObjectHelper.Dispose(ref _input);
				ObjectHelper.Dispose(ref _output);
				ObjectHelper.Dispose(ref _error);
				_inputProcess.Die();
				ObjectHelper.Dispose(ref _inputProcess);
			}

			_manualResetEventSlim.Reset();
			_inputProcess = ProcessHelper.CreateForRunCore(fileName, arguments, settings);
			if (_inputProcess == null) return false;
			_inputProcess.EnableRaisingEvents = true;

			ProcessStartInfo startInfo = _inputProcess.StartInfo;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.StandardOutputEncoding = EncodingHelper.Default;
			startInfo.StandardErrorEncoding = EncodingHelper.Default;

			if (_cts == null)
			{
				//This is for the wait function, it's not used here
				_cts = new CancellationTokenSource();
				Token = _cts.Token;
				if (_token.CanBeCanceled) _token.Register(() => _cts.CancelIfNotDisposed(), false);
			}

			_inputProcess.Exited += (sender, _) =>
			{
				Process p = (Process)sender;
				DateTime? exitTime = null;
				int? exitCode = null;

				if (p.IsAssociated())
				{
					try
					{
						exitTime = p.ExitTime;
						exitCode = p.ExitCode;
					}
					catch
					{
						// ignored
					}
				}				

				OnExit(fileName, exitTime, exitCode);
				ObjectHelper.Dispose(ref _inputProcess);
				ObjectHelper.Dispose(ref _cts);
				FileName = null;
				Arguments = null;
				LastException = null;
				_manualResetEventSlim.Set();
			};

			bool result;

			try
			{
				result = _inputProcess.Start();
				if (!result) return false;
				ProcessJob.AddProcess(settings.JobHandle, _inputProcess);
				_output = new AsyncStreamReader(_inputProcess, _inputProcess.StandardOutput.BaseStream, OnOutput, _inputProcess.StandardOutput.CurrentEncoding);
				_output.BeginRead();
				_error = new AsyncStreamReader(_inputProcess, _inputProcess.StandardError.BaseStream, OnError, _inputProcess.StandardError.CurrentEncoding);
				_error.BeginRead();
				_input = _inputProcess.StandardInput;
				_input.AutoFlush = true;
				FileName = fileName;
				Arguments = arguments;
			}
			catch (Exception e)
			{
				LastException = e;
				_inputProcess.Die();
				result = false;
			}

			return result;
		}

		public void Stop() { Stop(WaitForProcess); }
		public void Stop(bool waitForProcess)
		{
			Cancel();
			if (waitForProcess) Wait(TimeSpanHelper.INFINITE);
			ObjectHelper.Dispose(ref _input);
			ObjectHelper.Dispose(ref _output);
			ObjectHelper.Dispose(ref _error);
			_inputProcess.Die();
			ObjectHelper.Dispose(ref _inputProcess);
			ObjectHelper.Dispose(ref _cts);
			FileName = null;
			Arguments = null;
			LastException = null;
		}

		public Process Run([NotNull] string execName) { return Run(execName, null, RunSettings.Default); }

		public Process Run([NotNull] string execName, [NotNull] RunSettings settings) { return Run(execName, null, settings); }

		public Process Run([NotNull] string execName, string arguments, [NotNull] RunSettings settings) { return ProcessHelper.Run(execName, arguments, settings); }

		public bool RunAndWaitFor([NotNull] string execName, CancellationToken token = default(CancellationToken)) { return RunAndWaitFor(execName, null, null, token); }

		public bool RunAndWaitFor([NotNull] string execName, RunAndWaitForSettings settings, CancellationToken token = default(CancellationToken)) { return RunAndWaitFor(execName, null, settings, token); }

		public bool RunAndWaitFor([NotNull] string execName, string arguments, CancellationToken token = default(CancellationToken)) { return RunAndWaitFor(execName, arguments, null, token); }

		public bool RunAndWaitFor([NotNull] string execName, string arguments, RunAndWaitForSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return RunAndWaitFor(execName, arguments, settings, token.WaitHandle);
		}

		public bool RunAndWaitFor([NotNull] string execName, WaitHandle awaitableHandle) { return RunAndWaitFor(execName, null, null, awaitableHandle); }

		public bool RunAndWaitFor([NotNull] string execName, RunAndWaitForSettings settings, WaitHandle awaitableHandle)
		{
			return RunAndWaitFor(execName, null, settings, awaitableHandle);
		}

		public bool RunAndWaitFor([NotNull] string execName, string arguments, WaitHandle awaitableHandle) { return RunAndWaitFor(execName, arguments, null, awaitableHandle); }

		public bool RunAndWaitFor([NotNull] string execName, string arguments, RunAndWaitForSettings settings, WaitHandle awaitableHandle)
		{
			return ProcessHelper.RunAndWaitFor(execName, arguments, settings, awaitableHandle);
		}

		public RunOutput RunAndGetOutput([NotNull] string execName, CancellationToken token = default(CancellationToken)) { return RunAndGetOutput(execName, null, null, token); }

		public RunOutput RunAndGetOutput([NotNull] string execName, RunSettingsBase settings, CancellationToken token = default(CancellationToken)) { return RunAndGetOutput(execName, null, settings, token); }

		public RunOutput RunAndGetOutput([NotNull] string execName, string arguments, CancellationToken token = default(CancellationToken)) { return RunAndGetOutput(execName, arguments, null, token); }

		public RunOutput RunAndGetOutput([NotNull] string execName, string arguments, RunSettingsBase settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return RunAndGetOutput(execName, arguments, settings, token.WaitHandle);
		}

		public RunOutput RunAndGetOutput([NotNull] string execName, WaitHandle awaitableHandle) { return RunAndGetOutput(execName, null, null, awaitableHandle); }

		public RunOutput RunAndGetOutput([NotNull] string execName, RunSettingsBase settings, WaitHandle awaitableHandle)
		{
			return RunAndGetOutput(execName, null, settings, awaitableHandle);
		}

		public RunOutput RunAndGetOutput([NotNull] string execName, string arguments, WaitHandle awaitableHandle)
		{
			return RunAndGetOutput(execName, arguments, null, awaitableHandle);
		}

		public RunOutput RunAndGetOutput([NotNull] string execName, string arguments, RunSettingsBase settings, WaitHandle awaitableHandle)
		{
			return ProcessHelper.RunAndGetOutput(execName, arguments, settings, awaitableHandle);
		}

		[NotNull]
		public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull]
		public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return !_inputProcess.IsAwaitable()
				? Task.FromResult(true) 
				: Task.Run(() => Wait(millisecondsTimeout), _token);
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE); }

		public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds()); }

		public bool Wait(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!_inputProcess.IsAwaitable()) return true;

			bool result;

			try
			{
				result = millisecondsTimeout > TimeSpanHelper.INFINITE
					? _inputProcess.WaitForExit(millisecondsTimeout, _cts.Token)
					: _inputProcess.WaitForExit(_cts.Token);
			}
			catch (OperationCanceledException)
			{
				result = false;
			}
			catch (TimeoutException)
			{
				result = false;
			}

			return result;
		}

		private void Cancel() { _cts?.Cancel(); }

		private void OnOutput(string e)
		{
			Output?.Invoke(this, e);
		}

		private void OnError(string e)
		{
			Error?.Invoke(this, e);
		}

		private void OnExit(string name, DateTime? exitTime, int? exitCode)
		{
			Exit?.Invoke(this, (name, exitTime, exitCode));
		}
	}
}