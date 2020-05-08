using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using asm.Extensions;
using asm.Web.Helpers;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Helpers
{
	public static class ControllerHelper
	{
		private static readonly Type __controllerType = typeof(Controller);

		[NotNull]
		public static T CreateController<T>(RouteData routeData = null)
			where T : Controller, new()
		{
			return (T)CreateController(typeof(T), routeData);
		}

		[NotNull]
		public static Controller CreateController([NotNull] Type type, RouteData routeData = null)
		{
			if (!__controllerType.IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not a '{__controllerType}'.");
			
			Controller controller = (Controller)Activator.CreateInstance(type);
			if (routeData == null) routeData = new RouteData();

			if (!routeData.Values.Any(v => StringExtension.IsSame(v.Key, "controller")))
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