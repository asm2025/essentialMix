#pragma warning disable 1591, 0612
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace essentialMix.Patterns.Object;

/// <inheritdoc />
public abstract class Disposable : IDisposable
{
	private const int DISPOSAL_NOT_STARTED = 0;
	private const int DISPOSAL_STARTED = 1;
	private const int DISPOSAL_COMPLETE = 2;

	private sealed class DisposableAction : IDisposable
	{
		private readonly object _lock = new object();

		private Action _onDispose;

		public DisposableAction(Action onDispose)
		{
			_onDispose = onDispose;
		}

		public void Dispose()
		{
			Action todo;

			lock (_lock)
			{
				todo = _onDispose;
				_onDispose = null;
			}

			todo?.Invoke();
		}
	}

#if DEBUG
	// useful diagnostics when a failure to dispose is detected
	[SuppressMessage("ReSharper", "NotAccessedField.Local")]
	private readonly StackTrace _creationStackTrace;
#endif

	// see the constants defined above for valid values
	private volatile int _disposeStage;

	protected Disposable()
	{
#if DEBUG
		_creationStackTrace = new StackTrace(1, true);
#endif
	}

#if DEBUG
	/// <summary>
	/// Finalizes an instance of the Disposable class.
	/// </summary>
	~Disposable()
	{
		Dispose(false);
	}
#endif

	/// <summary>
	/// Gets a value indicating whether this object is in the process of disposing.
	/// </summary>
	protected bool IsDisposing => Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_STARTED, DISPOSAL_STARTED) == DISPOSAL_STARTED;

	/// <summary>
	/// Gets a value indicating whether this object has been disposed.
	/// </summary>
	protected bool IsDisposed => Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_COMPLETE, DISPOSAL_COMPLETE) == DISPOSAL_COMPLETE;

	/// <summary>
	/// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
	/// </summary>
	protected bool IsDisposedOrDisposing => Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_NOT_STARTED, DISPOSAL_NOT_STARTED) != DISPOSAL_NOT_STARTED;

	/// <inheritdoc />
	/// <summary>
	/// Disposes of this object, if it hasn't already been disposed.
	/// </summary>
	public void Dispose()
	{
		if (Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_STARTED, DISPOSAL_NOT_STARTED) != DISPOSAL_NOT_STARTED) return;
		GC.SuppressFinalize(this);
		Dispose(true);
		MarkAsDisposed();
	}

	/// <summary>
	/// Verifies that this object is not in the process of disposing, throwing an exception if it is.
	/// </summary>
	protected void ThrowIfDisposing()
	{
		if (!IsDisposing) return;
		throw new ObjectDisposedException(this.ObjectName());
	}

	/// <summary>
	/// Verifies that this object has not been disposed, throwing an exception if it is.
	/// </summary>
	protected void ThrowIfDisposed()
	{
		if (!IsDisposed) return;
		throw new ObjectDisposedException(this.ObjectName());
	}

	/// <summary>
	/// Verifies that this object is not being disposed or has been disposed, throwing an exception if either of these are true.
	/// </summary>
	protected void ThrowIfDisposedOrDisposing()
	{
		if (!IsDisposedOrDisposing) return;
		throw new ObjectDisposedException(this.ObjectName());
	}

	/// <summary>
	/// Allows subclasses to provide dispose logic.
	/// </summary>
	/// <param name="disposing">
	/// Whether the method is being called in response to disposal, or finalization.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>
	/// Marks this object as disposed without running any other dispose logic.
	/// </summary>
	/// <remarks>
	/// Use this method with caution. It is helpful when you have an object that can be disposed in multiple fashions, such as through a <c>CloseAsync</c> method.
	/// </remarks>
	protected void MarkAsDisposed()
	{
		Interlocked.Exchange(ref _disposeStage, DISPOSAL_COMPLETE);
	}

	[NotNull]
	public static IDisposable Create([NotNull] Action action) { return new DisposableAction(action); }
}
#pragma warning restore 1591, 0612