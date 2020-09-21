using System;
using asm.Helpers;
using asm.Web.Handlers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class IRouteBuilderExtension
	{
		[NotNull]
		public static IRouteBuilder MapAreaDefaultRoutes([NotNull] this IRouteBuilder thisValue, [AspMvcArea] string areaName, object defaults = null, string prefix = null)
		{
			areaName = areaName?.Trim();
			if (string.IsNullOrEmpty(areaName)) throw new ArgumentNullException(nameof(areaName));
			prefix = prefix?.Trim('/', ' ') ?? string.Empty;
			string prefixWithArea = string.Join("/", prefix, areaName);
			thisValue.MapAreaRoute(
									null, 
									areaName, 
									prefixWithArea + "/{controller}/{action}/{id?}",
									new { area = areaName, action = "Index" },
									null,
									new { UseNamespaceFallback = false }
								);

			thisValue.MapAreaRoute(
									areaName + "_default",
									areaName,
									prefixWithArea + "/{controller}/{action}/{id?}",
									defaults ?? new { area = areaName, controller = "Home", action = "Index" },
									null,
									new { UseNamespaceFallback = false }
								);

			return thisValue;
		}

		[NotNull]
		public static IRouteBuilder MapDefaultRoutes([NotNull] this IRouteBuilder thisValue, object defaults = null, string prefix = null, string swaggerUrl = null)
		{
			prefix = UriHelper.Trim(prefix);
			if (!string.IsNullOrEmpty(prefix)) prefix += "/";
			prefix ??= string.Empty;
			thisValue.MapRoute("areas",
								prefix + "{area:exists}/{controller}/{action}/{id?}",
								new
								{
									action = "Index"
								},
								null,
								new
								{
									UseNamespaceFallback = false
								});
			
			if (!string.IsNullOrWhiteSpace(swaggerUrl))
			{
				thisValue.MapRoute("Swagger",
									string.Empty,
									null,
									null,
									new RedirectHandler(swaggerUrl));
			}

			thisValue.MapRoute("Default",
								prefix + "{controller}/{action}/{id?}",
								defaults ??
								new
								{
									controller = "Home",
									action = "Index"
								},
								null,
								new
								{
									UseNamespaceFallback = false
								});

			return thisValue;
		}
	}
}