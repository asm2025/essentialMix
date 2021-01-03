using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class SemaphoreQueue<T> : NamedProducerConsumerThreadQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly List<Thread> _running = new List<Thread>();

		private ManualResetEventSlim _manualResetEventSlim;
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

			if (createdNew && options is SemaphoreQueueOptions<T> semaphoreQueueOptions && semaphoreQueueOptions.Security != null)
			{
				// see https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphore.openexisting for examples
				_semaphore.SetAccessControl(semaphoreQueueOptions.Security);
			}

			IsOwner = createdNew;
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
			ObjectHelper.Dispose(ref _semaphore);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		public override int Count => _queue.Count + RunningCount();

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
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.TryDequeue(out item))
				{
					T value = item;
					Thread thread = new Thread(() => Run(value))
					{
						IsBackground = IsBackground,
						Priority = Priority
					};
					AddRunning(thread);
					thread.Start();
				}

				Thread.Sleep(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					T value = item;
					Thread thread = new Thread(() => Run(value))
					{
						IsBackground = IsBackground,
						Priority = Priority
					};
					AddRunning(thread);
					thread.Start();
				}
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				Thread[] threads;

				lock(_running)
				{
					threads = _running.Count > 0
								? _running.ToArray()
								: null;
				}

				if (threads != null)
				{
					foreach (Thread thread in threads) 
						thread.Join();
				}

				OnWorkCompleted(EventArgs.Empty);
				_manualResetEventSlim.Set();
			}
		}

		protected override void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			bool entered = false;

			try
			{
				if (!_semaphore.WaitOne(Token)) return;
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				base.Run(item);
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				RemoveRunning(Thread.CurrentThread);
				if (entered) _semaphore.Release();
			}
		}

		private void AddRunning(Thread thread)
		{
			lock(_running) 
				_running.Add(thread);
		}

		private void RemoveRunning(Thread thread)
		{
			lock(_running) 
				_running.Remove(thread);
		}

		private int RunningCount()
		{
			lock(_running) 
				return _running.Count;
		}
	}
}