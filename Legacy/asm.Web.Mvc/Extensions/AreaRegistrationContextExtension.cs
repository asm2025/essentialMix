using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class AreaRegistrationContextExtension
	{
		[NotNull]
		public static AreaRegistrationContext MapAreaDefaultRoutes([NotNull] this AreaRegistrationContext thisValue, object defaults = null, string prefix = null)
		{
			prefix = prefix?.Trim('/', ' ');
			if (!string.IsNullOrEmpty(prefix)) prefix += "/";
			prefix ??= string.Empty;
			thisValue.MapRoute(thisValue.AreaName + "_default",
							prefix + thisValue.AreaName + "/{controller}/{action}/{id}",
							defaults ?? new { area = thisValue.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
							thisValue.Namespaces.ToArray())
					.DataTokens["UseNamespaceFallback"] = false;
			return thisValue;
		}
	}
}