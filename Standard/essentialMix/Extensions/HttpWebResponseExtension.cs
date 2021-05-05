using System;
using System.Net;
using JetBrains.Annotations;
using essentialMix.Web;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

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
			thisValue.Headers.Add(HeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, HeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpWebResponse thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}