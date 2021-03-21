using System;
using System.Web.Http.Controllers;
using JetBrains.Annotations;

namespace essentialMix.Web.Api.Helpers
{
	public static class IHttpControllerHelper
	{
		[NotNull]
		public static string ControllerName<T>()
			where T : IHttpController
		{
			return ControllerName(typeof(T));
		}

		[NotNull]
		public static string ControllerName([NotNull] Type type)
		{
			if (!typeof(IHttpController).IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not assignable from IHttpController.");
			return type.Name.Replace("Controller", string.Empty);
		}
	}
}