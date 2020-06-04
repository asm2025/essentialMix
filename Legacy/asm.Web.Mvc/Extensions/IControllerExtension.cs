using System.Web.Mvc;
using asm.Web.Mvc.Helpers;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class IControllerExtension
	{
		[NotNull]
		public static string ControllerName([NotNull] this IController thisValue) { return IControllerHelper.ControllerName(thisValue.GetType()); }
	}
}