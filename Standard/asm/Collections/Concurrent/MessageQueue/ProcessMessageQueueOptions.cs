using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.MessageQueue
{
	public class ProcessMessageQueueOptions
	{
		public ProcessMessageQueueOptions([NotNull] Action<string> onOutput, [NotNull] Action<string> onError, [NotNull] Action<string, DateTime?, int?> onExit)
		{
			OnOutput = onOutput;
			OnError = onError;
			OnExit = onExit;
		}

		public int BufferSize { get; set; } = Constants.GetBufferKB(4);

		public bool IsBackground { get; set; }

		public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
		
		[NotNull]
		public Action<string> OnOutput { get; set; }

		[NotNull]
		public Action<string> OnError { get; set; }

		[NotNull]
		public Action<string, DateTime?, int?> OnExit { get; set; }
	}
}