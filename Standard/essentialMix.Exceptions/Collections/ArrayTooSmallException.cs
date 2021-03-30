using System;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Properties;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Collections
{
	[Serializable]
	public class ArrayTooSmallException : Exception
	{
		public ArrayTooSmallException()
			: base(Resources.LBoundLargerThanZero)
		{
		}

		public ArrayTooSmallException(string message) : base(message) { }

		public ArrayTooSmallException(string message, Exception innerException) : base(message, innerException) { }

		protected ArrayTooSmallException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}