using System;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerThresholdQueueOptions : ProducerConsumerThreadQueueOptions
	{
		private TimeSpan _threshold = TimeSpan.Zero;

		public TimeSpan Threshold
		{
			get => _threshold;
			set
			{
				if (value < TimeSpan.Zero) value = TimeSpan.Zero;
				_threshold = value;
			}
		}
	}
}