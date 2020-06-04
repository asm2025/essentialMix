using System.Security.Principal;
using System.Web;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class HttpContextBaseExtension
	{
		[NotNull]
		public static HttpRequestBase GetRequest([NotNull] this HttpContextBase thisValue) { return thisValue.Request; }

		public static IPrincipal GetIdentity([NotNull] this HttpContextBase thisValue) { return thisValue.User; }
	}
}