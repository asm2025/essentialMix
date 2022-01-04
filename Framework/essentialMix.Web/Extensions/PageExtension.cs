using System.Web.UI;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class PageExtension
{
	public static bool InRole([NotNull] this Page thisValue, string roleName)
	{
		if (string.IsNullOrEmpty(roleName) || !thisValue.Request.IsAuthenticated) return false;
		return thisValue.User.IsInRole(roleName);
	}
}