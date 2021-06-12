using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public sealed class ThresholdTaskGroupQueue<T> : ProducerConsumerThresholdQueue<T>, IProducerQueue<T>
	{
		private readonly Queue<T> _queue;
		private readonly ICollection _collection;

		private CountdownEvent _countdown;
		private Thread _worker;

		public ThresholdTaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new Queue<T>();
			_collection = _queue;
			_countdown = new CountdownEvent(1);
			(_worker = new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}).Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _countdown);
		}

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		public override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			lock (SyncRoot)
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

			if (_queue.Count == 0)
			{
				item = default(T);
				return false;
			}
			
			lock(SyncRoot)
			{
				ThrowIfDisposed();

				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Dequeue();
				Monitor.Pulse(SyncRoot);
				return true;
			}
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			ThrowIfDisposed();

			if (_queue.Count == 0)
			{
				item = default(T);
				return false;
			}
			
			lock(SyncRoot)
			{
				ThrowIfDisposed();

				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Peek();
				return true;
			}
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();
			if (_queue.Count == 0) return;

			lock(SyncRoot)
			{
				ThrowIfDisposed();
				if (_queue.Count == 0) return;

				int n = 0;

				while (_queue.Count > 0)
				{
					T item = _queue.Peek();
					if (!predicate(item)) break;
					_queue.Dequeue();
					n++;
				}

				if (n == 0) return;
				Monitor.Pulse(SyncRoot);
			}
		}

		protected override void CompleteInternal()
		{
			lock(SyncRoot)
			{
				CompleteMarked = true;
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected override void ClearInternal()
		{
			lock (SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected override bool WaitInternal(int millisecondsTimeout)
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
			}
			catch (TimeoutException)
			{
			}
			catch (AggregateException ag) when (ag.InnerException is OperationCanceledException || ag.InnerException is TimeoutException)
			{
			}

			return false;
		}

		protected override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			Cancel();
			ClearInternal();
			ObjectHelper.Dispose(ref _worker);
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
							if (_queue.Count == 0) Monitor.Wait(SyncRoot, TimeSpanHelper.FAST_SCHEDULE);
							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
						}

						while (_queue.Count > 0 && count < tasks.Length && !IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
						{
							T item = _queue.Dequeue();
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

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					if (HasThreshold && count == 0) tasks[count++] = Task.Delay(Threshold).ConfigureAwait();

					while (_queue.Count > 0 && count < tasks.Length && !IsDisposed && !Token.IsCancellationRequested)
					{
						T item = _queue.Dequeue();
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
}