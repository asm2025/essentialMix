using System;
using System.Collections.Generic;
using System.Web;
using JetBrains.Annotations;

namespace asm.Web.Helpers
{
	public static class HttpContextBaseHelper
	{
		public static HttpContextBase Create(string url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
		{
			return Create(HttpContextHelper.Create(url, onFillOwinData));
		}

		public static HttpContextBase Create(Uri url = null, Func<IDictionary<string, object>, bool> onFillOwinData = null)
		{
			return Create(HttpContextHelper.Create(url, onFillOwinData));
		}

		public static HttpContextBase Create(HttpContext ctx)
		{
			return ctx == null
						? null
						: new HttpContextWrapper(ctx);
		}
	}
}