using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Web.Handlers
{
	public sealed class RedirectHandler : HttpMessageHandler
	{
		private readonly Uri _redirectPath;

		/// <inheritdoc />
		public RedirectHandler([NotNull] string redirectPath)
			: this(UriHelper.ToUri(redirectPath) ?? new Uri("/"))
		{
		}

		/// <inheritdoc />
		public RedirectHandler([NotNull] Uri redirectPath) 
		{
			_redirectPath = redirectPath;
		}

		/// <inheritdoc />
		[NotNull]
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Uri redirectUri = _redirectPath.IsAbsoluteUri
								? _redirectPath
								: UriHelper.Combine(UriHelper.GetHostUrl(request.RequestUri), _redirectPath);
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Moved);
			response.Headers.Location = redirectUri;
			return Task.FromResult(response);
		}
	}
}