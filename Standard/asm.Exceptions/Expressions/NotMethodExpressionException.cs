using System;
using System.Runtime.Serialization;
using asm.Exceptions.Properties;
using JetBrains.Annotations;

namespace asm.Exceptions.Expressions
{
	[Serializable]
	public class NotMethodExpressionException : Exception
	{
		public NotMethodExpressionException()
			: base(Resources.NotMethodExpression)
		{
		}

		public NotMethodExpressionException(string message) : base(message) { }

		public NotMethodExpressionException(string message, Exception innerException) : base(message, innerException) { }

		protected NotMethodExpressionException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}