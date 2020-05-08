using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using asm.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace asm.Core.Web.Mvc
{
	public class HttpResponseMessageResult : ResponseResult
	{
		private HttpResponseMessage _response;

		/// <inheritdoc />
		public HttpResponseMessageResult([NotNull] HttpResponseMessage response)
		{
			_response = response;
		}

		[NotNull]
		protected HttpResponseMessage Response => _response;

		/// <inheritdoc />
		public override Task ExecuteResultAsync([NotNull] ActionContext context)
		{
			HttpResponse response = context.HttpContext.Response;

			try
			{
				response.StatusCode = (int)Response.StatusCode;
				
				IHttpResponseFeature responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
				if (responseFeature != null) responseFeature.ReasonPhrase = Response.ReasonPhrase;

				HttpResponseHeaders headers = Response.Headers;

				// Ignore the Transfer-Encoding header if it is just "chunked".
				// We let the host decide about whether the response should be chunked or not.
				if (headers.TransferEncodingChunked == true && headers.TransferEncoding.Count == 1) headers.TransferEncoding.Clear();

				foreach ((string key, IEnumerable<string> value) in headers)
				{
					response.Headers.Append(key, value.ToArray());
				}

				if (Response.Content == null) return Task.CompletedTask;
				
				HttpContentHeaders contentHeaders = Response.Content.Headers;

				// Copy the response content headers only after ensuring they are complete.
				// We ask for Content-Length first because HttpContent lazily computes this
				// and only afterwards writes the value into the content headers.
				long? unused = contentHeaders.ContentLength;

				foreach ((string key, IEnumerable<string> value) in contentHeaders)
				{
					response.Headers.Append(key, value.ToArray());
				}

				return Response.Content.CopyToAsync(response.Body);

			}
			finally
			{
				ObjectHelper.Dispose(ref _response);
			}
		}
	}
}