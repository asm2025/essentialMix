using System.Web.Http.Controllers;
using JetBrains.Annotations;
using asm.Web.Api.Helpers;

namespace asm.Web.Api.Extensions
{
	public static class IHttpControllerExtension
	{
		[NotNull] public static string ControllerName([NotNull] this IHttpController thisValue) { return IHttpControllerHelper.ControllerName(thisValue.GetType()); }
	}
}