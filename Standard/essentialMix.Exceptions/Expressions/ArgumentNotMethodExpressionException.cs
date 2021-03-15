using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using essentialMix.Exceptions.Properties;

namespace essentialMix.Exceptions.Expressions
{
	[Serializable]
	public class ArgumentNotMethodExpressionException : ArgumentException
	{
		/// <inheritdoc />
		public ArgumentNotMethodExpressionException() 
			: base(Resources.NotMethodExpression)
		{
		}

		/// <inheritdoc />
		public ArgumentNotMethodExpressionException(string paramName)
			: base(Resources.NotMethodExpression, paramName)
		{
		}

		/// <inheritdoc />
		public ArgumentNotMethodExpressionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <inheritdoc />
		public ArgumentNotMethodExpressionException(string message, string paramName)
			: base(message, paramName)
		{
		}

		/// <inheritdoc />
		public ArgumentNotMethodExpressionException(string message, string paramName, Exception innerException)
			: base(message, paramName, innerException)
		{
		}

		/// <inheritdoc />
		protected ArgumentNotMethodExpressionException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}