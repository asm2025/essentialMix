using System;
using System.Windows.Threading;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DispatcherExtension
{
	// inspired by https://stackoverflow.com/questions/1751966/commandmanager-invalidaterequerysuggested-isnt-fast-enough-what-can-i-do#1857619
	public static void Run([NotNull] this Dispatcher thisValue, [NotNull] Action action) { Run(thisValue, action, DispatcherPriority.Normal); }
	public static void Run([NotNull] this Dispatcher thisValue, [NotNull] Action action, DispatcherPriority priority)
	{
		if (thisValue.CheckAccess())
		{
			action();
			return;
		}

		thisValue.Invoke(priority, action);
	}

	public static void Run<T>([NotNull] this Dispatcher thisValue, [NotNull] Action<T> action, T parameter) { Run(thisValue, action, parameter, DispatcherPriority.Normal); }
	public static void Run<T>([NotNull] this Dispatcher thisValue, [NotNull] Action<T> action, T parameter, DispatcherPriority priority)
	{
		if (thisValue.CheckAccess())
		{
			action(parameter);
			return;
		}

		thisValue.Invoke(priority, action);
	}
}