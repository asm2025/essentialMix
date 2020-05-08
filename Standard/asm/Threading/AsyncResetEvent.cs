namespace asm.Threading
{
	public abstract class AsyncResetEvent
	{
		/// <inheritdoc />
		protected AsyncResetEvent()
		{
		}

		protected abstract void SetInternal();
	}
}