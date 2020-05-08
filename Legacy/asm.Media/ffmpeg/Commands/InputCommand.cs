using System;
using asm.Collections;
using asm.Extensions;

namespace asm.Media.ffmpeg.Commands
{
	public class InputCommand : Media.Commands.InputCommand
	{
		public InputCommand()
			: this(null)
		{
		}

		protected InputCommand(string command)
			: base(command)
		{
			Arguments.Add(new Property("input", "-i \"{0}\"", true, true));
		}

		protected override string CollectArgument(IProperty property)
		{
			if (!property.Name.IsSame("input")) return base.CollectArgument(property);
			string fmt = (string)property.Value;
			if (string.IsNullOrWhiteSpace(fmt)) throw new ArgumentNullException(nameof(property.Value));
			if (string.IsNullOrWhiteSpace(Input)) throw new InvalidOperationException(nameof(Input) + " is missing.");
			return string.Format(fmt, Input);
		}
	}
}