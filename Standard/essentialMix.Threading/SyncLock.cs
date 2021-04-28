using System;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Exceptions.Threading;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading
{
	/// <summary>
	/// Class used for locking, as an alternative to just locking on normal monitors.
	/// Allows for timeouts when locking, and each Lock method returns a token which
	/// must then be disposed of to release the internal monitor (i.e. to unlock).
	/// All properties and methods of this class are thread-safe.
	/// </summary>
	public class SyncLock
	{
		/// <summary>
		/// A lock token returned by a Lock method call on a SyncLock.
		/// This effectively holds the lock until it is disposed - a
		/// slight violation of the IDisposable contract, but it makes
		/// for easy use of the SyncLock global::System. This type itself
		/// is not thread-safe - LockTokens should not be shared between
		/// threads.
		/// </summary>
		private class LockToken : Disposable
		{
			/// <summary>
			/// The lock this token has been created by.
			/// </summary>
			private readonly SyncLock _parent;

			/// <summary>
			/// Constructs a new lock token for the specified lock.
			/// </summary>
			/// <param name="parent">The internal monitor used for locking.</param>
			internal LockToken(SyncLock parent)
			{
				_parent = parent;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing) _parent.Unlock();
				base.Dispose(disposing);
			}
		}

		/// <summary>
		/// The default timeout for new instances of this class
		/// where the default timeout isn't otherwise specified.
		/// Defaults to Timeout.INFINITE.
		/// </summary>
		private static int __defaultTimeout = TimeSpanHelper.INFINITE;

		private object _syncRoot;

		/// <inheritdoc />
		/// <summary>
		/// Creates a new lock with no name, and the default timeout specified by DefaultTimeout.
		/// </summary>
		public SyncLock() 
			: this(null, DefaultTimeout)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Creates a new lock with the specified name, and the default timeout specified by
		/// DefaultTimeout.
		/// </summary>
		/// <param name="name">The name of the new lock</param>
		public SyncLock(string name) 
			: this(name, DefaultTimeout)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Creates a new lock with no name, and the specified default timeout
		/// </summary>
		/// <param name="timeout">Default timeout, in milliseconds</param>
		public SyncLock(int timeout) 
			: this(null, timeout)
		{
		}

		/// <summary>
		/// Creates a new lock with the specified name, and an
		/// infinite default timeout.
		/// </summary>
		/// <param name="name">The name of the new lock</param>
		/// <param name="timeout">
		/// Default timeout, in milliseconds. Use Timeout.INFINITE
		/// for an infinite timeout, or a non-negative number otherwise.
		/// </param>
		public SyncLock(string name, int timeout)
		{
			if (timeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(timeout), "Invalid timeout specified");
			name ??= "AnonymousLock";
			Name = name;
			Timeout = timeout;
		}

		/// <summary>
		/// The default timeout for the
		/// </summary>
		public int Timeout { get; }

		/// <summary>
		/// The name of this lock.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The synchronization object used for locking. While this
		/// is owned by the thread, it can be used for waiting
		/// and pulsing in the usual way. Note that manually entering/exiting
		/// this monitor could result in the lock malfunctioning.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <summary>
		/// Locks the monitor, with the default timeout.
		/// </summary>
		/// <returns>A lock token which should be disposed to release the lock</returns>
		/// <exception cref="LockTimeoutException">The operation times out.</exception>
		[NotNull]
		public IDisposable Lock() { return Lock(TimeSpanHelper.INFINITE); }

		/// <summary>
		/// Locks the monitor, with the specified timeout.
		/// </summary>
		/// <param name="timeout">
		/// The timeout duration. When converted to milliseconds,
		/// must be Timeout.INFINITE, or non-negative.
		/// </param>
		/// <returns>A lock token which should be disposed to release the lock</returns>
		/// <exception cref="LockTimeoutException">The operation times out.</exception>
		[NotNull]
		public IDisposable Lock(TimeSpan timeout) { return Lock(timeout.TotalIntMilliseconds()); }

		/// <summary>
		/// Locks the monitor, with the specified timeout. Derived classes may override
		/// this method to change the behaviour; the other calls to Lock all result in
		/// a call to this method. This implementation checks the validity of the timeout,
		/// calls Monitor.TryEnter (throwing an exception if appropriate) and returns a
		/// new LockToken.
		/// </summary>
		/// <param name="millisecondsTimeout">
		/// The timeout, in milliseconds. Must be Timeout.INFINITE,
		/// or non-negative.
		/// </param>
		/// <returns>A lock token which should be disposed to release the lock</returns>
		/// <exception cref="LockTimeoutException">The operation times out.</exception>
		[NotNull]
		public virtual IDisposable Lock(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), "Invalid timeout specified");
			if (!Monitor.TryEnter(SyncRoot, millisecondsTimeout)) throw new LockTimeoutException("Failed to acquire lock {0}", Name);
			return new LockToken(this);
		}

		/// <summary>
		/// Unlocks the monitor. This method may be overridden in derived classes
		/// to change the behaviour. This implementation simply calls Monitor.Exit.
		/// </summary>
		protected internal virtual void Unlock()
		{
			Monitor.Exit(SyncRoot);
		}

		protected static int DefaultTimeout
		{
			get => __defaultTimeout;
			set
			{
				if (value < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(value), "Invalid timeout specified");
				Interlocked.CompareExchange(ref __defaultTimeout, value, TimeSpanHelper.INFINITE);
			}
		}
	}
}