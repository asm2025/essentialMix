using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Collections;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue;

public class EventQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
	where TQueue : ICollection, IReadOnlyCollection<T>
{
	private readonly QueueAdapter<TQueue, T> _queue;
	private readonly Thread[] _workers;

	private AutoResetEvent _queueEvent;

	public EventQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(options, token)
	{
		_queue = new QueueAdapter<TQueue, T>(queue);
		_queueEvent = new AutoResetEvent(false);
		_workers = new Thread[Threads];
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing) return;
		ObjectHelper.Dispose(ref _queueEvent);
	}

	/// <inheritdoc />
	// ReSharper disable once InconsistentlySynchronizedField
	public TQueue Queue => _queue.Queue;
		
	/// <inheritdoc />
	// ReSharper disable once InconsistentlySynchronizedField
	public bool IsSynchronized => _queue.IsSynchronized;

	/// <inheritdoc />
	// ReSharper disable once InconsistentlySynchronizedField
	public object SyncRoot => _queue.SyncRoot;

	/// <inheritdoc />
	// ReSharper disable once InconsistentlySynchronizedField
	public override int Count => _queue.Count + Running;

	/// <inheritdoc />
	// ReSharper disable once InconsistentlySynchronizedField
	public override bool IsEmpty => _queue.Count == 0;

	/// <inheritdoc />
	public override bool CanPause => true;

	protected override void EnqueueInternal(T item)
	{
		if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;

		if (!WaitForWorkerStart())
		{
			bool invokeWorkStarted = false;

			lock(_workers)
			{
				if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;

				if (!WaitForWorkerStart())
				{
					InitializeWorkerStart();
					InitializeWorkersCountDown();
					InitializeTasksCountDown();

					for (int i = 0; i < _workers.Length; i++)
					{
						(_workers[i] = new Thread(Consume)
								{
									IsBackground = IsBackground,
									Priority = Priority
								}).Start();
					}
					
					invokeWorkStarted = true;
					if (!WaitForWorkerStart()) throw new TimeoutException();
					if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;
				}
			}

			if (invokeWorkStarted) WorkStartedCallback?.Invoke(this);
		}

		lock(SyncRoot)
		{
			_queue.Enqueue(item);
			_queueEvent.Set();
		}
	}

	/// <inheritdoc />
	public bool TryDequeue(out T item)
	{
		ThrowIfDisposed();

		lock(SyncRoot)
		{
			if (!_queue.TryDequeue(out item)) return false;
			_queueEvent.Set();
		}

		return true;
	}

	/// <inheritdoc />
	public bool TryPeek(out T item)
	{
		ThrowIfDisposed();

		lock(SyncRoot)
			return _queue.TryPeek(out item);
	}

	/// <inheritdoc />
	public void RemoveWhile(Predicate<T> predicate)
	{
		ThrowIfDisposed();

		lock(SyncRoot)
		{
			if (_queue.IsEmpty) return;

			int n = _queue.Count;

			while (!_queue.IsEmpty && _queue.TryPeek(out T item) && predicate(item)) 
				_queue.Dequeue();

			if (n == _queue.Count) return;
			_queueEvent.Set();
		}
	}

	protected override void CompleteInternal()
	{
		IsCompleted = true;
		_queueEvent.Set();
	}

	protected override void ClearInternal()
	{
		lock(SyncRoot)
		{
			_queue.Clear();
			_queueEvent.Set();
		}
	}

	private void Consume()
	{
		if (IsDisposed || Token.IsCancellationRequested) return;
		SignalWorkerStart();

		try
		{
			while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
			{
				if (IsPaused)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
					continue;
				}

				if (_queue.IsEmpty && !_queueEvent.WaitOne(TimeSpanHelper.FAST, Token)) continue;
				if (IsPaused || IsDisposed || Token.IsCancellationRequested || IsCompleted || _queue.IsEmpty) continue;
				T item;

				lock(SyncRoot)
				{
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
				}

				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				Run(item);
			}

			if (IsDisposed || Token.IsCancellationRequested) return;

			while (!IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
			{
				if (IsPaused)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
					continue;
				}

				T item;

				lock(SyncRoot)
				{
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
				}

				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				Run(item);
			}
		}
		catch (ObjectDisposedException) { }
		catch (OperationCanceledException) { }
		finally
		{
			SignalWorkersCountDown();
		}
	}

	/// <inheritdoc />
	protected override void Run(T item)
	{
		try
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			base.Run(item);
		}
		finally
		{
			SignalTasksCountDown();
		}
	}
}

/// <inheritdoc cref="EventQueue{TQueue,T}" />
public sealed class EventQueue<T> : EventQueue<Queue<T>, T>, IProducerQueue<T>
{
	/// <inheritdoc />
	public EventQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(new Queue<T>(), options, token)
	{
	}
}