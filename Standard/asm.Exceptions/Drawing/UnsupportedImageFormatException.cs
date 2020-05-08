using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Drawing
{
	[Serializable]
	public class UnsupportedImageFormatException : Exception
	{
		public UnsupportedImageFormatException()
			: base(Resources.UnsupportedImageFormat)
		{
		}

		public UnsupportedImageFormatException(string message) : base(message) { }

		public UnsupportedImageFormatException(string message, Exception innerException) : base(message, innerException) { }

		protected UnsupportedImageFormatException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}