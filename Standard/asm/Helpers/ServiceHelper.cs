using System;
using System.Net;
using System.ServiceProcess;
using asm.Extensions;
using asm.Security;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class ServiceHelper
	{
		public static void Invoke([NotNull] Action<ServiceController> action, [NotNull] string serviceName) { Invoke(action, serviceName, null, null); }
		public static void Invoke([NotNull] Action<ServiceController> action, [NotNull] string serviceName, NetworkCredential credential) { Invoke(action, serviceName, credential, null); }
		public static void Invoke([NotNull] Action<ServiceController> action, [NotNull] string serviceName, NetworkCredential credential, string machineName)
		{
			if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentNullException(nameof(serviceName));
			machineName = machineName.ToNullIfEmpty() ?? ".";

			if (credential == null)
			{
				InvokeLocal(action, serviceName, machineName);
				return;
			}

			NetworkCredentialHelper.RunAsUser(credential, LogonType.Interactive, () => InvokeLocal(action, serviceName, machineName));

			static void InvokeLocal(Action<ServiceController> action, string serviceName, string machineName)
			{
				ServiceControllerPermission scp = new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, machineName, serviceName);
				scp.Assert();

				ServiceController controller = null;

				try
				{
					controller = new ServiceController(serviceName, machineName);
					action(controller);
				}
				finally
				{
					ObjectHelper.Dispose(ref controller);
				}
			}
		}

		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, [NotNull] string serviceName) { return Invoke(action, default(T), serviceName, null, null); }
		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, [NotNull] string serviceName, NetworkCredential credential) { return Invoke(action, default(T), serviceName, credential, null); }
		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, [NotNull] string serviceName, NetworkCredential credential, string machineName) { return Invoke(action, default(T), serviceName, credential, machineName); }
		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, T defaultValue, [NotNull] string serviceName) { return Invoke(action, defaultValue, serviceName, null, null); }
		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, T defaultValue, [NotNull] string serviceName, NetworkCredential credential) { return Invoke(action, defaultValue, serviceName, credential, null); }
		public static T Invoke<T>([NotNull] Func<ServiceController, T> action, T defaultValue, [NotNull] string serviceName, NetworkCredential credential, string machineName)
		{
			if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentNullException(nameof(serviceName));
			machineName = machineName.ToNullIfEmpty() ?? ".";

			return credential == null
						? InvokeLocal(action, serviceName, machineName)
						: NetworkCredentialHelper.RunAsUser(credential, LogonType.Interactive, () => InvokeLocal(action, serviceName, machineName));

			static T InvokeLocal(Func<ServiceController, T> action, string serviceName, string machineName)
			{
				ServiceControllerPermission scp = new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, machineName, serviceName);
				scp.Assert();

				ServiceController controller = null;

				try
				{
					controller = new ServiceController(serviceName, machineName);
					return action(controller);
				}
				finally
				{
					ObjectHelper.Dispose(ref controller);
				}
			}
		}
	}
}