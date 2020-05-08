using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Caching
{
	[Serializable]
	public class NotSafeToCacheException : InvalidCastException
	{
		public NotSafeToCacheException()
			: base(Resources.NotSafeToCache)
		{
		}

		public NotSafeToCacheException(object value)
			: base(value is null ? Resources.NotSafeToCache : string.Format(Resources.NotSafeTypeToCache, value.GetType()))
		{
		}

		public NotSafeToCacheException(string message, int errorCode)
			: base(message, errorCode)
		{
		}

		public NotSafeToCacheException(string message)
			: base(message)
		{
		}

		public NotSafeToCacheException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected NotSafeToCacheException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}