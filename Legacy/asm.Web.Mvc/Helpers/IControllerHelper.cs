using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Helpers
{
	public static class IControllerHelper
	{
		[NotNull]
		public static string ControllerName<T>()
			where T : IController
		{
			return ControllerName(typeof(T));
		}

		[NotNull]
		public static string ControllerName([NotNull] Type type)
		{
			if (!typeof(IController).IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not assignable from IController.");
			return type.Name.Replace("Controller", string.Empty);
		}

		[NotNull]
		public static IEnumerable<Type> GetAllControllers()
		{
			Type type = typeof(IController);
			return AppDomain.CurrentDomain.GetAssemblies()
							.SelectMany(e => e.GetTypes().Where(t => !t.IsAbstract && type.IsAssignableFrom(t)));
		}
	}
}