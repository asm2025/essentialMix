using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Patterns.Threading
{
	public class SingletonAppGuard : Disposable
	{
		private readonly bool _hasHandle;
		private Mutex _mutex;

		/// <inheritdoc />
		public SingletonAppGuard()
			: this(TimeSpanHelper.INFINITE)
		{
		}

		public SingletonAppGuard(TimeSpan timeout) 
			: this(timeout.TotalIntMilliseconds())
		{
		}

		public SingletonAppGuard(int timeout) 
		{
			try
			{
				// Global prefix means it is global to the machine.
				string mutexId = $"Global\\{AppInfo.AppGuid}";
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
}