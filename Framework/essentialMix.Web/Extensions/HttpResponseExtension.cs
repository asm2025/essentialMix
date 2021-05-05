using System;
using System.Web;
using JetBrains.Annotations;
using essentialMix.Web;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class HttpResponseExtension
	{
		public static void AllowOrigin([NotNull] this HttpResponse thisValue, string value)
		{
			thisValue.Headers.Add(MSHeaderNames.AccessControlAllowOrigin, value);
		}

		public static void AddError([NotNull] this HttpResponse thisValue, [NotNull] Exception exception)
		{
			thisValue.Headers.Add(HeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, HeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpResponse thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}