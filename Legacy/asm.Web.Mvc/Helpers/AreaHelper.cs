using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Helpers
{
	public static class AreaHelper
	{
		public static IEnumerable<AreaRegistration> GetAllAreas()
		{
			ISet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes().Where(e => typeof(AreaRegistration).IsAssignableFrom(e)))
				{
					if (string.IsNullOrEmpty(type.FullName) || !names.Add(type.FullName)) continue;
					yield return (AreaRegistration)Activator.CreateInstance(type);
				}
			}
		}

		[NotNull]
		public static IEnumerable<string> GetAllRegisteredAreas()
		{
			return RouteTable.Routes.OfType<Route>()
							.Where(e => e.DataTokens != null && e.DataTokens.ContainsKey("area"))
							.Select(e => (string)e.DataTokens["area"]);
		}
	}
}