namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerThreadNamedQueueOptions : ProducerConsumerThreadQueueOptions
	{
		private string _name;

		public string Name
		{
			get => _name;
			set
			{
				_name = value?.Trim();
				if (string.IsNullOrEmpty(_name)) _name = null;
			}
		}
	}
}