using System;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using JetBrains.Annotations;

namespace essentialMix.Web.Exceptions
{
	public class HttpConfigurationMissingException : HttpException
	{
		/// <inheritdoc />
		public HttpConfigurationMissingException()
			: this((int)HttpStatusCode.InternalServerError, "Missing configuration. Could not get the configuration associated with the current request.")
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(string message, int hr) : base(message, hr)
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(int httpCode, string message, Exception innerException) : base(httpCode, message, innerException)
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(int httpCode, string message) : base(httpCode, message)
		{
		}

		/// <inheritdoc />
		public HttpConfigurationMissingException(int httpCode, string message, int hr) : base(httpCode, message, hr)
		{
		}

		/// <inheritdoc />
		protected HttpConfigurationMissingException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
