using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using MSRegistry = Microsoft.Win32.Registry;

namespace essentialMix.Threading.Helpers;

[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true, SharedState = true, Synchronization = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public static class ProcessHelper
{
	private const ShellExecuteMaskFlagsEnum SHELL_BASE_FLAGS =
		ShellExecuteMaskFlagsEnum.SEE_MASK_NOCLOSEPROCESS |
		ShellExecuteMaskFlagsEnum.SEE_MASK_FLAG_DDEWAIT |
		ShellExecuteMaskFlagsEnum.SEE_MASK_INVOKEIDLIST |
		ShellExecuteMaskFlagsEnum.SEE_MASK_NOASYNC |
		ShellExecuteMaskFlagsEnum.SEE_MASK_DOENVSUBST |
		ShellExecuteMaskFlagsEnum.SEE_MASK_FLAG_LOG_USAGE;

	private const string REG_DLL_CMD = "regsvr32";
	private const string FMT_REG_DLL = "\"{0}\" /s";
	private const string FMT_UNREG_DLL = "/u \"{0}\" /s";

	public static readonly Process Empty = new Process();

	[NotNull]
	public static string[] GetVerbs([NotNull] string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

		string extension = Path.GetExtension(fileName);
		if (string.IsNullOrEmpty(extension)) return Array.Empty<string>();

		ArrayList arrayList = new ArrayList();
		RegistryKey registryKey = null;

		try
		{
			registryKey = MSRegistry.ClassesRoot.OpenSubKey(extension);

			if (registryKey != null)
			{
				string str = (string)registryKey.GetValue(string.Empty);
				registryKey.Close();
				registryKey = MSRegistry.ClassesRoot.OpenSubKey(str + "\\shell");

				if (registryKey != null)
				{
					string[] subKeyNames = registryKey.GetSubKeyNames();

					foreach (string subKey in subKeyNames)
					{
						if (subKey.IsSame("new")) continue;
						arrayList.Add(subKey);
					}

					registryKey.Close();
					registryKey = null;
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}

		string[] strArray = new string[arrayList.Count];
		arrayList.CopyTo(strArray, 0);
		return strArray;
	}

	public static Process ShellExec([NotNull] string fileName) { return ShellExec(fileName, null, null); }

	public static Process ShellExec([NotNull] string fileName, ShellSettings settings) { return ShellExec(fileName, null, settings); }

	public static Process ShellExec([NotNull] string fileName, string arguments, ShellSettings settings)
	{
		settings ??= ShellSettings.Default;
		SHELLEXECUTEINFO info = GetShellExecuteInfo(fileName, arguments, settings);
		Process p = InternalShellExec(info);
		if (p != null && !settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, p);
		return p;
	}

	public static Process ShellExec(IntPtr lpIDList) { return ShellExec(lpIDList, null); }

	public static Process ShellExec(IntPtr lpIDList, ShellSettings settings)
	{
		if (lpIDList.IsZero()) throw new ArgumentNullException(nameof(lpIDList));
		settings ??= ShellSettings.Default;
		SHELLEXECUTEINFO info = GetShellExecuteInfo(settings);
		info.lpIDList = lpIDList;
		info.fMask |= (int)ShellExecuteMaskFlagsEnum.SEE_MASK_IDLIST;
		return InternalShellExec(info);
	}

	[NotNull]
	public static Task<bool> ShellExecAndWaitForAsync([NotNull] string execName, CancellationToken token = default(CancellationToken)) { return ShellExecAndWaitForAsync(execName, null, null, token); }
	[NotNull]
	public static Task<bool> ShellExecAndWaitForAsync([NotNull] string execName, ShellSettings settings, CancellationToken token = default(CancellationToken)) { return ShellExecAndWaitForAsync(execName, null, settings, token); }
	[NotNull]
	public static Task<bool> ShellExecAndWaitForAsync([NotNull] string execName, string arguments, CancellationToken token = default(CancellationToken)) { return ShellExecAndWaitForAsync(execName, arguments, null, token); }
	[NotNull]
	public static Task<bool> ShellExecAndWaitForAsync([NotNull] string execName, string arguments, ShellSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		execName = execName.Trim();
		if (string.IsNullOrEmpty(execName)) throw new ArgumentNullException(nameof(execName));
		settings ??= ShellSettings.Default;

		SHELLEXECUTEINFO info = GetShellExecuteInfo(execName, arguments, settings);
		Process process = null;
		SafeWaitHandle waitHandle;
		ManualResetEvent processFinishedEvent = null;

		try
		{
			process = InternalShellExec(info);
			if (process == null) return Task.FromResult(false);
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);

			if (!token.CanBeCanceled)
			{
				process.WaitForExit();
				return Task.FromResult(true);
			}

			waitHandle = new SafeWaitHandle(process.Handle, false);
			if (!waitHandle.IsAwaitable()) return Task.FromResult(false);
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			// copy to local variable
			Process proc = process;
			return TaskHelper.FromWaitHandle(processFinishedEvent, token)
							.ConfigureAwait()
							.ContinueWith(t =>
							{
								int ndx = t.IsCompleted && t.Result
											? 0
											: -1;
								proc.Die();
								ObjectHelper.Dispose(ref proc);
								processFinishedEvent?.Close();
								ObjectHelper.Dispose(ref processFinishedEvent);
								waitHandle?.Close();
								ObjectHelper.Dispose(ref waitHandle);
								return ndx == 0;
							}, token, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
		}
		finally
		{
			if (processFinishedEvent == null) ObjectHelper.Dispose(ref process);
		}
	}

	public static bool ShellExecAndWaitFor([NotNull] string fileName, WaitHandle awaitableHandle) { return ShellExecAndWaitFor(fileName, null, null, awaitableHandle); }

	public static bool ShellExecAndWaitFor([NotNull] string fileName, ShellSettings settings, WaitHandle awaitableHandle)
	{
		return ShellExecAndWaitFor(fileName, null, settings, awaitableHandle);
	}

	public static bool ShellExecAndWaitFor([NotNull] string fileName, string arguments, WaitHandle awaitableHandle)
	{
		return ShellExecAndWaitFor(fileName, arguments, null, awaitableHandle);
	}

	public static bool ShellExecAndWaitFor([NotNull] string fileName, string arguments, ShellSettings settings, WaitHandle awaitableHandle)
	{
		fileName = fileName.Trim();
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		settings ??= ShellSettings.Default;

		SHELLEXECUTEINFO info = GetShellExecuteInfo(fileName, arguments, settings);
		Process process = null;
		SafeWaitHandle waitHandle = null;
		ManualResetEvent processFinishedEvent = null;

		try
		{
			process = InternalShellExec(info);
			if (process == null) return false;
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);

			if (!awaitableHandle.IsAwaitable())
			{
				process.WaitForExit();
				return true;
			}

			waitHandle = new SafeWaitHandle(process.Handle, false);
			if (!waitHandle.IsAwaitable()) return false;
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			if (!awaitableHandle.IsAwaitable()) return false;

			WaitHandle[] waitHandles =
			{
				processFinishedEvent,
				awaitableHandle
			};

			int ndx = waitHandles.WaitAny();
			if (ndx != 0) return false;
			process.Die();
			return true;
		}
		finally
		{
			ObjectHelper.Dispose(ref process);
			processFinishedEvent?.Close();
			ObjectHelper.Dispose(ref processFinishedEvent);
			waitHandle?.Close();
			ObjectHelper.Dispose(ref waitHandle);
		}
	}

	public static Process Run([NotNull] string execName) { return Run(execName, null, RunSettings.Default); }

	public static Process Run([NotNull] string execName, [NotNull] RunSettings settings) { return Run(execName, null, settings); }

	public static Process Run([NotNull] string execName, string arguments, [NotNull] RunSettings settings)
	{
		Process process = CreateForRun(execName, arguments, settings, out bool redirectOutput, out bool redirectError);

		bool result = false;

		try
		{
			result = process.Start();
			if (!result) return null;
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);

			if (redirectOutput) process.BeginOutputReadLine();
			if (redirectError) process.BeginErrorReadLine();

			settings.OnStart?.Invoke(execName, process.StartTime);
			return process;
		}
		catch (Win32Exception e)
		{
			throw new InvalidOperationException(e.CollectMessages(), e);
		}
		finally
		{
			if (!result) ObjectHelper.Dispose(ref process);
		}
	}

	[NotNull]
	public static Task<bool> RunAndWaitForAsync([NotNull] string execName, CancellationToken token = default(CancellationToken)) { return RunAndWaitForAsync(execName, null, null, token); }
	[NotNull]
	public static Task<bool> RunAndWaitForAsync([NotNull] string execName, RunAndWaitForSettings settings, CancellationToken token = default(CancellationToken)) { return RunAndWaitForAsync(execName, null, settings, token); }
	[NotNull]
	public static Task<bool> RunAndWaitForAsync([NotNull] string execName, string arguments, CancellationToken token = default(CancellationToken)) { return RunAndWaitForAsync(execName, arguments, null, token); }
	[NotNull]
	public static Task<bool> RunAndWaitForAsync([NotNull] string execName, string arguments, RunAndWaitForSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= RunAndWaitForSettings.Default;
		if (settings.OnOutput != null) settings.RedirectOutput = true;
		if (settings.OnError != null) settings.RedirectError = true;

		Process process = null;
		ManualResetEvent processFinishedEvent = null;

		try
		{
			process = CreateForRun(execName, arguments, settings);

			if (settings.RedirectOutput)
			{
				process.OutputDataReceived += (_, args) =>
				{
					if (args.Data == null) return;
					settings.OnOutput?.Invoke(args.Data);
				};
			}

			if (settings.RedirectError)
			{
				process.ErrorDataReceived += (_, args) =>
				{
					if (args.Data == null) return;
					settings.OnError?.Invoke(args.Data);
				};
			}

			process.Exited += (sender, _) =>
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

				settings.OnExit?.Invoke(execName, exitTime, exitCode);
			};

			bool result = process.Start();
			if (!result) return Task.FromResult(false);
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);

			if (settings.RedirectOutput && settings.OnOutput != null)
			{
				AsyncStreamReader output = new AsyncStreamReader(process, process.StandardOutput.BaseStream, settings.OnOutput, process.StandardOutput.CurrentEncoding);
				output.BeginRead();
			}

			if (settings.RedirectError && settings.OnError != null)
			{
				AsyncStreamReader error = new AsyncStreamReader(process, process.StandardError.BaseStream, settings.OnError, process.StandardError.CurrentEncoding);
				error.BeginRead();
			}

			settings.OnStart?.Invoke(execName, process.StartTime);

			if (!token.CanBeCanceled)
			{
				process.WaitForExit();
				return Task.FromResult(true);
			}

			SafeWaitHandle waitHandle = new SafeWaitHandle(process.Handle, false);
			if (!waitHandle.IsAwaitable()) return Task.FromResult(false);
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			// copy to local variable
			Process proc = process;
			return TaskHelper.FromWaitHandle(processFinishedEvent, token)
							.ConfigureAwait()
							.ContinueWith(t =>
							{
								int ndx = t.IsCompleted && t.Result
											? 0
											: -1;
								proc.Die();
								ObjectHelper.Dispose(ref proc);
								processFinishedEvent?.Close();
								ObjectHelper.Dispose(ref processFinishedEvent);
								waitHandle?.Close();
								ObjectHelper.Dispose(ref waitHandle);
								return ndx == 0;
							}, token, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
		}
		catch (Win32Exception e)
		{
			throw new InvalidOperationException(e.CollectMessages(), e);
		}
		finally
		{
			if (processFinishedEvent == null) ObjectHelper.Dispose(ref process);
		}
	}

	public static bool RunAndWaitFor([NotNull] string execName, WaitHandle awaitableHandle) { return RunAndWaitFor(execName, null, null, awaitableHandle); }

	public static bool RunAndWaitFor([NotNull] string execName, RunAndWaitForSettings settings, WaitHandle awaitableHandle)
	{
		return RunAndWaitFor(execName, null, settings, awaitableHandle);
	}

	public static bool RunAndWaitFor([NotNull] string execName, string arguments, WaitHandle awaitableHandle)
	{
		return RunAndWaitFor(execName, arguments, null, awaitableHandle);
	}

	public static bool RunAndWaitFor([NotNull] string execName, string arguments, RunAndWaitForSettings settings, WaitHandle awaitableHandle)
	{
		settings ??= RunAndWaitForSettings.Default;
		if (settings.OnOutput != null) settings.RedirectOutput = true;
		if (settings.OnError != null) settings.RedirectError = true;

		Process process = null;

		try
		{
			process = CreateForRun(execName, arguments, settings);

			if (settings.RedirectOutput)
			{
				process.OutputDataReceived += (_, args) =>
				{
					if (args.Data == null) return;
					settings.OnOutput?.Invoke(args.Data);
				};
			}

			if (settings.RedirectError)
			{
				process.ErrorDataReceived += (_, args) =>
				{
					if (args.Data == null) return;
					settings.OnError?.Invoke(args.Data);
				};
			}

			process.Exited += OnProcExit;

			bool result = process.Start();
			if (!result) return false;
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);

			if (settings.RedirectOutput && settings.OnOutput != null)
			{
				AsyncStreamReader output = new AsyncStreamReader(process, process.StandardOutput.BaseStream, settings.OnOutput, process.StandardOutput.CurrentEncoding);
				output.BeginRead();
			}

			if (settings.RedirectError && settings.OnError != null)
			{
				AsyncStreamReader error = new AsyncStreamReader(process, process.StandardError.BaseStream, settings.OnError, process.StandardError.CurrentEncoding);
				error.BeginRead();
			}

			settings.OnStart?.Invoke(execName, process.StartTime);

			if (!awaitableHandle.IsAwaitable())
			{
				process.WaitForExit();
				return true;
			}

			SafeWaitHandle waitHandle = null;
			ManualResetEvent processFinishedEvent = null;

			try
			{
				waitHandle = new SafeWaitHandle(process.Handle, false);
				if (!waitHandle.IsAwaitable()) return false;
				processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
				if (!awaitableHandle.IsAwaitable()) return false;

				WaitHandle[] waitHandles =
				{
					processFinishedEvent,
					awaitableHandle
				};

				int ndx = waitHandles.WaitAny();
				if (ndx != 0) return false;
				process.Die();
				return true;
			}
			finally
			{
				processFinishedEvent?.Close();
				ObjectHelper.Dispose(ref processFinishedEvent);
				waitHandle?.Close();
				ObjectHelper.Dispose(ref waitHandle);
			}
		}
		catch (Win32Exception e)
		{
			throw new InvalidOperationException(e.CollectMessages(), e);
		}
		finally
		{
			ObjectHelper.Dispose(ref process);
		}

		void OnProcExit(object s, EventArgs e)
		{
			Process p = (Process)s;
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

			settings.OnExit?.Invoke(execName, exitTime, exitCode);
		}
	}

	[NotNull]
	public static Task<RunOutput> RunAndGetOutputAsync([NotNull] string execName, CancellationToken token = default(CancellationToken)) { return RunAndGetOutputAsync(execName, null, null, token); }
	[NotNull]
	public static Task<RunOutput> RunAndGetOutputAsync([NotNull] string execName, RunSettingsBase settings, CancellationToken token = default(CancellationToken)) { return RunAndGetOutputAsync(execName, null, settings, token); }
	[NotNull]
	public static Task<RunOutput> RunAndGetOutputAsync([NotNull] string execName, string arguments, CancellationToken token = default(CancellationToken)) { return RunAndGetOutputAsync(execName, arguments, null, token); }
	[NotNull]
	public static Task<RunOutput> RunAndGetOutputAsync([NotNull] string execName, string arguments, RunSettingsBase settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= RunSettingsBase.Default;
		settings.RedirectOutput = true;
		settings.RedirectError = true;

		RunOutput output = new RunOutput();
		Process process = null;
		ManualResetEvent processFinishedEvent = null;
		bool processReallyExited = false;

		try
		{
			process = CreateForRun(execName, arguments, settings);
			process.Exited += (sender, _) =>
			{
				Process p = (Process)sender;

				if (p.IsAssociated())
				{
					try
					{
						output.ExitTime = p.ExitTime;
						output.ExitCode = p.ExitCode;
					}
					catch
					{
						// ignored
					}
				}

				processReallyExited = true;
				settings.OnExit?.Invoke(execName, output.ExitTime, output.ExitCode);
			};

			bool result = process.Start();
			if (!result) return Task.FromResult<RunOutput>(null);
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);
			output.StartTime = process.StartTime;
			settings.OnStart?.Invoke(execName, output.StartTime);

			AsyncStreamReader outputReader = new AsyncStreamReader(process, process.StandardOutput.BaseStream, data =>
			{
				if (data == null) return;
				output.Output.Append(data);
				output.OutputBuilder.Append(data);
			}, process.StandardOutput.CurrentEncoding);
			outputReader.BeginRead();

			AsyncStreamReader errorReader = new AsyncStreamReader(process, process.StandardError.BaseStream, data =>
			{
				if (data == null) return;
				output.Error.Append(data);
				output.OutputBuilder.Append(data);
			}, process.StandardOutput.CurrentEncoding);
			errorReader.BeginRead();

			if (!token.CanBeCanceled)
			{
				process.WaitForExit();
				return Task.FromResult(output);
			}

			SafeWaitHandle waitHandle = new SafeWaitHandle(process.Handle, false);
			if (!waitHandle.IsAwaitable()) return Task.FromResult<RunOutput>(null);
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			// copy to local variable
			Process proc = process;
			return TaskHelper.FromWaitHandle(processFinishedEvent, token)
							.ContinueWith(t =>
							{
								int ndx = t.IsCompleted && t.Result
											? 0
											: -1;

								if (!processReallyExited && proc.IsAwaitable())
								{
									if (!proc.WaitForExit(TimeSpanHelper.HALF)) ndx = -1;
								}

								proc.Die();
								ObjectHelper.Dispose(ref proc);
								processFinishedEvent?.Close();
								ObjectHelper.Dispose(ref processFinishedEvent);
								waitHandle?.Close();
								ObjectHelper.Dispose(ref waitHandle);
								return ndx != 0
											? null
											: output;
							}, token, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
		}
		catch (Win32Exception e)
		{
			throw new InvalidOperationException(e.CollectMessages(), e);
		}
		finally
		{
			if (processFinishedEvent == null) ObjectHelper.Dispose(ref process);
		}
	}

	public static RunOutput RunAndGetOutput([NotNull] string execName, WaitHandle awaitableHandle) { return RunAndGetOutput(execName, null, null, awaitableHandle); }

	public static RunOutput RunAndGetOutput([NotNull] string execName, RunSettingsBase settings, WaitHandle awaitableHandle)
	{
		return RunAndGetOutput(execName, null, settings, awaitableHandle);
	}

	public static RunOutput RunAndGetOutput([NotNull] string execName, string arguments, WaitHandle awaitableHandle)
	{
		return RunAndGetOutput(execName, arguments, null, awaitableHandle);
	}

	public static RunOutput RunAndGetOutput([NotNull] string execName, string arguments, RunSettingsBase settings, WaitHandle awaitableHandle)
	{
		settings ??= RunSettingsBase.Default;
		settings.RedirectOutput = true;
		settings.RedirectError = true;

		RunOutput output = new RunOutput();
		Process process = null;
		bool processReallyExited = false;

		try
		{
			process = CreateForRun(execName, arguments, settings);
			process.Exited += (sender, _) =>
			{
				Process p = (Process)sender;

				if (p.IsAssociated())
				{
					try
					{
						output.ExitTime = p.ExitTime;
						output.ExitCode = p.ExitCode;
					}
					catch
					{
						// ignored
					}
				}

				processReallyExited = true;
				settings.OnExit?.Invoke(execName, output.ExitTime, output.ExitCode);
			};

			bool result = process.Start();
			if (!result) return null;
			if (!settings.JobHandle.IsInvalidHandle()) ProcessJob.AddProcess(settings.JobHandle, process);
			output.StartTime = process.StartTime;
			settings.OnStart?.Invoke(execName, output.StartTime);

			AsyncStreamReader outputReader = new AsyncStreamReader(process, process.StandardOutput.BaseStream, data =>
			{
				if (data == null) return;
				output.Output.Append(data);
				output.OutputBuilder.Append(data);
			}, process.StandardOutput.CurrentEncoding);
			outputReader.BeginRead();

			AsyncStreamReader errorReader = new AsyncStreamReader(process, process.StandardError.BaseStream, data =>
			{
				if (data == null) return;
				output.Error.Append(data);
				output.OutputBuilder.Append(data);
			}, process.StandardOutput.CurrentEncoding);
			errorReader.BeginRead();

			if (!awaitableHandle.IsAwaitable())
			{
				process.WaitForExit();
				return output;
			}

			SafeWaitHandle waitHandle = null;
			ManualResetEvent processFinishedEvent = null;

			try
			{
				waitHandle = new SafeWaitHandle(process.Handle, false);
				if (!waitHandle.IsAwaitable()) return null;
				processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
				if (!awaitableHandle.IsAwaitable()) return null;

				WaitHandle[] waitHandles =
				{
					processFinishedEvent,
					awaitableHandle
				};

				int ndx = waitHandles.WaitAny();
				if (ndx != 0) return null;

				if (!processReallyExited && process.IsAwaitable())
				{
					if (!process.WaitForExit(TimeSpanHelper.HALF)) ndx = -1;
				}

				process.Die();
				return ndx != 0
							? null
							: output;
			}
			finally
			{
				processFinishedEvent?.Close();
				ObjectHelper.Dispose(ref processFinishedEvent);
				waitHandle?.Close();
				ObjectHelper.Dispose(ref waitHandle);
			}
		}
		catch (Win32Exception e)
		{
			throw new InvalidOperationException(e.CollectMessages(), e);
		}
		finally
		{
			ObjectHelper.Dispose(ref process);
		}
	}

	[NotNull]
	public static Process CreateForRunCore([NotNull] string execName, string arguments, RunSettingsCore settings)
	{
		execName = execName.Trim();
		if (string.IsNullOrEmpty(execName)) throw new ArgumentNullException(nameof(execName));
		settings ??= RunSettingsCore.Default;

		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = execName,
				Arguments = arguments ?? string.Empty,
				UseShellExecute = false,
				WorkingDirectory = settings.WorkingDirectory,
				CreateNoWindow = settings.CreateNoWindow
			}
		};

		if (settings.WindowStyle.HasValue) process.StartInfo.WindowStyle = settings.WindowStyle.Value;
		if (settings.RunAsAdministrator && Environment.OSVersion.IsWindowsVistaOrHigher()) process.StartInfo.Verb = ShellExecuteVerbs.RunAs;

		if (!settings.ErrorDialogParentHandle.IsInvalidHandle())
		{
			process.StartInfo.ErrorDialog = true;
			process.StartInfo.ErrorDialogParentHandle = settings.ErrorDialogParentHandle;
		}

		if (!settings.UseDefaultCredentials && settings.Credentials != null)
		{
			process.StartInfo.UserName = settings.Credentials.UserName;
			process.StartInfo.Password = settings.Credentials.SecurePassword;
			process.StartInfo.Domain = settings.Credentials.Domain;
			process.StartInfo.LoadUserProfile = settings.LoadUserProfile;
		}

		if (settings.EnvironmentVariables.Count > 0)
			settings.EnvironmentVariables.ForEach(pair => process.StartInfo.EnvironmentVariables.Add(pair.Key, pair.Value));

		IOnProcessCreated processCreated = settings;
		processCreated.OnCreate?.Invoke(process);
		return process;
	}

	[NotNull]
	public static Process CreateForRun([NotNull] string execName) { return CreateForRun(execName, null, RunSettingsBase.Default); }
	[NotNull]
	public static Process CreateForRun([NotNull] string execName, string arguments) { return CreateForRun(execName, arguments, RunSettingsBase.Default); }
	[NotNull]
	public static Process CreateForRun([NotNull] string execName, [NotNull] RunSettingsBase settings) { return CreateForRun(execName, null, settings); }
	[NotNull]
	public static Process CreateForRun([NotNull] string execName, string arguments, [NotNull] RunSettingsBase settings)
	{
		return CreateForRun(execName, arguments, settings, out bool _, out bool _);
	}

	[NotNull]
	public static Process CreateForRun([NotNull] string execName, string arguments, [NotNull] RunSettingsBase settings, out bool redirectOutput, out bool redirectError)
	{
		Process process = CreateForRunCore(execName, arguments, settings);
		IOnProcessStartAndExit processStartAndExit = settings;
		IOnProcessEvents processEvents = settings as IOnProcessEvents;
		redirectOutput = settings.RedirectOutput || processEvents?.OnOutput != null;
		redirectError = settings.RedirectError || processEvents?.OnError != null;
		process.EnableRaisingEvents = settings.RedirectInput || redirectOutput || redirectError || processStartAndExit.OnExit != null;

		ProcessStartInfo startInfo = process.StartInfo;
		startInfo.RedirectStandardInput = settings.RedirectInput;
		startInfo.RedirectStandardOutput = redirectOutput;
		startInfo.RedirectStandardError = redirectError;
		if (startInfo.RedirectStandardOutput) startInfo.StandardOutputEncoding = EncodingHelper.Default;
		if (startInfo.RedirectStandardError) startInfo.StandardErrorEncoding = EncodingHelper.Default;

		if (processEvents != null)
		{
			if (processEvents.OnOutput != null) process.OutputDataReceived += (_, args) => processEvents.OnOutput(args.Data);
			if (processEvents.OnError != null) process.ErrorDataReceived += (_, args) => processEvents.OnError(args.Data);
		}

		if (processStartAndExit.OnExit != null)
		{
			process.Exited += (sender, _) =>
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

				processStartAndExit.OnExit(execName, exitTime, exitCode);
			};
		}

		return process;
	}

	public static bool RegisterDll([NotNull] string fileName)
	{
		if (File.Exists(fileName)) throw new FileNotFoundException("File not found.", fileName);

		Process process = Run(REG_DLL_CMD, string.Format(FMT_REG_DLL, fileName), RunSettings.AsAdminHiddenNoWindow);
		if (process == null) return false;
		process.WaitForExit();
		return process.ExitCode == 0;
	}

	public static bool UnRegisterDll([NotNull] string fileName)
	{
		if (File.Exists(fileName)) throw new FileNotFoundException("File not found.", fileName);

		Process process = Run(REG_DLL_CMD, string.Format(FMT_UNREG_DLL, fileName), RunSettings.AsAdminHiddenNoWindow);
		if (process == null) return false;
		process.WaitForExit();
		return process.ExitCode == 0;
	}

	public static bool TryGetProcessById(IntPtr pid, out Process process) { return TryGetProcessById(pid.ToInt32(), out process); }

	public static bool TryGetProcessById(int pid, out Process process)
	{
		try
		{
			process = Process.GetProcessById(pid);
		}
		catch
		{
			process = null;
		}

		return process != null;
	}

	[NotNull]
	public static Task<bool> WaitForProcessExitAsync([NotNull] Process process, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (!process.IsAwaitable()) return Task.FromResult(false);

		if (!token.CanBeCanceled)
		{
			process.WaitForExit();
			return Task.FromResult(true);
		}

		SafeWaitHandle waitHandle = new SafeWaitHandle(process.Handle, false);
		if (!waitHandle.IsAwaitable()) return Task.FromResult(false);

		bool processReallyExited = false;
		process.Exited += (_, _) => processReallyExited = true;
		ManualResetEvent processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
		return TaskHelper.FromWaitHandle(processFinishedEvent, token)
						.ContinueWith(t =>
						{
							int ndx = t.IsCompleted && t.Result
										? 0
										: -1;

							if (!processReallyExited && process.IsAwaitable())
							{
								if (!process.WaitForExit(TimeSpanHelper.HALF)) ndx = -1;
							}

							processFinishedEvent?.Close();
							ObjectHelper.Dispose(ref processFinishedEvent);
							waitHandle?.Close();
							ObjectHelper.Dispose(ref waitHandle);
							return ndx == 0;
						}, token, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
	}

	[NotNull]
	public static Task<bool> WaitForProcessExitAsync([NotNull] Process process, WaitHandle awaitableHandle)
	{
		return !process.IsAwaitable()
					? Task.FromResult(false)
					: WaitForProcessExitAsync(process.Id, awaitableHandle);
	}

	[NotNull]
	public static Task<bool> WaitForProcessExitAsync(int pid) { return WaitForProcessExitAsync(pid, null); }

	[NotNull]
	public static Task<bool> WaitForProcessExitAsync(int pid, WaitHandle awaitableHandle)
	{
		if (pid == Win32.INVALID_HANDLE_VALUE.ToInt32()) return Task.FromResult(false);

		SafeWaitHandle waitHandle = null;
		ManualResetEvent processFinishedEvent = null;

		try
		{
			Process process = Process.GetProcessById(pid);
			if (!process.IsAwaitable()) return Task.FromResult(false);

			if (!awaitableHandle.IsAwaitable())
			{
				process.WaitForExit();
				return Task.FromResult(true);
			}

			waitHandle = new SafeWaitHandle(process.Handle, false);
			if (!waitHandle.IsAwaitable()) return Task.FromResult(false);
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			if (!awaitableHandle.IsAwaitable()) return Task.FromResult(false);

			WaitHandle[] waitHandles =
			{
				processFinishedEvent,
				awaitableHandle
			};

			bool processReallyExited = false;
			process.Exited += (_, _) => processReallyExited = true;

			int ndx = waitHandles.WaitAny();
			if (ndx != 0) return Task.FromResult(false);

			if (!processReallyExited && process.IsAwaitable())
			{
				if (!process.WaitForExit(TimeSpanHelper.HALF)) ndx = -1;
			}

			return Task.FromResult(ndx == 0);
		}
		finally
		{
			processFinishedEvent?.Close();
			ObjectHelper.Dispose(ref processFinishedEvent);
			waitHandle?.Close();
			ObjectHelper.Dispose(ref waitHandle);
		}
	}

	[NotNull]
	private static SHELLEXECUTEINFO GetShellExecuteInfo(string fileName, string arguments, [NotNull] ShellSettings settings)
	{
		fileName = PathHelper.Trim(fileName);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		SHELLEXECUTEINFO info = GetShellExecuteInfo(settings);
		info.lpFile = fileName;
		if (!string.IsNullOrWhiteSpace(arguments)) info.lpParameters = arguments.ToNullIfEmpty();
		if (PathHelper.IsUnc(fileName)) info.fMask |= (int)ShellExecuteMaskFlagsEnum.SEE_MASK_CONNECTNETDRV;
		return info;
	}

	[NotNull]
	private static SHELLEXECUTEINFO GetShellExecuteInfo([NotNull] ShellSettings settings)
	{
		SHELLEXECUTEINFO info = new SHELLEXECUTEINFO
		{
			nShow = (int)settings.WindowStyle,
			hKeyClass = settings.HKeyClass,
			lpVerb = settings.Verb.ToNullIfEmpty(),
			lpDirectory = settings.WorkingDirectory.ToNullIfEmpty()
		};

		ShellExecuteMaskFlagsEnum mask = SHELL_BASE_FLAGS;
		if (settings.ErrorDialog) info.hWnd = settings.ErrorDialogParentHandle;
		else mask |= ShellExecuteMaskFlagsEnum.SEE_MASK_FLAG_NO_UI;

		if (settings.InheritConsole) mask |= ShellExecuteMaskFlagsEnum.SEE_MASK_NO_CONSOLE;
		if (!info.hKeyClass.IsInvalidHandle()) mask |= ShellExecuteMaskFlagsEnum.SEE_MASK_CLASSKEY;
		if (info.lpClass != null) mask |= ShellExecuteMaskFlagsEnum.SEE_MASK_CLASSNAME;
		info.fMask = (int)mask;
		return info;
	}

	private static Process InternalShellExec([NotNull] SHELLEXECUTEINFO info)
	{
		bool succeeded = false;
		int errCode = 0;

		if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
		{
			Thread thread = new Thread(state =>
			{
				SHELLEXECUTEINFO sei = (SHELLEXECUTEINFO)state;
				succeeded = Win32.ShellExecuteEx(sei);
				if (!succeeded) errCode = Marshal.GetLastWin32Error();
			})
			{
				IsBackground = false,
				Priority = ThreadPriority.Normal
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start(info);
			thread.Join();
		}
		else
		{
			succeeded = Win32.ShellExecuteEx(info);
			if (!succeeded) errCode = Marshal.GetLastWin32Error();
		}

		if (!succeeded)
		{
			if (errCode == 0)
			{
				long hInstApp = (long)info.hInstApp;

				switch (hInstApp - 2L)
				{
					case 0:
						errCode = 2;
						break;
					case 1:
						errCode = 3;
						break;
					case 2:
					case 4:
					case 5:
						errCode = (int)hInstApp;
						break;
					case 3:
						errCode = 5;
						break;
					case 6:
						errCode = 8;
						break;
					default:
						switch (hInstApp - 26L)
						{
							case 0:
								errCode = 32;
								break;
							case 2:
							case 3:
							case 4:
								errCode = 1156;
								break;
							case 5:
								errCode = 1155;
								break;
							case 6:
								errCode = 1157;
								break;
							default:
								errCode = (int)hInstApp;
								break;
						}

						break;
				}
			}

			if (errCode is 193 or 216) throw new Win32Exception(errCode);
			throw new Win32Exception(errCode);
		}

		if (info.hProcess.IsInvalidHandle()) return null;
		return TryGetProcessById(Win32.GetProcessId(info.hProcess), out Process process)
					? process
					: null;
	}
}