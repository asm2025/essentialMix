using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Collections;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue;

/// <summary>
/// ONLY use this queue when the life span duration of this whole queue object is generally short to do specific things and then get thrown away.
/// This queue uses dedicated Tasks to consume queued items which is Ok for a short time but not Ok for long running tasks because they will block
/// the default ThreadPool's threads. If this queue should have a long lifetime span, then consider using other <see cref="ProducerConsumerQueue{T}"/>
/// types which use dedicated threads to consume queued items such as <see cref="WaitAndPulseQueue{TQueue,T}" /> or <see cref="EventQueue{TQueue,T}" />
/// <para>This queue uses ThreadPool.UnsafeQueueUserWorkItem.
/// For info about ThreadPool.UnsafeQueueUserWorkItem vs ThreadPool.QueueUserWorkItem, see <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool.unsafequeueuserworkitem">ThreadPool.UnsafeQueueUserWorkItem</see>
/// vs <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool.queueuserworkitem?view=net-5.0">ThreadPool.QueueUserWorkItem</see> documentation
/// or <see href="https://stackoverflow.com/questions/16926106/unsafequeueuserworkitem-and-what-exactly-does-does-not-propagate-the-calling-st#16928762">this answer</see> on <see href="https://stackoverflow.com/">StackOverflow.com</see> for explanation.</para>
/// <para>In summary, It's about CAS, Code Access Security, and the ExecutionContext. The ExecutionContext.Capture() method performs an essential role. It makes a copy of the context of the calling thread, including making a stack walk to create a "compressed"
/// stack of the security attributes discovered. ThreadPool.UnsafeQueueUserWorkItem() skips the Capture() call. The thread pool thread will run with the default execution context. This is an optimization, Capture() is not a cheap method. It matters in the kind
/// of program that depends on TP threads to get stuff done in a hurry.</para>
/// </summary>
public class ThreadPoolQueue<TQueue, T>(
	[NotNull] TQueue queue,
	[NotNull] ProducerConsumerQueueOptions<T> options,
	CancellationToken token = default(CancellationToken))
	: ProducerConsumerThreadQueue<T>(options, token), IProducerQueue<TQueue, T>
	where TQueue : ICollection, IReadOnlyCollection<T>
{
	private readonly QueueAdapter<TQueue, T> _queue = new(queue);

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
		/*
		*/

		try
		{
			while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
			{
				if (IsPaused || Running >= Threads)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || (!IsPaused && Running < Threads));
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
				ThreadPool.UnsafeQueueUserWorkItem(RunThread, item);
			}

			if (IsDisposed || Token.IsCancellationRequested) return;

			while (!IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
			{
				if (IsPaused || Running >= Threads)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || (!IsPaused && Running < Threads));
					continue;
				}

				T item;

				lock(SyncRoot)
				{
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
				}

				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				ThreadPool.UnsafeQueueUserWorkItem(RunThread, item);
			}
		}
		catch (ObjectDisposedException) { }
		catch (OperationCanceledException) { }
		finally
		{
			SignalWorkersCountDown();
		}
	}

	private void RunThread(object rawValue)
	{
		if (IsDisposed || Token.IsCancellationRequested) return;

		try
		{
			Run((T)rawValue);
		}
		finally
		{
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

/// <inheritdoc cref="SemaphoreSlimQueue{TQueue,T}"/>
public sealed class ThreadPoolQueue<T> : SemaphoreSlimQueue<Queue<T>, T>, IProducerQueue<T>
{
	/// <inheritdoc />
	public ThreadPoolQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(new Queue<T>(), options, token)
	{
	}
}