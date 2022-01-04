using System.Web.Mvc;
using essentialMix.Web.Mvc.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IControllerExtension
{
	[NotNull]
	public static string ControllerName([NotNull] this IController thisValue) { return IControllerHelper.ControllerName(thisValue.GetType()); }
}