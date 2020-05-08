using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Drawing
{
	[Serializable]
	public class ImagePropertiesMismatchException : Exception
	{
		public ImagePropertiesMismatchException()
			: base(Resources.ImagePropertiesMismatch)
		{
		}

		public ImagePropertiesMismatchException(string message) : base(message) { }

		public ImagePropertiesMismatchException(string message, Exception innerException) : base(message, innerException) { }

		protected ImagePropertiesMismatchException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}