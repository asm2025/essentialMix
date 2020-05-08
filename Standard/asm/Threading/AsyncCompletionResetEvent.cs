using System.Threading.Tasks;

namespace asm.Threading
{
	public abstract class AsyncCompletionResetEvent : AsyncResetEvent
	{
		protected static readonly Task _completed = Task.FromResult(true);

		/// <inheritdoc />
		protected AsyncCompletionResetEvent()
		{
		}

		public abstract Task CompleteAsync();
	}
}