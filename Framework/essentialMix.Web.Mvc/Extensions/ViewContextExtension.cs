using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ViewContextExtension
	{
		public static HttpContextBase GetHttpContext([NotNull] this ViewContext thisValue) { return thisValue.HttpContext; }

		public static HttpRequestBase GetRequest([NotNull] this ViewContext thisValue) { return GetHttpContext(thisValue)?.Request; }

		public static IPrincipal GetIdentity([NotNull] this ViewContext thisValue) { return GetHttpContext(thisValue)?.User; }
	}
}