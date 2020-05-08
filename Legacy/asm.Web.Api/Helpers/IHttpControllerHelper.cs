using System;
using System.Web.Http.Controllers;
using JetBrains.Annotations;

namespace asm.Web.Api.Helpers
{
	public static class IHttpControllerHelper
	{
		private static readonly Type __controllerType = typeof(IHttpController);

		[NotNull]
		public static string ControllerName<T>()
			where T : IHttpController
		{
			return ControllerName(typeof(T));
		}

		[NotNull]
		public static string ControllerName([NotNull] Type type)
		{
			if (!__controllerType.IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not a '{__controllerType}'.");
			return type.Name.Replace("Controller", string.Empty);
		}
	}
}