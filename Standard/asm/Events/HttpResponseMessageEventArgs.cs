using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	[ComVisible(true)]
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