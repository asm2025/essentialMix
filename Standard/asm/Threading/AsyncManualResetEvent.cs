using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;

namespace asm.Threading
{
	public class AsyncManualResetEvent : AsyncCompletionResetEvent
	{
		private volatile TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();

		/// <inheritdoc />
		public AsyncManualResetEvent()
		{
		}

		public override Task CompleteAsync() { return _taskCompletionSource.Task; }

		public void Set() { SetInternal(); }

		public void Reset()
		{
			SpinWait.SpinUntil(() =>
			{
				TaskCompletionSource<bool> tcs = _taskCompletionSource;
				return tcs.Task.IsFinished() || Interlocked.CompareExchange(ref _taskCompletionSource, new TaskCompletionSource<bool>(), tcs) == tcs;
			});
		}

		protected override void SetInternal()
		{
			TaskCompletionSource<bool> tcs = _taskCompletionSource;

			Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
				tcs,
				CancellationToken.None,
				TaskCreationOptions.PreferFairness,
				TaskScheduler.Default);
			tcs.Task.Wait();
		}
	}
}