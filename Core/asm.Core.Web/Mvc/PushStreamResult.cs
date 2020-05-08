using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using asm.Exceptions.Web;
using asm.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace asm.Core.Web.Mvc
{
	public abstract class PushStreamResult : ResponseResult
	{
		/// <inheritdoc />
		protected PushStreamResult()
			: this(null, null)
		{
		}

		/// <inheritdoc />
		protected PushStreamResult(Action<Exception, ActionContext> onError)
			: this(null, onError)
		{
		}

		/// <inheritdoc />
		protected PushStreamResult(Action onCompleted, Action<Exception, ActionContext> onError) 
		{
			OnCompleted = onCompleted;
			OnError = onError;
		}

		protected virtual Action OnCompleted { get; }
		protected virtual Action<Exception, ActionContext> OnError { get; }

		/// <inheritdoc />
		public override async Task ExecuteResultAsync([NotNull] ActionContext context)
		{
			HttpResponse response = context.HttpContext.Response;

			try
			{
				await WriteContent(context, context.HttpContext.RequestAborted);
			}
			catch (HttpException ex)
			{
				// The remote host closed the connection.
				if (ex.ErrorCode == Constants.REMOTE_CONNECTION_CLOSED) return;
				response.StatusCode = (int)HttpStatusCode.InternalServerError;

				if (OnError != null)
				{
					OnError(ex, context);
				}
				else
				{
					await using (StreamWriter writer = new StreamWriter(response.Body))
					{
						await writer.WriteAsync(ex.CollectMessages());
					}
				}
			}
			catch (Exception e)
			{
				response.StatusCode = (int)HttpStatusCode.InternalServerError;

				if (OnError != null)
				{
					OnError(e, context);
				}
				else
				{
					await using (StreamWriter writer = new StreamWriter(response.Body))
					{
						await writer.WriteAsync(e.CollectMessages());
					}
				}
			}
			finally
			{
				OnCompleted?.Invoke();
			}
		}

		protected abstract Task WriteContent(ActionContext context, CancellationToken token = default(CancellationToken));
	}
}