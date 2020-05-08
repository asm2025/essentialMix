using System.Web.UI;
using JetBrains.Annotations;

namespace asm.Web.Extensions
{
	public static class PageExtension
	{
		public static bool InRole([NotNull] this Page thisValue, string roleName)
		{
			if (string.IsNullOrEmpty(roleName) || !thisValue.Request.IsAuthenticated) return false;
			return thisValue.User.IsInRole(roleName);
		}
	}
}
