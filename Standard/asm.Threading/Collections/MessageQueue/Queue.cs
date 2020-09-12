using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Threading.Collections.MessageQueue
{
	public sealed class Queue : QueueBase<object>
	{
		/// <inheritdoc />
		public Queue([NotNull] Action<object> callback, CancellationToken token = default(CancellationToken))
			: this(callback, false, ThreadPriority.Normal, token)
		{
		}

		/// <inheritdoc />
		public Queue([NotNull] Action<object> callback, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: this(callback, false, priority, token)
		{
		}

		/// <inheritdoc />
		public Queue([NotNull] Action<object> callback, bool isBackground, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: base(callback, isBackground, priority, token)
		{
		}
	}

	public sealed class Queue<T> : QueueBase<T>
	{
		/// <inheritdoc />
		public Queue([NotNull] Action<T> callback, CancellationToken token = default(CancellationToken))
			: this(callback, false, ThreadPriority.Normal, token)
		{
		}

		/// <inheritdoc />
		public Queue([NotNull] Action<T> callback, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: this(callback, false, priority, token)
		{
		}

		/// <inheritdoc />
		public Queue([NotNull] Action<T> callback, bool isBackground, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: base(callback, isBackground, priority, token)
		{
		}
	}
}