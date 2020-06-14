using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using asm.Helpers;
using asm.Web.Api.Model;
using asm.Web.Extensions;
using JetBrains.Annotations;

namespace asm.Web.Api.Extensions
{
	public static class ApiControllerExtension
	{
		[NotNull]
		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, ActionData logOffActionData, ActionData defaultUrlData)
		{
			Regex logOffUrlExpression = logOffActionData == null ? null : UriHelper.CreateBadRedirectExpression(logOffActionData.CreateUrl(thisValue.Url));
			return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrlData);
		}

		[NotNull]
		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, Regex logOffUrlExpression, ActionData defaultUrlData)
		{
			string defaultUrl = defaultUrlData?.CreateUrl(thisValue.Url);
			return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrl);
		}

		[NotNull]
		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, Regex logOffUrlExpression = null, string defaultUrl = null)
		{
			if (!string.IsNullOrEmpty(returnUrl)) returnUrl = WebUtility.UrlDecode(returnUrl);

			bool isBadRedirect = !string.IsNullOrEmpty(returnUrl) && logOffUrlExpression != null && logOffUrlExpression.IsMatch(returnUrl);
			if (thisValue.Url.Request.IsLocalUrl() && !isBadRedirect && returnUrl != null) return new RedirectResult(new Uri(returnUrl), thisValue);
			return new RedirectResult(new Uri(string.IsNullOrEmpty(defaultUrl) ? "/" : defaultUrl), thisValue);
		}
	}
}