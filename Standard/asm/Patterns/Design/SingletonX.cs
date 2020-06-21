using System;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Patterns.Design
{
	public class SingletonX : Disposable
	{
		private Mutex _mutex;

		public SingletonX([NotNull] string uniqueName)
			: this(uniqueName, 0)
		{
		}

		public SingletonX([NotNull] string uniqueName, TimeSpan timeout)
			: this(uniqueName, timeout.TotalIntMilliseconds())
		{
		}

		public SingletonX([NotNull] string uniqueName, int millisecondsTimeout)
		{
			if (string.IsNullOrEmpty(uniqueName)) throw new ArgumentNullException(nameof(uniqueName));
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			Name = uniqueName;
			Timeout = millisecondsTimeout;
			_mutex = new Mutex(true, Name);
		}

		public string Name { get; }

		public int Timeout { get; }

		public bool IsRunning
		{
			get
			{
				ThrowIfDisposed();
				return !_mutex.WaitOne(Timeout, true);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _mutex != null)
			{
				_mutex.ReleaseMutex();
				_mutex.Close();
				ObjectHelper.Dispose(ref _mutex);
			}
			base.Dispose(disposing);
		}
	}
}