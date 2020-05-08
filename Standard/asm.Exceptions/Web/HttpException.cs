using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Exceptions.Web
{
	/// <devdoc>
	///     <para>
	///         Enables ASP.NET
	///         to send exception information.
	///     </para>
	/// </devdoc>
	[Serializable]
	public class HttpException : ExternalException
	{
		private const int FACILITY_WIN32 = 7;

		private int _webEventCode;

		/// <devdoc>
		///     <para> Default constructor.</para>
		/// </devdoc>
		public HttpException() { }

		/// <devdoc>
		///     <para>
		///         Construct an exception using error message.
		///     </para>
		/// </devdoc>
		public HttpException(string message)
			: base(message)
		{
		}

		/// <devdoc>
		///     <para>Construct an exception using error message and hr.</para>
		/// </devdoc>
		public HttpException(string message, int hr)
			: base(message)
		{
			HResult = hr;
		}

		/// <devdoc>
		///     <para>
		///         Construct an exception using error message, HTTP code,
		///         and innerException
		///         .
		///     </para>
		/// </devdoc>
		public HttpException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <devdoc>
		///     <para>
		///         Construct an exception using HTTP error code, error message,
		///         and innerException
		///         .
		///     </para>
		/// </devdoc>
		public HttpException(int httpStatusCode, string message, Exception innerException)
			: base(message, innerException)
		{
			StatusCode = httpStatusCode;
		}

		/// <devdoc>
		///     <para>
		///         Construct an exception using HTTP error code, error message,
		///         and innerException
		///         .
		///     </para>
		/// </devdoc>
		public HttpException(HttpStatusCode httpStatusCode, string message, Exception innerException)
			: base(message, innerException)
		{
			StatusCode = (int)httpStatusCode;
		}

		/// <devdoc>
		///     <para>
		///         Construct an
		///         exception using HTTP error code and error message.
		///     </para>
		/// </devdoc>
		public HttpException(int httpStatusCode, string message)
			: base(message)
		{
			StatusCode = httpStatusCode;
		}

		/// <devdoc>
		///     <para>
		///         Construct an
		///         exception using HTTP error code and error message.
		///     </para>
		/// </devdoc>
		public HttpException(HttpStatusCode httpStatusCode, string message)
			: base(message)
		{
			StatusCode = (int)httpStatusCode;
		}

		/// <devdoc>
		///     <para>
		///         Construct an exception
		///         using HTTP error code, error message, and hr.
		///     </para>
		/// </devdoc>
		public HttpException(int httpStatusCode, string message, int hr)
			: base(message)
		{
			HResult = hr;
			StatusCode = httpStatusCode;
		}

		/// <devdoc>
		///     <para>
		///         Construct an exception
		///         using HTTP error code, error message, and hr.
		///     </para>
		/// </devdoc>
		public HttpException(HttpStatusCode httpStatusCode, string message, int hr)
			: base(message)
		{
			HResult = hr;
			StatusCode = (int)httpStatusCode;
		}

		/// <devdoc>
		///     <para> Constructor used for deserialization.</para>
		/// </devdoc>
		protected HttpException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			StatusCode = info.GetInt32(nameof(StatusCode));
		}

		internal HttpException(string message, Exception innerException, int code)
			: base(message, innerException)
		{
			_webEventCode = code;
		}

		public int StatusCode { get; }

		public int WebEventCode { get => _webEventCode; internal set => _webEventCode = value; }

		/// <devdoc>
		///     <para>Serialize the object.</para>
		/// </devdoc>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(StatusCode), StatusCode);
		}

		/*
         * If we have an Http code (non-zero), return it.  Otherwise, return
         * the inner exception's code.  If there isn't one, return 500.
         */

		/// <devdoc>
		///     <para>
		///         HTTP return code to send back to client. If there is a
		///         non-zero Http code, it is returned. Otherwise, the System.HttpException.innerException
		///         code is returned. If there isn't an inner exception, error code 500 is returned.
		///     </para>
		/// </devdoc>
		public int GetHttpCode() { return GetHttpCodeForException(this); }

		/// <devdoc>
		///     <para>Creates a new Exception based on the previous Exception. </para>
		/// </devdoc>
		[NotNull]
		public static HttpException CreateFromLastError(string message) { return new HttpException(message, HResultFromLastError(Marshal.GetLastWin32Error())); }

		public static int GetHttpCodeForException([NotNull] Exception e)
		{
			switch (e)
			{
				case HttpException exception:
				{
					HttpException he = exception;

					// If the HttpException specifies an HTTP code, use it
					if (he.StatusCode > 0) return he.StatusCode;
					break;
				}

				case UnauthorizedAccessException _:
					return 401;
				case PathTooLongException _:
					return 414;
			}

			// If there is an inner exception, try to get the code from it
			return e.InnerException != null
						? GetHttpCodeForException(e.InnerException)
						: 500;
		}

		// N.B. The last error code can be lost if we were to 
		// call UnsafeNativeMethods.GetLastError from this function
		// and it were not yet jitted.
		internal static int HResultFromLastError(int lastError)
		{
			int hr;

			if (lastError < 0) hr = lastError;
			else hr = (int)(((uint)lastError & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);

			return hr;
		}
	}
}