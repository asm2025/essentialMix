using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using essentialMix.Extensions;
using essentialMix.Web.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Helpers
{
	public static class ControllerHelper
	{
		[NotNull]
		public static T CreateController<T>(RouteData routeData = null)
			where T : Controller, new()
		{
			return (T)CreateController(typeof(T), routeData);
		}

		[NotNull]
		public static Controller CreateController([NotNull] Type type, RouteData routeData = null)
		{
			if (!typeof(Controller).IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not a Controller.");
			
			Controller controller = (Controller)Activator.CreateInstance(type);
			routeData ??= new RouteData();

			if (!routeData.Values.Any(v => v.Key.IsSame("controller")))
				routeData.Values.Add("controller", controller.GetType().Name.ToLower().Replace("controller", string.Empty));

			if (HttpContext.Current == null)
			{
				HttpContext ctx = HttpContextHelper.Create((Uri)null);
				HttpContext.Current = ctx;
			}

			HttpContextWrapper wrapper = new HttpContextWrapper(HttpContext.Current);
			controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
			controller.Url = new UrlHelper(new RequestContext(wrapper, routeData));
			return controller;
		}
	}
}