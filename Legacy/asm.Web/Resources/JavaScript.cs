using System;
using System.Collections.Generic;
using System.Web.UI;
using JetBrains.Annotations;

[assembly: WebResource("asm.Web.Resources.cookies.js", "application/x-javascript")]
namespace asm.Web.Resources
{
	public static class JavaScript
	{
		private static readonly IDictionary<string, string> __names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
																	{
																		{ "cookies", "cookies.js" }
																	};

		public static void Include([NotNull] ClientScriptManager manager, [NotNull] string name)
		{
			if (!__names.TryGetValue(name, out string fileName)) throw new KeyNotFoundException();

			Type type = typeof(JavaScript);
			manager.RegisterClientScriptResource(type, $"{type.Namespace}.{fileName}");
		}
	}
}