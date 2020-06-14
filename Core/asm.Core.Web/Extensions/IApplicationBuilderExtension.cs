using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using asm.Extensions;
using asm.Model.Web;

namespace asm.Core.Web.Extensions
{
	public static class IApplicationBuilderExtension
	{
		public static IApplicationBuilder UseRedirectWithStatusCode([NotNull] this IApplicationBuilder thisValue, string errorHandlerPathTemplate = null)
		{
			errorHandlerPathTemplate = errorHandlerPathTemplate?.Trim(Path.AltDirectorySeparatorChar, ' ').ToNullIfEmpty() ?? "/error/{0}";
			return thisValue.UseStatusCodePagesWithReExecute(errorHandlerPathTemplate);
		}

		[NotNull]
		public static IApplicationBuilder UseDefaultExceptionDelegate([NotNull] this IApplicationBuilder thisValue, ILogger logger = null)
		{
			return UseDefaultExceptionDelegate(thisValue, null, logger);
		}

		[NotNull]
		public static IApplicationBuilder UseDefaultExceptionDelegate([NotNull] this IApplicationBuilder thisValue, Func<HttpContext, Exception, ILogger, Task> onError, ILogger logger = null)
		{
			onError ??= async (context, exception, log) =>
			{
				log?.LogError(exception, exception.Message);
				context.Response.ContentType = "text/html";
				await context.Response.WriteAsync(new ResponseStatus
													{
														StatusCode = (HttpStatusCode)context.Response.StatusCode,
														Exception = exception
													}.ToString()
													.Replace(Environment.NewLine, $"{Environment.NewLine}<br />"));
			};

			thisValue.UseExceptionHandler(app =>
			{
				app.Run(async context =>
				{
					IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();
					if (contextFeature == null) return;
					await onError(context, contextFeature.Error, logger);
				});
			});

			return thisValue;
		}
	}
}