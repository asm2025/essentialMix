namespace essentialMix.Media.Commands
{
	public class InputCommand : Command
	{
		public InputCommand()
			: this(null)
		{
		}

		protected InputCommand(string command)
			: base(command)
		{
		}

		public string Input { get; set; }
	}
}