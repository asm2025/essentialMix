using System;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Threading;

public class SingletonAppGuard : Disposable
{
	private readonly bool _hasHandle;
	private Mutex _mutex;

	/// <inheritdoc />
	public SingletonAppGuard()
		: this(null, TimeSpanHelper.INFINITE)
	{
	}

	public SingletonAppGuard([NotNull] Assembly assembly)
		: this(assembly, TimeSpanHelper.INFINITE)
	{
	}

	public SingletonAppGuard(TimeSpan timeout) 
		: this(null, timeout.TotalIntMilliseconds())
	{
	}

	public SingletonAppGuard(int timeout) 
		: this(null, timeout)
	{
	}

	public SingletonAppGuard(Assembly assembly, TimeSpan timeout) 
		: this(assembly, timeout.TotalIntMilliseconds())
	{
	}

	public SingletonAppGuard(Assembly assembly, int timeout) 
	{
		try
		{
			assembly ??= Assembly.GetCallingAssembly();
			AppInfo appInfo = new AppInfo(assembly);
			// Global prefix means it is global to the machine.
			string mutexId = $"Global\\{appInfo.AppGuid}";
			_mutex = new Mutex(false, mutexId);

			SecurityIdentifier worldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			MutexAccessRule allowAll = new MutexAccessRule(worldSid, MutexRights.FullControl, AccessControlType.Allow);
			MutexSecurity mutexSecurity = new MutexSecurity();
			mutexSecurity.AddAccessRule(allowAll);
			_mutex.SetAccessControl(mutexSecurity);
			_hasHandle = timeout < 0
							? _mutex.WaitOne(TimeSpanHelper.INFINITE, false)
							: _mutex.WaitOne(timeout, false);
			if (!_hasHandle) throw new TimeoutException();
		}
		catch (AbandonedMutexException)
		{
			_hasHandle = true;
		}
		catch
		{
			if (_mutex != null)
			{
				if (_hasHandle) _mutex.ReleaseMutex();
				_mutex.Close();
				ObjectHelper.Dispose(ref _mutex);
			}

			throw;
		}
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_mutex != null)
			{
				if (_hasHandle) _mutex.ReleaseMutex();
				_mutex.Close();
				ObjectHelper.Dispose(ref _mutex);
			}
		}

		base.Dispose(disposing);
	}
}