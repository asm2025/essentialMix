using System;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public struct TaskItemResult
	{
		public TaskItemResult([NotNull] TaskItem item)
			: this(item, TaskQueueResult.None)
		{
		}

		public TaskItemResult([NotNull] TaskItem item, TaskQueueResult result)
		{
			Item = item;
			Result = result;
		}

		[NotNull]
		public TaskItem Item { get; }
		public TaskQueueResult Result { get; set; }

		public static TaskItemResult FromResult(TaskQueueResult result, TaskItem item = null)
		{
			return new TaskItemResult(item ?? new TaskItem(e => result), result);
		}

		public static TaskItemResult FromException(TaskQueueResult result, Exception exception)
		{
			return new TaskItemResult(new TaskItem(e => result) {Exception = exception}, result);
		}
	}
}