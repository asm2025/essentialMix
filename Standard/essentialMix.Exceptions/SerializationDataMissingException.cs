using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace essentialMix.Exceptions
{
	[Serializable]
	public class SerializationDataMissingException : SerializationException
	{
		/// <inheritdoc />
		public SerializationDataMissingException()
		{
		}

		/// <inheritdoc />
		protected SerializationDataMissingException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public SerializationDataMissingException(string message)
			: base(message)
		{
		}

		/// <inheritdoc />
		public SerializationDataMissingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}