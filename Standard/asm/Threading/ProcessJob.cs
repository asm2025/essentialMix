using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using asm.Extensions;
using asm.Helpers;

namespace asm.Threading
{
	/// <summary>
	/// Allows processes to be automatically killed if this parent process unexpectedly quits.
	/// This feature requires Windows 8 or greater. On Windows 7, nothing is done.</summary>
	public static class ProcessJob
	{
		public static IntPtr Create()
		{
			/*
			Windows will automatically close any open job handles when our process terminates.
			This can be verified by using SysInternals' readHandle utility. When the job handle
			is closed, the child processes will be killed.

			This feature requires Windows 8 or later. To support Windows 7 requires
			registry settings to be added if you are using Visual Studio plus an
			app.manifest change.
			*/
			if (!Environment.OSVersion.IsWindows8OrHigher()) return IntPtr.Zero;

			// The job name is optional (and can be null) but it helps with diagnostics.
			//  If it's not null, it has to be unique. Use SysInternals' Handle command-line
			//  utility: handle -a ChildProcessTracker
			string jobName = "ChildProcessTracker" + Process.GetCurrentProcess().Id;
			IntPtr jobHandle = Win32.CreateJobObject(IntPtr.Zero, jobName);

			Win32.JOBOBJECT_BASIC_LIMIT_INFORMATION info = new Win32.JOBOBJECT_BASIC_LIMIT_INFORMATION
			{
				LimitFlags = Win32.JobObjectLimitEnum.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
			};

			// This is the key flag. When our process is killed, Windows will automatically
			//  close the job handle, and when that happens, we want the child processes to
			//  be killed, too.
			Win32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new Win32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION {BasicLimitInformation = info};

			int length = Marshal.SizeOf(typeof(Win32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
			IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);

			try
			{
				Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

				if (!Win32.SetInformationJobObject(jobHandle, Win32.JobObjectInfoTypeEnum.ExtendedLimitInformation,
					extendedInfoPtr, (uint)length))
				{
					throw new Win32Exception("Could not set job extended limit information");
				}
			}
			finally
			{
				Marshal.FreeHGlobal(extendedInfoPtr);
			}

			return jobHandle;
		}

		/// <summary>
		/// Add the process to be tracked. If our current process is killed, the child processes
		/// that we are tracking will be automatically killed, too. If the child process terminates
		/// first, that's fine, too.</summary>
		/// <param name="hJob"></param>
		/// <param name="process"></param>
		public static bool AddProcess(IntPtr hJob, Process process)
		{
			return !hJob.IsInvalidHandle() && process.IsAwaitable() && Win32.AssignProcessToJobObject(hJob, process.Handle);
		}

		/// <summary>
		/// Add the process to be tracked. If our current process is killed, the child processes
		/// that we are tracking will be automatically killed, too. If the child process terminates
		/// first, that's fine, too.</summary>
		/// <param name="hJob"></param>
		/// <param name="hProcess"></param>
		public static bool AddProcess(IntPtr hJob, IntPtr hProcess)
		{
			if (!hJob.IsInvalidHandle() || !hProcess.IsInvalidHandle()) return false;
			if (!ProcessHelper.TryGetProcessById(hProcess, out Process process)) return false;
			return process != null && AddProcess(hJob, process);
		}
	}
}