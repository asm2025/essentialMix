using System;

namespace asm.Media.Youtube.Exceptions
{
	public class VideoRequiresPurchaseException : Exception
	{
		/// <inheritdoc />
		public VideoRequiresPurchaseException()
			: base("The video is a paid Youtube Red video and cannot be processed")
		{
		}
	}
}
