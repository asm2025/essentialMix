using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class TaskGroupQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private AutoResetEvent _workDoneEvent;
		private Thread _worker;

		public TaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
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

		public override int Count => _queue.Count;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.Enqueue(item);
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected override void ClearInternal()
		{
			_queue.Clear();
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
				int count = 0;
				T[] items = new T[Threads];
				Task[] tasks = new Task[Threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!ReadItems(items, ref count) || count < items.Length) break;

					for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < items.Length; i++)
					{
						T item = items[i];
						tasks[i] = Task.Run(() => Run(item), Token).ConfigureAwait();
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				if (count == items.Length) count = 0;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (!ReadItems(items, ref count) || count < items.Length) break;

					for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < items.Length; i++)
					{
						T item = items[i];
						tasks[i] = Task.Run(() => Run(item), Token).ConfigureAwait();
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				if (count < 1 || IsDisposed || Token.IsCancellationRequested) return;
				Array.Resize(ref tasks, ++count);

				for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < tasks.Length; i++)
				{
					T item = items[i];
					tasks[i] = Task.Run(() => Run(item), Token).ConfigureAwait();
				}

				tasks.WaitSilently(Token);
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_workDoneEvent.Set();
			}
		}

		private bool ReadItems([NotNull] T[] items, ref int offset)
		{
			if (IsDisposed || Token.IsCancellationRequested) return false;
			int count = items.Length - offset;
			if (count < 1) return false;

			int read = 0;

			while (count > 0 && _queue.TryDequeue(out T item))
			{
				items[offset++] = item;
				count--;
				read++;
			}

			return !IsDisposed && !Token.IsCancellationRequested && read > 0;
		}
	}
}