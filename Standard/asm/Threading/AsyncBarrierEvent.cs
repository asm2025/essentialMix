using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace asm.Threading
{
	public class AsyncBarrierEvent : AsyncResetEvent
	{
		private ConcurrentStack<TaskCompletionSource<bool>> _waiters = new ConcurrentStack<TaskCompletionSource<bool>>();

		private int _remainingParticipants;

		/// <inheritdoc />
		public AsyncBarrierEvent()
			: this(1)
		{
		}

		/// <inheritdoc />
		public AsyncBarrierEvent(int participantCount)
		{
			if (participantCount <= 0) throw new ArgumentOutOfRangeException(nameof(participantCount));
			ParticipantCount = _remainingParticipants = participantCount;
		}

		public int ParticipantCount { get; }

		public int RemainingParticipants => _remainingParticipants;

		public Task SignalAndWait()
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			_waiters.Push(tcs);
			SetInternal();
			return tcs.Task;
		}

		protected override void SetInternal()
		{
			if (Interlocked.Decrement(ref _remainingParticipants) != 0) return;
			_remainingParticipants = ParticipantCount;

			ConcurrentStack<TaskCompletionSource<bool>> waiters = _waiters;
			_waiters = new ConcurrentStack<TaskCompletionSource<bool>>();
			Parallel.ForEach(waiters, w => w.SetResult(true));
		}
	}
}