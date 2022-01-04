using System.Net;
using System.Text.RegularExpressions;
using essentialMix.Core.Web.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using essentialMix.Core.Web.Model;
using essentialMix.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class ControllerBaseExtension
{
	[NotNull]
	public static string ControllerName([NotNull] this ControllerBase thisValue) { return ControllerBaseHelper.ControllerName(thisValue.GetType()); }

	[NotNull]
	public static IActionResult RedirectToLocal([NotNull] this ControllerBase thisValue, string returnUrl, ControllerActionData logOffActionData, ControllerActionData defaultUrlData)
	{
		Regex logOffUrlExpression = logOffActionData == null ? null : UriHelper.CreateBadRedirectExpression(logOffActionData.CreateUrl(thisValue.Url));
		return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrlData);
	}

	[NotNull]
	public static IActionResult RedirectToLocal([NotNull] this ControllerBase thisValue, string returnUrl, Regex logOffUrlExpression, ControllerActionData defaultUrlData)
	{
		string defaultUrl = defaultUrlData?.CreateUrl(thisValue.Url);
		return RedirectToLocal(thisValue, returnUrl, logOffUrlExpression, defaultUrl);
	}

	[NotNull]
	public static IActionResult RedirectToLocal([NotNull] this ControllerBase thisValue, string returnUrl, Regex logOffUrlExpression = null, string defaultUrl = null)
	{
		if (!string.IsNullOrEmpty(returnUrl)) returnUrl = WebUtility.UrlDecode(returnUrl);

		bool isBadRedirect = !string.IsNullOrEmpty(returnUrl) && logOffUrlExpression != null && logOffUrlExpression.IsMatch(returnUrl);
		if (!isBadRedirect && returnUrl != null) return new RedirectResult(returnUrl);
		return new RedirectResult(string.IsNullOrEmpty(defaultUrl) ? "/" : defaultUrl);
	}
}