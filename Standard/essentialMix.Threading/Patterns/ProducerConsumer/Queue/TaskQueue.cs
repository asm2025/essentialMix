using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	/// <summary>
	/// ONLY use this queue when the life span duration of this whole queue object is generally short to do specific things and then get thrown away.
	/// This queue uses dedicated Tasks to consume queued items which is Ok for a short time but not Ok for long running tasks because they will block
	/// the default ThreadPool's threads. If this queue should have a long lifetime span, then consider using other <see cref="ProducerConsumerQueue{T}"/>
	/// types which use dedicated threads to consume queued items such as <see cref="WaitAndPulseQueue{TQueue,T}" /> or <see cref="EventQueue{TQueue,T}" />
	/// </summary>
	public class TaskQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;
		
		private CountdownEvent _countdown;
		private bool _workStarted;

		public TaskQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
			_countdown = new CountdownEvent(1);
			new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}.Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _countdown);
		}

		/// <inheritdoc />
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _queue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

		public sealed override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public sealed override bool IsBusy => Count > 0;

		protected sealed override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			lock(SyncRoot) 
				_queue.Enqueue(item);
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();

			lock(SyncRoot)
				return _queue.TryDequeue(out item);
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
				while (!_queue.IsEmpty && _queue.TryPeek(out T item) && predicate(item)) 
					_queue.Dequeue();
			}
		}

		protected sealed override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected sealed override void ClearInternal()
		{
			lock(SyncRoot)
				_queue.Clear();
		}

		protected sealed override bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;
			
			try
			{
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _countdown.Wait(millisecondsTimeout, Token);
				_countdown.Wait(Token);
				return !Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
			}

			return false;
		}

		protected sealed override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			Cancel();
			ClearInternal();
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			OnWorkStarted(EventArgs.Empty);

			try
			{
				List<Task> tasks = new List<Task>(Threads);

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (IsPaused) continue;
					T item;
					
					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					ScheduledCallback?.Invoke(item);

					// copy to local variables
					int index = tasks.Count;
					List<Task> tasksRef = tasks;
					_countdown.AddCount();
					Task task = Task.Run(() =>
									{
										try
										{
											Run(item);
										}
										finally
										{
											lock(tasksRef)
											{
												tasksRef[index].Dispose();
												tasksRef.RemoveAt(index);
											}

											_countdown?.SignalOne();
										}
									}, Token)
									.ConfigureAwait();
					lock(tasksRef)
						tasksRef.Add(task);
					
					// if not filled yet continue
					if (IsDisposed || Token.IsCancellationRequested || tasks.Count < Threads) continue;
					tasks.ToArray().WaitAnySilently(Token);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (IsPaused) continue;
					T item;
					
					lock(SyncRoot)
					{
						if (IsPaused) continue;
						if (_queue.IsEmpty || !_queue.TryDequeue(out item)) break;
					}

					ScheduledCallback?.Invoke(item);
					Run(item);
				}
			}
			finally
			{
				SignalAndCheck();
			}
		}

		private void SignalAndCheck()
		{
			if (IsDisposed || _countdown == null) return;
			Monitor.Enter(_countdown);

			bool completed;

			try
			{
				if (IsDisposed || _countdown == null) return;
				_countdown.Signal();
			}
			finally
			{
				completed = _countdown is null or { CurrentCount: < 2 };
			}

			if (completed)
			{
				OnWorkCompleted(EventArgs.Empty);
				_countdown.SignalAll();
			}

			if (_countdown != null) Monitor.Exit(_countdown);
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
}