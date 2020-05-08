using System;
using asm.Helpers;
using asm.Web.Handlers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace asm.Core.Web.Extensions
{
	public static class IEndpointRouteBuilderExtension
	{
		[NotNull]
		public static IEndpointRouteBuilder MapAreaDefaultRoutes([NotNull] this IEndpointRouteBuilder thisValue, [AspMvcArea] string areaName, object defaults = null, string prefix = null)
		{
			areaName = areaName?.Trim();
			if (string.IsNullOrEmpty(areaName)) throw new ArgumentNullException(nameof(areaName));
			prefix = prefix?.Trim('/', ' ') ?? string.Empty;
			string prefixWithArea = string.Join("/", prefix, areaName);
			thisValue.MapAreaControllerRoute(null,
											areaName,
											prefixWithArea + "/{controller}/{action}/{id?}",
											new
											{
												area = areaName,
												action = "Index"
											},
											null,
											new
											{
												UseNamespaceFallback = false
											});

			thisValue.MapAreaControllerRoute(areaName + "_default",
											areaName,
											prefixWithArea + "/{controller}/{action}/{id?}",
											defaults ??
											new
											{
												area = areaName,
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

		[NotNull]
		public static IEndpointRouteBuilder MapDefaultRoutes([NotNull] this IEndpointRouteBuilder thisValue, object defaults = null, string prefix = null, string swaggerUrl = null)
		{
			prefix = UriHelper.Trim(prefix);
			if (!string.IsNullOrEmpty(prefix)) prefix += "/";
			if (prefix == null) prefix = string.Empty;
			thisValue.MapControllerRoute("areas",
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
				thisValue.MapControllerRoute("Swagger",
									string.Empty,
									null,
									null,
									new RedirectHandler(swaggerUrl));
			}

			thisValue.MapControllerRoute("Default",
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