using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.SessionState;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Web.Helpers
{
	public static class HttpContextHelper
	{
		private const string URL_DEF = "http://tempuri.org";

		[NotNull]
		public static HttpContext Create(string url = null)
		{
			url = url?.Trim();
			if (string.IsNullOrEmpty(url)) url = URL_DEF;
			if (!UriHelper.TryBuildUri(url, out Uri uri)) throw new ArgumentException("Uri is not well formatted.", nameof(url));
			return Create(uri);
		}

		[NotNull]
		public static HttpContext Create(Uri url = null, Encoding encoding = null)
		{
			string uri;
			string query;

			if (url != null)
			{
				uri = url.ToString();
				query = url.Query.TrimStart('?');
			}
			else
			{
				uri = null;
				query = string.Empty;
			}

			if (string.IsNullOrEmpty(uri)) uri = URL_DEF;
			HttpRequest request = new HttpRequest(string.Empty, uri, query)
			{
				ContentEncoding = encoding ?? EncodingHelper.Default  //UrlDecode needs this to be set
			};

			HttpContext ctx = new HttpContext(request, new HttpResponse(new StringWriter()));

			//Session need to be set
			HttpSessionStateContainer sessionContainer = new HttpSessionStateContainer("id", 
				new SessionStateItemCollection(),
				new HttpStaticObjectsCollection(), 
				10, true, HttpCookieMode.AutoDetect,
				SessionStateMode.InProc, false);
			SessionStateUtility.AddHttpSessionStateToContext(ctx, sessionContainer);
			ctx.User = new GenericPrincipal(new GenericIdentity(string.Empty), Array.Empty<string>());
			return ctx;
		}

		public static HttpContext Create(string url, Func<IDictionary<string, object>, bool> onFillOwinData) { return FillContextData(Create(url), onFillOwinData); }

		public static HttpContext Create(Uri url, Func<IDictionary<string, object>, bool> onFillOwinData) { return FillContextData(Create(url), onFillOwinData); }

		private static HttpContext FillContextData(HttpContext ctx, Func<IDictionary<string, object>, bool> onFillOwinData = null)
		{
			if (ctx == null) return null;

			Dictionary<string, object> data = new Dictionary<string, object>
			{
				{"owin.RequestBody", null} // fake whatever  you need here.
			};

			if (onFillOwinData != null && !onFillOwinData(data)) return null;
			ctx.Items["owin.Environment"] = data;
			return ctx;
		}
	}
}