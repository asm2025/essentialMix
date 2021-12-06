using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Web.Api.Http
{
	public class MessageResult : ResponseResultBase
	{
		/// <inheritdoc />
		public MessageResult([NotNull] HttpRequestMessage request, HttpStatusCode statusCode, string message)
			: base(request)
		{
			StatusCode = statusCode;
			Message = message;
		}

		public HttpStatusCode StatusCode { get; }
		public string Message { get; }

		[NotNull]
		public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<HttpResponseMessage>(token)
						: Task.FromResult(Request.CreateResponse(StatusCode, Message ?? string.Empty));
		}
	}
}