using System;
using System.Net;
using System.Text;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web
{
	public struct ResponseStatus
	{
		private HttpStatusCode _statusCode;
		private Exception _exception;
		private string _toString;

		public HttpStatusCode StatusCode
		{
			get => _statusCode;
			set
			{
				_statusCode = value;
				_toString = null;
			}
		}

		public Exception Exception
		{
			get => _exception;
			set
			{
				_exception = value;
				_toString = null;
			}
		}

		/// <inheritdoc />
		[NotNull]
		public override string ToString()
		{
			return _toString ??= Format(this);
		}

		[NotNull]
		private static string Format(ResponseStatus status)
		{
			StringBuilder sb = new StringBuilder($"{nameof(StatusCode)}: {(int)status.StatusCode}");
			if (status.Exception != null) sb.AppendWithLine($"{nameof(Exception)}: {status.Exception.CollectMessages()}");
			return sb.ToString();
		}
	}
}
