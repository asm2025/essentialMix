using JetBrains.Annotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace asm.Core.Web.Mvc.Filters
{
	// https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.0
	public class AntiForgeryTokenAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted([NotNull] ActionExecutedContext context)
		{
			IAntiforgery antiForgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();
			// We can send the request token as a JavaScript-readable cookie, 
			// and Angular will use it by default.
			AntiforgeryTokenSet tokens = antiForgery?.GetAndStoreTokens(context.HttpContext);
			if (tokens?.RequestToken == null) return;
			context.HttpContext.Response.Cookies.Append(asm.Web.CookieNames.AntiForgeryToken, tokens.RequestToken, new CookieOptions
			{
				HttpOnly = false
			});
		}
	}
}
