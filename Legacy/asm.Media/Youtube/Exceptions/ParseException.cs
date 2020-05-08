using System;

namespace asm.Media.Youtube.Exceptions
{
	public class ParseException : Exception
	{
		public ParseException(string message)
			: base(message)
		{
		}
	}
}