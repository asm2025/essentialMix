using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using essentialMix.Exceptions.Web;
using essentialMix.Extensions;

namespace essentialMix.Core.Web.Middleware;

public class ExceptionHandler : MiddlewareBase<ExceptionHandlerOptions>
{
	/// <inheritdoc />
	public ExceptionHandler()
		: this(null, null, null)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(RequestDelegate next)
		: this(next, null, null)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(IOptions<ExceptionHandlerOptions> options)
		: this(null, options, null)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(ILogger logger)
		: this(null, null, logger)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(RequestDelegate next, IOptions<ExceptionHandlerOptions> options)
		: this(next, options, null)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(RequestDelegate next, ILogger logger)
		: this(next, null, logger)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(IOptions<ExceptionHandlerOptions> options, ILogger logger)
		: this(null, options, logger)
	{
	}

	/// <inheritdoc />
	public ExceptionHandler(RequestDelegate next, IOptions<ExceptionHandlerOptions> options, ILogger logger)
		: base(next, options, logger)
	{
		Options.Value.ExceptionHandler ??= Next;
	}

	/// <inheritdoc />
	public override async Task Invoke(HttpContext context)
	{
		if (Next == null) return;

		try
		{
			await Next.Invoke(context);
		}
		catch (Exception e)
		{
			string errorMessage = e.CollectMessages();
			Logger.LogError(errorMessage);
			if (context.Response.HasStarted) throw;

			PathString originalPath = context.Request.Path;
			if (Options.Value.ExceptionHandlingPath.HasValue) context.Request.Path = Options.Value.ExceptionHandlingPath;

			try
			{
				ExceptionHandlerFeature errorHandlerFeature = new ExceptionHandlerFeature
				{
					Error = e
				};

				context.Features.Set<IExceptionHandlerFeature>(errorHandlerFeature);
				context.Response.Headers.Clear();

				switch (e)
				{
					case HttpException httpException:
						context.Response.StatusCode = httpException.StatusCode;
						break;
					case HttpListenerException httpListenerException:
						context.Response.StatusCode = httpListenerException.ErrorCode;
						break;
					default:
						context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
						break;
				}

				if (Options.Value.ExceptionHandler != null)
					await Options.Value.ExceptionHandler.Invoke(context);
				else
				{
					IHttpResponseFeature responseFeature = context.Features.Get<IHttpResponseFeature>();
					if (responseFeature != null) responseFeature.ReasonPhrase = errorMessage;
				}

				return;
			}
			catch (Exception ex)
			{
				errorMessage = ex.CollectMessages();
				Logger.LogError(errorMessage);
			}
			finally
			{
				context.Request.Path = originalPath;
			}

			// Re-throw the original if we couldn't handle it
			throw;
		}
	}
}