using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Caching
{
	[Serializable]
	public class BufferAcquisitionException : Exception
	{
		public BufferAcquisitionException() {
		}

		public BufferAcquisitionException(string message) : base(message)
		{
		}

		public BufferAcquisitionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BufferAcquisitionException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}