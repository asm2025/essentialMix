using System;
using asm.Extensions;
using asm.Helpers;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerQueueOptions
	{
		private int _threads = TaskHelper.ProcessDefault;

		public int Threads
		{
			get => _threads;
			set
			{
				if (value == 0) value = TaskHelper.ProcessDefault;
				if (!value.InRange(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(Threads)} must fall within the range of {TaskHelper.QueueMinimum} and {TaskHelper.QueueMaximum}.");
				_threads = value;
			}
		}
	}
}