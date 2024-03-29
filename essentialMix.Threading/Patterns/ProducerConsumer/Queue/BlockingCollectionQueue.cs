using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue;

public sealed class BlockingCollectionQueue<T> : ProducerConsumerThreadQueue<T>
{
	private readonly Thread[] _workers;

	private BlockingCollection<T> _queue;

	public BlockingCollectionQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(options, token)
	{
		_queue = new BlockingCollection<T>();
		_workers = new Thread[Threads];
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing) return;
		ObjectHelper.Dispose(ref _queue);
	}

	/// <inheritdoc />
	public override int Count => _queue.Count + Running;

	/// <inheritdoc />
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

		// BlockingCollection<T> is thread safe
		_queue.Add(item, Token);
	}

	protected override void CompleteInternal()
	{
		IsCompleted = true;
		_queue.CompleteAdding();
	}

	protected override void ClearInternal()
	{
		_queue.Clear();
	}

	private void Consume()
	{
		if (IsDisposed || Token.IsCancellationRequested) return;
		SignalWorkerStart();
			
		try
		{
			// BlockingCollection<T> is thread safe
			while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
			{
				if (IsPaused)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
					continue;
				}

				if (_queue.Count == 0 || !_queue.TryTake(out T item, TimeSpanHelper.FAST, Token)) continue;
				if (IsPaused || IsDisposed || Token.IsCancellationRequested || IsCompleted || _queue.Count == 0) continue;
				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				Run(item);
			}

			if (IsDisposed || Token.IsCancellationRequested) return;

			IEnumerator<T> enumerator = null;

			try
			{
				enumerator = _queue.GetConsumingEnumerable(Token).GetEnumerator();

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						if (IsDisposed || Token.IsCancellationRequested) return;
					}

					if (!enumerator.MoveNext()) break;
					T item = enumerator.Current;
					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					Run(item);
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref enumerator);
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