using System;

namespace asm.Media.Youtube.Exceptions
{
	public class VideoNotAvailableException : Exception
	{
		public VideoNotAvailableException()
			: this(-1, null)
		{
		}

		public VideoNotAvailableException(string reason)
			: this(-1, reason)
		{
		}

		public VideoNotAvailableException(int code, string reason)
			: base("The video is not available.")
		{
			Code = code;
			Reason = reason;
		}

		public int Code { get; }

		public string Reason { get; }
	}
}