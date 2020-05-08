using System.Threading;
using System.Threading.Tasks;
using asm.Patterns.Object;

namespace asm.Threading
{
	public class AsyncLock
	{
		public class Releaser : Disposable
		{
			private readonly AsyncLock _toRelease;

			internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }

			/// <inheritdoc />
			protected override void Dispose(bool disposing)
			{
				if (disposing) _toRelease?._semaphore.Release();
				base.Dispose(disposing);
			}
		}

		private readonly AsyncSemaphore _semaphore;
		private readonly Task<Releaser> _releaser;
		
		/// <inheritdoc />
		public AsyncLock()
		{
			_semaphore = new AsyncSemaphore(1);
			_releaser = Task.FromResult(new Releaser(this));
		}

		public Task<Releaser> LockAsync()
		{
			Task task = _semaphore.CompleteAsync();
			return task.IsCompleted
				? _releaser 
				: task.ContinueWith((_, state) => new Releaser((AsyncLock)state), this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
	}
}