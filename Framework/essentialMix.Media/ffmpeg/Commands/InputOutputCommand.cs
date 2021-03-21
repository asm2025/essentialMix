using System;
using essentialMix.Collections;
using essentialMix.Extensions;

namespace essentialMix.Media.ffmpeg.Commands
{
	public class InputOutputCommand : InputCommand
	{
		public InputOutputCommand()
			: this(null)
		{
		}

		protected InputOutputCommand(string command)
			: base(command)
		{
			Arguments.Add(new Property("overwrite", "-y", true, true));
		}

		public string Output { get; set; }
		public bool Overwrite { get; set; }

		protected override void OnCollectingArguments()
		{
			base.OnCollectingArguments();
			if (Arguments.Contains("output")) Arguments.Remove("output");
		}

		protected override void OnCollectedArguments()
		{
			base.OnCollectedArguments();
			Arguments.Add(new Property("output", "\"{0}\""));
		}

		protected override string CollectArgument(IProperty property)
		{
			if (!property.Name.IsSame("output")) return base.CollectArgument(property);
			if (string.IsNullOrWhiteSpace((string)property.Value)) throw new ArgumentNullException(nameof(property.Value));
			if (string.IsNullOrWhiteSpace(Output)) throw new InvalidOperationException(nameof(Output) + " is missing.");
			return string.Format((string)property.Value, Output);
		}
	}
}