using System.Web.Http.Controllers;
using JetBrains.Annotations;
using essentialMix.Web.Api.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class IHttpControllerExtension
	{
		[NotNull]
		public static string ControllerName([NotNull] this IHttpController thisValue) { return IHttpControllerHelper.ControllerName(thisValue.GetType()); }
	}
}