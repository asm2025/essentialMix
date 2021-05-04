using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	/// <summary>
	/// Not recommended because it's way too expensive unless you know what you're doing and need to
	/// have items run on their own threads.
	/// </summary>
	public sealed class SemaphoreQueue<T> : NamedProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

		private CountdownEvent _countdown;
		private ManualResetEvent _allWorkDone;
		private Semaphore _semaphore;
		private Thread _worker;

		public SemaphoreQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			bool createdNew;
			string name = Name.ToNullIfEmpty()?.LeftMax(Win32.MAX_PATH);
			
			if (name == null)
			{
				_semaphore = new Semaphore(Threads, Threads, null, out createdNew);
			}
			else
			{
				try
				{
					_semaphore = Semaphore.OpenExisting(name);
					createdNew = false;
				}
				catch (WaitHandleCannotBeOpenedException)
				{
					_semaphore = new Semaphore(Threads, Threads, name, out createdNew);
				}
			}

			if (createdNew && options is SemaphoreQueueOptions<T> { Security: { } } semaphoreQueueOptions)
			{
				// see https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphore.openexisting for examples
				_semaphore.SetAccessControl(semaphoreQueueOptions.Security);
			}

			IsOwner = createdNew;
			_allWorkDone = new ManualResetEvent(false);
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

			if (_semaphore != null)
			{
				_semaphore.Close();
				ObjectHelper.Dispose(ref _semaphore);
			}

			ObjectHelper.Dispose(ref _countdown);
			ObjectHelper.Dispose(ref _allWorkDone);
		}

		public override int Count
		{
			get
			{
				int threads = _countdown?.CurrentCount ?? 0;
				if (threads > 1) threads--;
				return _queue.Count + threads;
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.Enqueue(item);
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();
			return _queue.TryDequeue(out item);
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			ThrowIfDisposed();
			return _queue.TryPeek(out item);
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
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _allWorkDone.WaitOne(millisecondsTimeout, Token);
				_allWorkDone.WaitOne(Token);
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
			ObjectHelper.Dispose(ref _countdown);
			ObjectHelper.Dispose(ref _worker);
		}

		private void Consume()
		{
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);

			IList<Thread> threads = new List<Thread>(Threads);

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!_queue.TryDequeue(out item)) continue;

					Thread thread = new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					};

					if (_countdown == null) _countdown = new CountdownEvent(2);
					else _countdown.AddCount();
					thread.Start(item);
					threads.Add(thread);
					if (threads.Count < Threads) continue;
					_countdown.Signal();
					_countdown.Wait(Token);
					ClearThreads(threads);
					ObjectHelper.Dispose(ref _countdown);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					Thread thread = new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					};

					if (_countdown == null) _countdown = new CountdownEvent(2);
					else _countdown.AddCount();
					thread.Start(item);
					threads.Add(thread);
					if (threads.Count < Threads) continue;
					_countdown.Signal();
					_countdown.Wait(Token);
					ClearThreads(threads);
					ObjectHelper.Dispose(ref _countdown);
				}

				if (_countdown is not { CurrentCount: > 1 }) return;
				_countdown.Signal();
				_countdown.Wait(Token);
			}
			finally
			{
				ClearThreads(threads);
				ObjectHelper.Dispose(ref _countdown);
				OnWorkCompleted(EventArgs.Empty);
				_allWorkDone.Set();
			}

			static void ClearThreads(IList<Thread> threads)
			{
				for (int i = threads.Count - 1; i >= 0; i--)
				{
					Thread thread = threads[i];
					ObjectHelper.Dispose(ref thread);
					threads.RemoveAt(i);
				}
			}
		}

		private void RunThread(object rawValue)
		{
			if (IsDisposed) return;

			if (Token.IsCancellationRequested)
			{
				_countdown?.Signal();
				return;
			}

			bool entered = false;

			try
			{
				if (!_semaphore.WaitOne(Token)) return;
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				Run((T)rawValue);
			}
			finally
			{
				if (entered) _semaphore?.Release();
				_countdown?.Signal();
			}
		}
	}
}