using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	public sealed class CancellationTokenAwaiter : Disposable
	{
		private IDisposable _tokenRegistration;

		public CancellationTokenAwaiter(CancellationToken token = default(CancellationToken))
		{
			Token = token;

			if (token.IsCancellationRequested)
			{
				Task = Task.FromCanceled(token);
				return;
			}

			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			if (token.CanBeCanceled) _tokenRegistration = Token.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(Token), tcs, false);
			Task = tcs.Task;
		}

		public bool IsCompleted => Token.IsCancellationRequested;

		public CancellationToken Token { get; }
		public Task Task { get; }

		public void GetResult()
		{
			if (Token.IsCancellationRequested) return;

			using (ManualResetEventSlim evt = new ManualResetEventSlim())
				evt.Wait(Token);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _tokenRegistration);
			base.Dispose(disposing);
		}
	}
}