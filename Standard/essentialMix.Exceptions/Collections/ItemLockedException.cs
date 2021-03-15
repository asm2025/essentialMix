using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections
{
	[Serializable]
	public class ItemLockedException : Exception
	{
		public ItemLockedException()
			: base(Resources.ItemLocked)
		{
		}

		public ItemLockedException(string message) : base(message) { }

		public ItemLockedException(string message, Exception innerException) : base(message, innerException) { }

		protected ItemLockedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}