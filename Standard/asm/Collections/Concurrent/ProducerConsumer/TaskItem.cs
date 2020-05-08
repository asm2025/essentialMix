using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class TaskItem
	{
		public TaskItem([NotNull] Func<TaskItem, TaskQueueResult> onExecute)
		{
			OnExecute = onExecute;
		}

		[NotNull]
		public Func<TaskItem, TaskQueueResult> OnExecute { get; }
		public object State { get; set; }
		public Func<TaskItem, TaskQueueResult, bool> OnResult { get; set; }
		public Action<TaskItem> OnCleanUp { get; set; }
		public Exception Exception { get; internal set; }
		public CancellationToken Token { get; internal set; }
	}
}