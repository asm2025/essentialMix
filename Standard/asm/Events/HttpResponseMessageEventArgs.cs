using System;
using System.ComponentModel;
using System.Net.Http;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	public class HttpResponseMessageEventArgs : CancelEventArgs
	{
		/// <inheritdoc />
		public HttpResponseMessageEventArgs([NotNull] HttpResponseMessage response)
		{
			Response = response;
		}

		[NotNull]
		public HttpResponseMessage Response { get; }
	}
}