using System;
using System.Net.Http;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	public class HttpRequestExceptionEventArgs : ExceptionEventArgs
	{
		/// <inheritdoc />
		public HttpRequestExceptionEventArgs(HttpRequestMessage request, [NotNull] Exception exception)
			: base(exception)
		{
			Request = request;
		}

		public HttpRequestMessage Request { get; }
		public bool Handled { get; set; }
	}
}