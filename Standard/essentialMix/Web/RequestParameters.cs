using System;
using System.Collections.Generic;
using System.Linq;
using essentialMix.Helpers;
using essentialMix.Patterns.Sorting;
using JetBrains.Annotations;

namespace essentialMix.Web
{
	[Serializable]
	public class RequestParameters<T>
		where T : struct, IComparable
	{
		private string _culture = RequestParameters.DefaultCulture;

		public RequestParameters() 
		{
		}

		public T Id { get; set; }
		public int? Page { get; set; } = 1;
		public int? PageSize { get; set; } = 10;

		public string Culture
		{
			get => _culture;
			set
			{
				if (!RequestParameters.IsSupportedCulture(value)) return;
				_culture = value;
			}
		}

		public string Filter { get; set; }

		public IList<SortField> Sort { get; set; }
	}

	public static class RequestParameters
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