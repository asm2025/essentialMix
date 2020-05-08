using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions
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