using System;
using System.Web;
using asm.Extensions;
using JetBrains.Annotations;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using asmHeaderNames = asm.Web.HeaderNames;

namespace asm.Web.Extensions
{
	public static class HttpResponseExtension
	{
		public static void AllowOrigin([NotNull] this HttpResponse thisValue, string value)
		{
			thisValue.Headers.Add(MSHeaderNames.AccessControlAllowOrigin, value);
		}

		public static void AddError([NotNull] this HttpResponse thisValue, [NotNull] Exception exception)
		{
			thisValue.Headers.Add(asmHeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, asmHeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpResponse thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}