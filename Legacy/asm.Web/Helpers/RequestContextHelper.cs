using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace asm.Web.Helpers
{
	public static class RequestContextHelper
	{
		public static RequestContext Create(string url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
		{
			return Create(HttpContextBaseHelper.Create(url, onFillOwinData));
		}

		public static RequestContext Create(Uri url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
		{
			return Create(HttpContextBaseHelper.Create(url, onFillOwinData));
		}

		public static RequestContext Create(HttpContext context)
		{
			return context == null
						? null
						: Create(new HttpContextWrapper(context));
		}

		public static RequestContext Create(HttpContextBase context)
		{
			if (context == null) return null;

			RouteData routeData = RouteTable.Routes.GetRouteData(context) ?? new RouteData();
			return new RequestContext(context, routeData);
		}
	}
}