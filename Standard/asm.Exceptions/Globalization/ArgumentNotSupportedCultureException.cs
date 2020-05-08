using System;
using System.Globalization;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using asm.Exceptions.Properties;

namespace asm.Exceptions.Globalization
{
	[Serializable]
	public class ArgumentNotSupportedCultureException : CultureNotFoundException
	{
		/// <inheritdoc />
		public ArgumentNotSupportedCultureException()
			: base(Resources.CultureNotSupported)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string message, Exception innerException) 
			: base(message, innerException)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string message, int invalidCultureId, Exception innerException) 
			: base(message, invalidCultureId, innerException)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string message, string invalidCultureName, Exception innerException) 
			: base(message, invalidCultureName, innerException)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string paramName) 
			: base(paramName, Resources.CultureNotSupported)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string paramName, int invalidCultureId) 
			: base(paramName, invalidCultureId, Resources.CultureNotSupported)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string paramName, string invalidCultureName) 
			: base(paramName, invalidCultureName, Resources.CultureNotSupported)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string paramName, int invalidCultureId, string message) 
			: base(paramName, invalidCultureId, message)
		{
		}

		/// <inheritdoc />
		public ArgumentNotSupportedCultureException(string paramName, string invalidCultureName, string message) 
			: base(paramName, invalidCultureName, message)
		{
		}

		/// <inheritdoc />
		protected ArgumentNotSupportedCultureException([NotNull] SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
	}
}