using System;
using System.Net;
using JetBrains.Annotations;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using essentialMixHeaderNames = essentialMix.Web.HeaderNames;

namespace essentialMix.Extensions
{
	public static class HttpWebResponseExtension
	{
		public static void AllowOrigin([NotNull] this HttpWebResponse thisValue, string value)
		{
			thisValue.Headers.Add(MSHeaderNames.AccessControlAllowOrigin, value);
		}

		public static void AddError([NotNull] this HttpWebResponse thisValue, [NotNull] Exception exception)
		{
			thisValue.Headers.Add(essentialMixHeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, essentialMixHeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpWebResponse thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}