using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue;

/// <summary>
/// ONLY use this queue when the life span duration of this whole queue object is generally short to do specific things and then get thrown away.
/// This queue uses dedicated Tasks to consume queued items which is Ok for a short time but not Ok for long running tasks because they will block
/// the default ThreadPool's threads. If this queue should have a long lifetime span, then consider using other <see cref="ProducerConsumerQueue{T}"/>
/// types which use dedicated threads to consume queued items such as <see cref="WaitAndPulseQueue{TQueue, T}" /> or <see cref="EventQueue{TQueue,T}" />
/// </summary>
public class TaskQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
	where TQueue : ICollection, IReadOnlyCollection<T>
{
	private readonly QueueAdapter<TQueue, T> _queue;

	// will use semaphoreSlim instead of any collection to manage running tasks
	private SemaphoreSlim _semaphore;

	public TaskQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(options, token)
	{
		_queue = new QueueAdapter<TQueue, T>(queue);
		_semaphore = new SemaphoreSlim(Threads);
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing) return;
		ObjectHelper.Dispose(ref _semaphore);
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

			lock(SyncRoot)
			{
				if (!WaitForWorkerStart())
				{
					InitializeWorkerStart();
					InitializeWorkersCountDown(1);
					InitializeTasksCountDown();

					new Thread(Consume)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start();
					
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
			Monitor.Pulse(SyncRoot);
		}
	}

	/// <inheritdoc />
	public bool TryDequeue(out T item)
	{
		ThrowIfDisposed();

		lock(SyncRoot)
		{
			if (!_queue.TryDequeue(out item)) return false;
			Monitor.Pulse(SyncRoot);
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
			Monitor.Pulse(SyncRoot);
		}
	}

	protected override void CompleteInternal()
	{
		lock(SyncRoot)
		{
			IsCompleted = true;
			Monitor.PulseAll(SyncRoot);
		}
	}

	protected override void ClearInternal()
	{
		lock(SyncRoot)
		{
			_queue.Clear();
			Monitor.PulseAll(SyncRoot);
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

				if (!WaitForQueue()) continue;
				if (IsDisposed || Token.IsCancellationRequested) return;
				T item;
						
				lock(SyncRoot)
				{
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
				}

				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				// this MUST be a real thread (which LongRunning is for) because of the semaphore, don't be tempted to use Task.Run
				TaskHelper.Run(() => RunThread(item), TaskCreationOptions.LongRunning, Token)
						.ConfigureAwait();
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
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) break;
				}

				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				// this MUST be a real thread (which LongRunning is for) because of the semaphore, don't be tempted to use Task.Run.
				TaskHelper.Run(() => RunThread(item), TaskCreationOptions.LongRunning, Token)
						.ConfigureAwait();
			}
		}
		catch (ObjectDisposedException) { }
		catch (OperationCanceledException) { }
		finally
		{
			SignalWorkersCountDown();
		}
	}

	private void RunThread(T item)
	{
		bool entered = false;

		try
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			if (!_semaphore.Wait(TimeSpanHelper.INFINITE, Token)) return;
			if (IsDisposed) return;
			entered = true;
			if (Token.IsCancellationRequested) return;
			Run(item);
		}
		finally
		{
			if (entered) _semaphore?.Release();
			SignalTasksCountDown();
		}
	}

	private bool WaitForQueue()
	{
		if (!_queue.IsEmpty) return true;
		SpinWait spinner = new SpinWait();

		while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted && _queue.IsEmpty)
		{
			if (IsPaused) return false;

			lock(SyncRoot)
			{
				if (IsPaused || IsDisposed || Token.IsCancellationRequested || IsCompleted || !_queue.IsEmpty) continue;
				Monitor.Wait(SyncRoot, TimeSpanHelper.FAST);
			}

			spinner.SpinOnce();
		}

		return !IsDisposed && !Token.IsCancellationRequested && !IsCompleted && !_queue.IsEmpty;
	}
}

/// <inheritdoc cref="TaskQueue{TQueue,T}"/>
public sealed class TaskQueue<T> : TaskQueue<Queue<T>, T>, IProducerQueue<T>
{
	/// <inheritdoc />
	public TaskQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(new Queue<T>(), options, token)
	{
	}
}