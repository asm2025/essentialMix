using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Collections
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