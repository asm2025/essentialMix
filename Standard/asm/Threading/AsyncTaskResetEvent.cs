using System.Collections.Generic;
using System.Threading.Tasks;

namespace asm.Threading
{
	public class AsyncTaskResetEvent : AsyncCompletionResetEvent
	{
		private readonly Queue<TaskCompletionSource<bool>> _taskQueue = new Queue<TaskCompletionSource<bool>>();

		private bool _signaled;

		/// <inheritdoc />
		public AsyncTaskResetEvent()
		{
		}

		public override Task CompleteAsync()
		{
			lock (_taskQueue)
			{
				if (_signaled)
				{
					_signaled = false;
					return _completed;
				}

				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				_taskQueue.Enqueue(tcs);
				return tcs.Task;
			}
		}

		public void Set() { SetInternal(); }

		protected override void SetInternal()
		{
			TaskCompletionSource<bool> toRelease = null;

			lock(_taskQueue)
			{
				if (_taskQueue.Count > 0)
					toRelease = _taskQueue.Dequeue();
				else if (!_signaled)
					_signaled = true;
			}

			toRelease?.SetResult(true);
		}
	}
}