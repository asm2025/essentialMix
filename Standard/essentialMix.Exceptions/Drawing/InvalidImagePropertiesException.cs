using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Drawing
{
	[Serializable]
	public class InvalidImagePropertiesException : Exception
	{
		public InvalidImagePropertiesException()
			: base(Resources.InvalidImageProperties)
		{
		}

		public InvalidImagePropertiesException(string message) : base(message) { }

		public InvalidImagePropertiesException(string message, Exception innerException) : base(message, innerException) { }

		protected InvalidImagePropertiesException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}