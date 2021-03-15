using System;
using System.Globalization;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using essentialMix.Exceptions.Properties;

namespace essentialMix.Exceptions.Globalization
{
	[Serializable]
	public class CultureNotSupportedException : CultureNotFoundException
	{
		/// <inheritdoc />
		public CultureNotSupportedException()
			: base(Resources.CultureNotSupported)
		{
		}

		/// <inheritdoc />
		public CultureNotSupportedException(string message) 
			: base(message)
		{
		}

		/// <inheritdoc />
		public CultureNotSupportedException(string message, Exception innerException) 
			: base(message, innerException)
		{
		}

		/// <inheritdoc />
		public CultureNotSupportedException(string message, int invalidCultureId, Exception innerException) 
			: base(message, invalidCultureId, innerException)
		{
		}

		/// <inheritdoc />
		public CultureNotSupportedException(string message, string invalidCultureName, Exception innerException) 
			: base(message, invalidCultureName, innerException)
		{
		}

		/// <inheritdoc />
		protected CultureNotSupportedException([NotNull] SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
	}
}