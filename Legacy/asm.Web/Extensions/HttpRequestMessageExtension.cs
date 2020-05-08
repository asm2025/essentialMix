using System;
using System.Net.Http;
using System.Web;
using JetBrains.Annotations;
using Microsoft.Owin;

namespace asm.Web.Extensions
{
	public static class HttpRequestMessageExtension
	{
		public static bool IsLocalUrl([NotNull] this HttpRequestMessage thisValue)
		{
			object isLocal = thisValue.Properties["MS_IsLocal"];
			return isLocal as bool? == true || isLocal is Lazy<bool> localFlag && localFlag.Value;
		}

		public static IOwinContext GetOwinContext([NotNull] this HttpRequestMessage thisValue)
		{
			HttpContextWrapper context = thisValue.Properties["MS_HttpContext"] as HttpContextWrapper;
			return context?.Request.GetOwinContext();
		}
	}
}