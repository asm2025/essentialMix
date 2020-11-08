using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using asm.Helpers;
using JetBrains.Annotations;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace asm.Extensions
{
	public static class ServiceControllerExtension
	{
		public static bool IsRunning([NotNull] this ServiceController thisValue) { return IsRunning(thisValue, TimeSpanHelper.FiveSeconds); }
		public static bool IsRunning([NotNull] this ServiceController thisValue, TimeSpan timeout)
		{
			thisValue.Refresh();

			switch (thisValue.Status)
			{
				case ServiceControllerStatus.StartPending:
				case ServiceControllerStatus.ContinuePending:
					try
					{
						thisValue.WaitForStatus(ServiceControllerStatus.Running, timeout);
						thisValue.Refresh();
						return thisValue.Status == ServiceControllerStatus.Running;
					}
					catch (TimeoutException)
					{
						return false;
					}
				case ServiceControllerStatus.Running:
					return true;
				default:
					return false;
			}
		}

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public static bool AssertControlAccessRights([NotNull] this ServiceController thisValue, [NotNull] SecurityIdentifier sid)
		{
			ServiceControllerPermission scp = new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, thisValue.MachineName, thisValue.ServiceName);
			scp.Demand();

			ServiceAccessRights accessRights = GetEffectiveAccessRights(thisValue, sid);
			return accessRights.HasFlag(ServiceAccessRights.QueryStatus | ServiceAccessRights.Start | ServiceAccessRights.Stop | ServiceAccessRights.PauseContinue);
		}

		public static bool IsStopped([NotNull] this ServiceController thisValue) { return IsStopped(thisValue, TimeSpanHelper.FiveSeconds); }
		public static bool IsStopped([NotNull] this ServiceController thisValue, TimeSpan timeout)
		{
			thisValue.Refresh();

			switch (thisValue.Status)
			{
				case ServiceControllerStatus.StopPending:
				case ServiceControllerStatus.PausePending:
					ServiceController controller = thisValue;
					return SpinWait.SpinUntil(() =>
					{
						controller.Refresh();
						return controller.Status == ServiceControllerStatus.Stopped || controller.Status == ServiceControllerStatus.Paused;
					}, timeout);
				case ServiceControllerStatus.Stopped:
				case ServiceControllerStatus.Paused:
					return true;
				default:
					return false;
			}
		}

		public static void InvokeWithCredential([NotNull] this ServiceController thisValue, [NotNull] Action<ServiceController> action, [NotNull] NetworkCredential credential)
		{
			ImpersonationHelper.RunWithCredential(credential, LogonType.Interactive, () => action(thisValue));
		}

		public static T InvokeWithCredential<T>([NotNull] this ServiceController thisValue, [NotNull] Func<ServiceController, T> action, [NotNull] NetworkCredential credential)
		{
			return ImpersonationHelper.RunWithCredential(credential, LogonType.Interactive, () => action(thisValue));
		}

		public static void InvokeWithElevatedPrivilege([NotNull] this ServiceController thisValue, [NotNull] Action<ServiceController> action)
		{
			ImpersonationHelper.RunWithElevatedPrivilege(() => action(thisValue));
		}

		public static T InvokeWithElevatedPrivilege<T>([NotNull] this ServiceController thisValue, [NotNull] Func<ServiceController, T> action)
		{
			return ImpersonationHelper.RunWithElevatedPrivilege(() => action(thisValue));
		}

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public static ServiceAccessRights GetEffectiveAccessRights([NotNull] this ServiceController thisValue, [NotNull] SecurityIdentifier sid)
		{
			Win32.QueryServiceObjectSecurity(thisValue.ServiceHandle, SecurityInfos.DiscretionaryAcl, null, 0u, out uint len);
			int errCode = Marshal.GetLastWin32Error();

			if (errCode != ResultWin32.ERROR_INSUFFICIENT_BUFFER)
			{
				return errCode == 0
							? (ServiceAccessRights)0
							: throw new Win32Exception(errCode);
			}
			
			byte[] buffer = new byte[len];
			if (!Win32.QueryServiceObjectSecurity(thisValue.ServiceHandle, SecurityInfos.DiscretionaryAcl, buffer, len, out len)) throw new Win32Exception(Marshal.GetLastWin32Error());

			RawSecurityDescriptor rsd = new RawSecurityDescriptor(buffer, 0);
			RawAcl racl = rsd.DiscretionaryAcl;
			DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, racl);
			byte[] daclBuffer = new byte[dacl.BinaryLength];
			dacl.GetBinaryForm(daclBuffer, 0);

			byte[] sidBuffer = new byte[sid.BinaryLength];
			sid.GetBinaryForm(sidBuffer, 0);

			TRUSTEE trustee = new TRUSTEE();
			Win32.BuildTrusteeWithSid(ref trustee, sidBuffer);

			uint access = 0u;
			int hr = (int)Win32.GetEffectiveRightsFromAcl(daclBuffer, ref trustee, ref access);
			Marshal.Release(trustee.ptstrName);
			if (hr != ResultWin32.ERROR_SUCCESS) throw Marshal.GetExceptionForHR(hr);
			return (ServiceAccessRights)access;
		}
	}
}