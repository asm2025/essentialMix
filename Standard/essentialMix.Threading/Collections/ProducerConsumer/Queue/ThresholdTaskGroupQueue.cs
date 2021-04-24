using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class ThresholdTaskGroupQueue<T> : ProducerConsumerThresholdQueue<T>, IProducerQueue<T>
	{
		private readonly object _lock = new object();
		private readonly Queue<T> _queue = new Queue<T>();

		private AutoResetEvent _workDoneEvent;
		private Thread _worker;

		public ThresholdTaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_workDoneEvent = new AutoResetEvent(false);
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
			ObjectHelper.Dispose(ref _workDoneEvent);
		}

		public override int Count
		{
			get
			{
				lock(_lock)
				{
					return _queue.Count;
				}
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			lock (_lock)
			{
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
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
			
			lock(_lock)
			{
				ThrowIfDisposed();

				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Dequeue();
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
			
			lock(_lock)
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

		protected override void CompleteInternal()
		{
			lock(_lock)
			{
				CompleteMarked = true;
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock (_lock)
			{
				_queue.Clear();
				Monitor.PulseAll(_lock);
			}
		}

		protected override bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;
			
			try
			{
				if (millisecondsTimeout < TimeSpanHelper.ZERO)
					_workDoneEvent.WaitOne(Token);
				else if (!_workDoneEvent.WaitOne(millisecondsTimeout, Token))
					return false;

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
				int n = 0;
				int threads = Threads;
				if (HasThreshold) threads++;
				Task[] tasks = new Task[threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					n = 0;
					if (HasThreshold) tasks[n++] = Task.Delay(Threshold).ConfigureAwait();

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && n < tasks.Length)
					{
						lock(_lock)
						{
							if (_queue.Count == 0) Monitor.Wait(_lock, TimeSpanHelper.FAST_SCHEDULE);
							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
						}

						while (_queue.Count > 0 && n < tasks.Length && !IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
						{
							T item = _queue.Dequeue();
							tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
						}
					}

					if (n < tasks.Length) break;
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					if (HasThreshold && n == 0) tasks[n++] = Task.Delay(Threshold).ConfigureAwait();

					while (_queue.Count > 0 && n < tasks.Length && !IsDisposed && !Token.IsCancellationRequested)
					{
						T item = _queue.Dequeue();
						tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
					}

					if (n < tasks.Length) break;
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
					n = 0;
				}

				if (n < 1 || IsDisposed || Token.IsCancellationRequested) return;
				Array.Resize(ref tasks, ++n);
				tasks.WaitSilently(Token);
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_workDoneEvent.Set();
			}
		}
	}
}