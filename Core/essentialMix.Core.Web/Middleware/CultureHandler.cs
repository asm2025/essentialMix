using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using essentialMix.Web;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace essentialMix.Core.Web.Middleware;

public class CultureHandler : MiddlewareBase<CultureHandlerOptions>
{
	private string _parameterName;

	/// <inheritdoc />
	public CultureHandler(RequestDelegate next, IOptions<CultureHandlerOptions> options, ILogger<CultureHandler> logger)
		: base(next, options, logger)
	{
	}

	[NotNull]
	public override Task Invoke([NotNull] HttpContext context)
	{
		if (context.RequestAborted.IsCancellationRequested) return Task.FromCanceled(context.RequestAborted);

		// here you can chose to get the lang from database, cookie or from the request if the culture is stored on local storage.
		CultureInfo ci = null;
		HttpRequest request = context.Request;

		if (request.Query.TryGetValue(ParameterName, out StringValues queryValues) && queryValues.Count > 0)
		{
			string name = queryValues.SkipNullOrWhitespace().FirstOrDefault();
			if (!string.IsNullOrEmpty(name)) ci = CultureInfoHelper.Get(name);
		}

		if (ci == null && request.Headers.TryGetValue(MSHeaderNames.AcceptLanguage, out StringValues headerValues) && headerValues.Count > 0)
		{
			string name = queryValues.SkipNullOrWhitespace()
									.FirstOrDefault(e => e.Length > 1)
									?.Substring(0, 2);
			if (!string.IsNullOrEmpty(name)) ci = CultureInfoHelper.Get(name);
		}

		if (ci != null)
		{
			request.Headers[MSHeaderNames.AcceptLanguage] = ci.Name;
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}

		return Next.Invoke(context);
	}

	[NotNull]
	protected string ParameterName
	{ 
		get { return _parameterName ??= Options?.Value?.ParameterName.ToNullIfEmpty() ?? RequestParameterNames.Culture; }
	}
}

public static class CultureHandlerExtension
{
	[NotNull]
	public static IServiceCollection AddCultureHandler([NotNull] this IServiceCollection thisValue, Action<CultureHandlerOptions> options = null)
	{
		options ??= _ => {};
		thisValue.Configure(options);
		return thisValue;
	}

	[NotNull]
	public static IApplicationBuilder UseCultureHandler([NotNull] this IApplicationBuilder thisValue)
	{
		return thisValue.UseMiddleware<CultureHandler>();
	}

	[NotNull]
	public static IApplicationBuilder UseCultureHandler([NotNull] this IApplicationBuilder thisValue, [NotNull] CultureHandlerOptions options)
	{
		return thisValue.UseMiddleware<CultureHandler>(options);
	}
}