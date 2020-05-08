using System;

namespace asm.Media.Youtube
{
	public class ClosedCaption
	{
		/// <inheritdoc />
		public ClosedCaption()
		{
		}

		public string Text { get; internal set; }
		public TimeSpan Offset { get; internal set; }
		public TimeSpan Duration { get; internal set; }
	}
}