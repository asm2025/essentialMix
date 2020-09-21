using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class CoreWebHttpContextExtension
	{
		public static Task WriteModelAsync<T>([NotNull] this HttpContext context, T model)
		{
			ObjectResult result = new ObjectResult(model)
			{
				DeclaredType = typeof(T)
			};

			return context.ExecuteResultAsync(result);
		}

		public static Task ExecuteResultAsync<TResult>([NotNull] this HttpContext context, [NotNull] TResult result)
			where TResult : IActionResult
		{
			IActionResultExecutor<TResult> executor = context.RequestServices.GetRequiredService<IActionResultExecutor<TResult>>();
			RouteData routeData = context.GetRouteData() ?? new RouteData();
			ActionContext actionContext = new ActionContext(context, routeData, new ActionDescriptor());
			return executor.ExecuteAsync(actionContext, result);
		}

		public static (IOutputFormatter SelectedFormatter, OutputFormatterWriteContext FormatterContext) SelectFormatter<T>([NotNull] this HttpContext context, [NotNull] T model)
		{
			OutputFormatterSelector selector = context.RequestServices.GetRequiredService<OutputFormatterSelector>();
			IHttpResponseStreamWriterFactory writerFactory = context.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>();
			OutputFormatterWriteContext formatterContext = new OutputFormatterWriteContext(context, writerFactory.CreateWriter, typeof(T), model);
			IOutputFormatter selectedFormatter = selector.SelectFormatter(formatterContext, Array.Empty<IOutputFormatter>(), new MediaTypeCollection());
			return (selectedFormatter, formatterContext);
		}
	}
}