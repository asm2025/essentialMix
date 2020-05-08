using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asm.Threading
{
	public class AsyncSemaphore : AsyncCompletionResetEvent
	{
		private readonly Queue<TaskCompletionSource<bool>> _taskQueue = new Queue<TaskCompletionSource<bool>>();

		/// <inheritdoc />
		public AsyncSemaphore()
			: this(1)
		{
		}

		/// <inheritdoc />
		public AsyncSemaphore(int initialCount)
		{
			if (initialCount <= 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
			Count = initialCount;
		}

		public int Count { get; private set; }

		public override Task CompleteAsync()
		{
			lock (_taskQueue)
			{
				if (Count > 0)
				{
					--Count;
					return _completed;
				}

				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				_taskQueue.Enqueue(tcs);
				return tcs.Task;
			}
		}

		public void Release() { SetInternal(); }

		protected override void SetInternal()
		{
			TaskCompletionSource<bool> toRelease = null;

			lock (_taskQueue)
			{
				if (_taskQueue.Count > 0)
					toRelease = _taskQueue.Dequeue();
				else
					++Count;
			}

			toRelease?.SetResult(true);
		}
	}
}