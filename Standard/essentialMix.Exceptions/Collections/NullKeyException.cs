using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections
{
	[Serializable]
	public class NullKeyException : Exception
	{
		public NullKeyException()
			: base(Resources.KeyIsNull)
		{
		}

		public NullKeyException(string message) : base(message) { }

		public NullKeyException(string message, Exception innerException) : base(message, innerException) { }

		protected NullKeyException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}