using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;

namespace asm.Web.Api.Http
{
	public abstract class ResponseResult : IHttpActionResult
	{
		protected ResponseResult([NotNull] HttpRequestMessage request)
		{
			Request = request;
		}

		[NotNull]
		protected HttpRequestMessage Request { get; }

		public abstract Task<HttpResponseMessage> ExecuteAsync(CancellationToken token = default(CancellationToken));
	}
}