using System;
using System.Collections.Generic;
using System.Linq;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Web
{
	public static class SupportedCultures
	{
		private static readonly ISet<string> __supportedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public static string DefaultCulture => __supportedCultures.Count == 0 ? null : __supportedCultures.First();

		public static bool AddCulture([NotNull] string name) { return CultureInfoHelper.IsCultureName(name) && __supportedCultures.Add(name); }

		public static bool RemoveCulture([NotNull] string name) { return __supportedCultures.Remove(name); }

		public static bool IsSupportedCulture(string name) { return CultureInfoHelper.IsSupported(name, __supportedCultures); }

		public static void ResetCultures()
		{
			__supportedCultures.Clear();
		}
	}
}