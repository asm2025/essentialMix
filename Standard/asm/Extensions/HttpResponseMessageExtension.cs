using System;
using System.Net.Http;
using JetBrains.Annotations;
using MSHeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using asmHeaderNames = asm.Web.HeaderNames;

namespace asm.Extensions
{
	public static class HttpResponseMessageExtension
	{
		public static void AllowOrigin([NotNull] this HttpResponseMessage thisValue, string value)
		{
			thisValue.Headers.Add(MSHeaderNames.AccessControlAllowOrigin, value);
		}

		public static void AddError([NotNull] this HttpResponseMessage thisValue, [NotNull] Exception exception)
		{
			thisValue.Headers.Add(asmHeaderNames.ApplicationError, exception.CollectMessages());
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, asmHeaderNames.ApplicationError);
		}

		public static void AddHeader([NotNull] this HttpResponseMessage thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.Add(name, value);
			thisValue.Headers.Add(MSHeaderNames.AccessControlExposeHeaders, name);
		}
	}
}