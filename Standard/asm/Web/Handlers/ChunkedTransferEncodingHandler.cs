using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Web.Handlers
{
	public class ChunkedTransferEncodingHandler : DelegatingHandler
	{
		/// <inheritdoc />
		public ChunkedTransferEncodingHandler() 
		{
		}

		/// <inheritdoc />
		public ChunkedTransferEncodingHandler(HttpMessageHandler innerHandler) 
			: base(innerHandler)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Task<HttpResponseMessage> response = base.SendAsync(request, cancellationToken);
			response.Result.Headers.TransferEncodingChunked = true;
			return response;
		}
	}
}