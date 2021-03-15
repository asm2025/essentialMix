using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections
{
	[Serializable]
	public class BufferNotLargeEnoughException : Exception
	{
		public BufferNotLargeEnoughException()
			: base(Resources.BufferNotLargeEnough)
		{
		}

		public BufferNotLargeEnoughException(string message) : base(message) { }

		public BufferNotLargeEnoughException(string message, Exception innerException) : base(message, innerException) { }

		protected BufferNotLargeEnoughException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}