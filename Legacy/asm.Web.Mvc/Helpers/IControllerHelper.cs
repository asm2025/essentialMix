using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Helpers
{
	public static class IControllerHelper
	{
		private static readonly Type __controllerType = typeof(IController);

		[NotNull]
		public static string ControllerName<T>()
			where T : IController
		{
			return ControllerName(typeof(T));
		}

		[NotNull]
		public static string ControllerName([NotNull] Type type)
		{
			if (!__controllerType.IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not a '{__controllerType}'.");
			return type.Name.Replace("Controller", string.Empty);
		}

		[NotNull]
		public static IEnumerable<Type> GetAllControllers()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
							.SelectMany(e => e.GetTypes().Where(t => !t.IsAbstract && __controllerType.IsAssignableFrom(t)));
		}
	}
}