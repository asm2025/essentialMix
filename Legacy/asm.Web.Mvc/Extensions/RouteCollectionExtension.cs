using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class RouteCollectionExtension
	{
		[NotNull]
		public static RouteCollection MapDefaultRoutes([NotNull] this RouteCollection thisValue, object defaults = null, string prefix = null)
		{
			prefix = prefix?.Trim('/', ' ');
			if (!string.IsNullOrEmpty(prefix)) prefix += "/";
			if (prefix == null) prefix = string.Empty;

			thisValue.MapMvcAttributeRoutes();

			thisValue.MapRoute(null,
							prefix + "{controller}",
							new { action = "Index", id = UrlParameter.Optional });

			thisValue.MapRoute("Default",
							prefix + "{controller}/{action}/{id}",
							defaults ?? new { controller = "Home", action = "Index", id = UrlParameter.Optional });
			return thisValue;
		}
	}
}