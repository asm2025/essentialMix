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
	public class ThresholdTaskGroupQueue<TQueue, T> : ProducerConsumerThresholdQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		private CountdownEvent _countdown;

		public ThresholdTaskGroupQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
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
		protected sealed override void Dispose(bool disposing)
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
			{
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
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
			return _queue.TryPeek(out item);
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();

			lock(SyncRoot)
			{
				int n = 0;

				while (_queue.TryPeek(out T item) && predicate(item))
				{
					if (!_queue.TryDequeue(out _)) continue;
					n++;
				}

				if (n == 0) return;
				Monitor.Pulse(SyncRoot);
			}
		}

		protected sealed override void CompleteInternal()
		{
			lock(SyncRoot)
			{
				CompleteMarked = true;
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected sealed override void ClearInternal()
		{
			lock(SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
			}
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
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);

			try
			{
				int count = 0;
				int threads = Threads;
				if (HasThreshold) threads++;
		
				Task[] tasks = new Task[threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					count = 0;
					if (HasThreshold) tasks[count++] = Task.Delay(Threshold).ConfigureAwait();

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && count < tasks.Length)
					{
						lock(SyncRoot)
						{
							if (_queue.IsEmpty) Monitor.Wait(SyncRoot, TimeSpanHelper.FAST);
							if (IsDisposed || Token.IsCancellationRequested || _queue.IsEmpty) continue;
						}

						while (count < tasks.Length && !IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsEmpty)
						{
							T item;

							lock(SyncRoot)
							{
								if (_queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
							}

							_countdown.AddCount();
							tasks[count++] = Task.Run(() =>
							{
								try
								{
									Run(item);
								}
								finally
								{
									_countdown?.SignalOne();
								}
							}, Token);
						}
					}

					if (count < tasks.Length) break;
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;

					for (int i = 0; i < tasks.Length; i++)
						ObjectHelper.Dispose(ref tasks[i]);

					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
				{
					if (HasThreshold && count == 0) tasks[count++] = Task.Delay(Threshold).ConfigureAwait();

					while (count < tasks.Length && !IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
					{
						T item;

						lock(SyncRoot)
						{
							if (_queue.IsEmpty || !_queue.TryDequeue(out item)) break;
						}

						_countdown.AddCount();
						tasks[count++] = Task.Run(() =>
						{
							try
							{
								Run(item);
							}
							finally
							{
								_countdown?.SignalOne();
							}
						}, Token);
					}

					if (count < tasks.Length) break;
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;

					for (int i = 0; i < tasks.Length; i++)
						ObjectHelper.Dispose(ref tasks[i]);

					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				if (count < 1 || IsDisposed || Token.IsCancellationRequested) return;
				Array.Resize(ref tasks, count);
				tasks.WaitSilently(Token);
				
				for (int i = 0; i < tasks.Length; i++)
					ObjectHelper.Dispose(ref tasks[i]);
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_countdown.SignalAll();
			}
		}
	}

	/// <inheritdoc cref="ThresholdTaskGroupQueue{TQueue,T}"/>
	public sealed class ThresholdTaskGroupQueue<T> : ThresholdTaskGroupQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public ThresholdTaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}