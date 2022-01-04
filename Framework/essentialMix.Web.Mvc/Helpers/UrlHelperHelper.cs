using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using essentialMix.Web.Helpers;

namespace essentialMix.Web.Mvc.Helpers;

public static class UrlHelperHelper
{
	public static UrlHelper Create(string url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
	{
		return Create(RequestContextHelper.Create(url, onFillOwinData));
	}

	public static UrlHelper Create(Uri url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
	{
		return Create(RequestContextHelper.Create(url, onFillOwinData));
	}

	public static UrlHelper Create(HttpContext context) { return Create(RequestContextHelper.Create(context)); }

	public static UrlHelper Create(HttpContextBase context) { return Create(RequestContextHelper.Create(context)); }

	public static UrlHelper Create(RequestContext context) { return Create(context, RouteTable.Routes); }

	public static UrlHelper Create(RequestContext context, RouteCollection routeCollection)
	{
		return context == null
					? null
					: new UrlHelper(context, routeCollection);
	}
}