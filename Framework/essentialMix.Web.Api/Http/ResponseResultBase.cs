using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;

namespace essentialMix.Web.Api.Http
{
	public abstract class ResponseResultBase : IHttpActionResult
	{
		protected ResponseResultBase([NotNull] HttpRequestMessage request)
		{
			Request = request;
		}

		[NotNull]
		protected HttpRequestMessage Request { get; }

		public abstract Task<HttpResponseMessage> ExecuteAsync(CancellationToken token = default(CancellationToken));
	}
}