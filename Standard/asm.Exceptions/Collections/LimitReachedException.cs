using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Collections
{
	[Serializable]
	public class LimitReachedException : Exception
	{
		public LimitReachedException()
			: base(Resources.LimitReached)
		{
		}

		public LimitReachedException(string message) : base(message) { }

		public LimitReachedException(string message, Exception innerException) : base(message, innerException) { }

		protected LimitReachedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}