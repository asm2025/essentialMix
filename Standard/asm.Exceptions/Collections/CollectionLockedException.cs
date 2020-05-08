using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Collections
{
	[Serializable]
	public class CollectionLockedException : Exception
	{
		public CollectionLockedException()
			: base(Resources.CollectionLocked)
		{
		}

		public CollectionLockedException(string message) : base(message) { }

		public CollectionLockedException(string message, Exception innerException) : base(message, innerException) { }

		protected CollectionLockedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}