using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public sealed class TaskGroupQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly Queue<T> _queue = new Queue<T>();
		private ManualResetEventSlim _manualResetEventSlim;
		private Thread _worker;

		public TaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_manualResetEventSlim = new ManualResetEventSlim(false);
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
			StopInternal(WaitOnDispose);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		public override int Count
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock (_lock)
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
				if (millisecondsTimeout < TimeSpanHelper.MINIMUM)
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
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
			_manualResetEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);

			try
			{
				int n = 0;
				Task[] tasks = new Task[Threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					n = 0;

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && n < tasks.Length)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;

							while (_queue.Count > 0 && n < tasks.Length && !IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
							{
								T item = _queue.Dequeue();
								tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
							}
						}
					}

					if (n < tasks.Length) break;
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					lock (_lock)
					{
						while (_queue.Count > 0 && n < tasks.Length && !IsDisposed && !Token.IsCancellationRequested)
						{
							T item = _queue.Dequeue();
							tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
						}
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
				_manualResetEventSlim.Set();
			}
		}
	}
}