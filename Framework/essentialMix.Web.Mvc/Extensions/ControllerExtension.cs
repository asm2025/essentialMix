using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Web.Mvc.Models;
using Controller = System.Web.Mvc.Controller;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class ControllerExtension
{
	[NotNull]
	public static ActionResult RedirectToLocal([NotNull] this Controller thisValue, string returnUrl, ControllerActionData logOffActionData, ControllerActionData defaultUrlData)
	{
		Regex logOffUrlExpression = logOffActionData == null ? null : UriHelper.CreateBadRedirectExpression(logOffActionData.CreateUrl(thisValue.Url));
		return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrlData);
	}

	[NotNull]
	public static ActionResult RedirectToLocal([NotNull] this Controller thisValue, string returnUrl, Regex logOffUrlExpression, ControllerActionData defaultUrlData)
	{
		string defaultUrl = defaultUrlData?.CreateUrl(thisValue.Url);
		return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrl);
	}

	[NotNull]
	public static ActionResult RedirectToLocal([NotNull] this Controller thisValue, string returnUrl, Regex logOffUrlExpression = null, string defaultUrl = null)
	{
		if (!string.IsNullOrEmpty(returnUrl)) returnUrl = WebUtility.UrlDecode(returnUrl);

		bool isBadRedirect = !string.IsNullOrEmpty(returnUrl) && logOffUrlExpression != null && logOffUrlExpression.IsMatch(returnUrl);
		if (thisValue.Url.IsLocalUrl(returnUrl) && !isBadRedirect) return new RedirectResult(returnUrl);
		return new RedirectResult(string.IsNullOrEmpty(defaultUrl) ? "/" : defaultUrl);
	}
}