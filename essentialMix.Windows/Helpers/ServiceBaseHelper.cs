using System;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Windows.Helpers;

public static class ServiceBaseHelper
{
	private static readonly Lazy<MethodInfo> __onStart = new Lazy<MethodInfo>(() => typeof(ServiceBase).GetMethod("OnStart", Constants.BF_NON_PUBLIC_INSTANCE), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<MethodInfo> __onStop = new Lazy<MethodInfo>(() => typeof(ServiceBase).GetMethod("OnStop", Constants.BF_NON_PUBLIC_INSTANCE), LazyThreadSafetyMode.PublicationOnly);

	public static void RunServicesInteractively([NotNull] params ServiceBase[] services)
	{
		foreach (ServiceBase service in services)
		{
			__onStart.Value.Invoke(service, []);
		}
	}

	public static void StopServices([NotNull] params ServiceBase[] services)
	{
		foreach (ServiceBase service in services)
		{
			__onStop.Value.Invoke(service, null);
		}
	}
}