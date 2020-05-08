using System;
using System.Threading;
using System.Threading.Tasks;

namespace asm.Threading
{
	public class AsyncCountdownEvent : AsyncManualResetEvent
	{
		private int _count;

		/// <inheritdoc />
		public AsyncCountdownEvent()
			: this(1)
		{
		}

		/// <inheritdoc />
		public AsyncCountdownEvent(int initialCount)
		{
			if (initialCount <= 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
			_count = initialCount;
		}

		public int Count => _count;

		public void Signal() { SetInternal(); }

		public Task SignalAndWait()
		{
			SetInternal();
			return CompleteAsync();
		}

		protected override void SetInternal()
		{
			if (_count <= 0) throw new InvalidOperationException();

			int newCount = Interlocked.Decrement(ref _count);
			if (newCount < 0) throw new InvalidOperationException();
			if (newCount == 0) base.SetInternal();
		}
	}
}