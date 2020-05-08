using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using asm.Extensions;
using asm.Helpers;
using asm.WebApi.ApiControllers;
using JetBrains.Annotations;
using ApiController = System.Web.Http.ApiController;

namespace asm.WebApi.Extensions
{
	public static class ApiControllerExtension
	{
		public static string NameOf([NotNull] this ApiController thisValue)
		{
			return thisValue.GetType().Name.Replace("Controller", string.Empty);
		}

		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, ApiControllerActionData logOffActionData, ApiControllerActionData defaultUrlData)
		{
			Regex logOffUrlExpression = logOffActionData == null ? null : UriHelper.CreateBadRedirectExpression(logOffActionData.CreateUrl(thisValue.Url));
			return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrlData);
		}

		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, Regex logOffUrlExpression, ApiControllerActionData defaultUrlData)
		{
			string defaultUrl = defaultUrlData?.CreateUrl(thisValue.Url);
			return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrl);
		}

		public static IHttpActionResult RedirectToLocal([NotNull] this ApiController thisValue, string returnUrl, Regex logOffUrlExpression = null, string defaultUrl = null)
		{
			if (!string.IsNullOrEmpty(returnUrl)) returnUrl = WebUtility.UrlDecode(returnUrl);

			bool isBadRedirect = !string.IsNullOrEmpty(returnUrl) && logOffUrlExpression != null && logOffUrlExpression.IsMatch(returnUrl);
			if (thisValue.Url.Request.IsLocalUrl() && !isBadRedirect && returnUrl != null) return new RedirectResult(new Uri(returnUrl), thisValue);
			return new RedirectResult(new Uri(string.IsNullOrEmpty(defaultUrl) ? "/" : defaultUrl), thisValue);
		}
	}
}