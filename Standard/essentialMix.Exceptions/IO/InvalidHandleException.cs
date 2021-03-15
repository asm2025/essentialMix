using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.IO
{
	[Serializable]
	public class InvalidHandleException : Exception
	{
		public InvalidHandleException()
			: base(Resources.InvalidHandle)
		{
		}

		public InvalidHandleException(string message) : base(message) { }

		public InvalidHandleException(string message, Exception innerException) : base(message, innerException) { }

		protected InvalidHandleException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}