using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections
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