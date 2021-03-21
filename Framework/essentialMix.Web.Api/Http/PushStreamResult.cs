using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JetBrains.Annotations;
using essentialMix.Extensions;

namespace essentialMix.Web.Api.Http
{
	public abstract class PushStreamResult : ResponseResult
	{
		/// <inheritdoc />
		protected PushStreamResult([NotNull] HttpRequestMessage request)
			: this(request, null, null)
		{
		}

		/// <inheritdoc />
		protected PushStreamResult([NotNull] HttpRequestMessage request, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, null, onError)
		{
		}

		/// <inheritdoc />
		protected PushStreamResult([NotNull] HttpRequestMessage request, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError) 
			: base(request)
		{
			OnCompleted = onCompleted;
			OnError = onError;
		}

		protected Action OnCompleted { get; }
		protected Action<Exception, Stream, HttpContent, TransportContext> OnError { get; }

		/// <inheritdoc />
		[NotNull]
		public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken token = default(CancellationToken))
		{
			HttpResponseMessage response = Request.CreateResponse();
			response.Content = new PushStreamContent(async (stream, content, context) =>
			{
				try
				{
					await WriteContent(response, stream, content, context, token);
				}
				catch (HttpException ex)
				{
					// The remote host closed the connection.
					if (ex.ErrorCode == Constants.REMOTE_CONNECTION_CLOSED) return;
					response.StatusCode = HttpStatusCode.InternalServerError;

					if (OnError != null)
					{
						OnError(ex, stream, content, context);
					}
					else
					{
						using (StreamWriter writer = new StreamWriter(stream))
						{
							await writer.WriteAsync(ex.CollectMessages());
						}
					}
				}
				catch (Exception e)
				{
					response.StatusCode = HttpStatusCode.InternalServerError;

					if (OnError != null)
					{
						OnError(e, stream, content, context);
					}
					else
					{
						using (StreamWriter writer = new StreamWriter(stream))
						{
							await writer.WriteAsync(e.CollectMessages());
						}
					}
				}
				finally
				{
					stream.Close();
					OnCompleted?.Invoke();
				}
			});
			
			return token.IsCancellationRequested
				? Task.FromCanceled<HttpResponseMessage>(token)
				: Task.FromResult(response);
		}

		protected abstract Task WriteContent(HttpResponseMessage response, Stream stream, HttpContent content, TransportContext context, CancellationToken token = default(CancellationToken));
	}
}