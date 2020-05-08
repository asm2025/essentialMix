using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace asm.Core.Web.Helpers
{
	public static class ControllerBaseHelper
	{
		[NotNull]
		public static string ControllerName<T>()
			where T : ControllerBase
		{
			return ControllerName(typeof(T));
		}

		[NotNull]
		public static string ControllerName([NotNull] Type type)
		{
			Type baseType = typeof(ControllerBase);
			if (!baseType.IsAssignableFrom(type)) throw new TypeAccessException($"Type '{type}' is not a '{baseType}'.");
			return type.Name.Replace("Controller", string.Empty);
		}
	}
}