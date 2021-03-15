using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using essentialMixHeaderNames = essentialMix.Web.HeaderNames;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class CoreWebHttpResponseExtension
	{
		public static void AllowOrigin([NotNull] this HttpResponse thisValue, StringValues value)
		{
			thisValue.Headers.Add(MSHeaderNames.AccessControlAllowOrigin, value);
		}

		public static void AddError([NotNull] this HttpResponse thisValue, [NotNull] Exception exception)
		{
			thisValue.Headers.Add(essentialMixHeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, essentialMixHeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpResponse thisValue, [NotNull] string name, StringValues value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}