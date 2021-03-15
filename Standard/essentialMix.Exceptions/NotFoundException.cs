using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions
{
	[Serializable]
	public class NotFoundException : Exception
	{
		public NotFoundException()
			: base(Resources.NotFound)
		{
		}

		public NotFoundException(string message) : base(message) { }

		public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

		protected NotFoundException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}