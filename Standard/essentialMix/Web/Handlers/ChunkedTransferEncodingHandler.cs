using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Web.Handlers
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
		[ItemNotNull]
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
			response.Headers.TransferEncodingChunked = true;
			return response;
		}
	}
}