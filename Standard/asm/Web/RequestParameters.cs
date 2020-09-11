using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using asm.Helpers;
using asm.Patterns.Sorting;
using asm.Patterns.String;
using JetBrains.Annotations;

namespace asm.Web
{
	[Serializable]
	public class RequestParameters<T>
		where T : struct, IComparable
	{
		private string _culture = RequestParameters.DefaultCulture;

		/// <inheritdoc />
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

		[NotNull]
		public ISet<string> Filter { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public string FilterTargets =>
			Filter.Count == 0
				? null
				: string.Join(",", Filter);

		public string FilterTarget { get; set; }

		public FilterType FilterType { get; set; }

		[NotNull]
		public ISet<string> SortTargets { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public SortType SortType { get; set; }

		[NotNull]
		public string BuildFilter()
		{
			// todo: make Command Param && pass through ()=> function. 
			StringBuilder filter = new StringBuilder();

			if (!string.IsNullOrEmpty(FilterTarget)
				&& string.IsNullOrEmpty(FilterTarget))
			{
				filter.Append($"AND (${FilterTarget} IS NOT NULL AND ");

				switch (FilterType)
				{
					case FilterType.Equals:
						filter.Append($"${FilterTarget} = '{FilterTarget}'");
						break;
					case FilterType.NotEquals:
						filter.Append($"${FilterTarget} != '{FilterTarget}'");
						break;
					case FilterType.DoesNotContain:
						filter.Append($"NOT (${FilterTarget} LIKE '%{FilterTarget}%')");
						break;
					case FilterType.StartsWith:
						filter.Append($"${FilterTarget} LIKE '{FilterTarget}%'");
						break;
					case FilterType.DoesNotStartWith:
						filter.Append($"NOT (${FilterTarget} LIKE '{FilterTarget}%')");
						break;
					case FilterType.EndsWith:
						filter.Append($"${FilterTarget} LIKE '%{FilterTarget}'");
						break;
					case FilterType.DoesNotEndWith:
						filter.Append($"NOT (${FilterTarget} LIKE '%{FilterTarget}')");
						break;
					case FilterType.In:
						filter.Append($"${FilterTarget} IN ('{FilterTarget}')");
						break;
					case FilterType.NotIn:
						filter.Append($"NOT (${FilterTarget} IN ('{FilterTarget}'))");
						break;
					default: // Case Default, Contains
						filter.Append($"${FilterTarget} LIKE '%{FilterTarget}%'");
						break;
				}

				filter.Append(")");
			}

			return filter.ToString();
		}
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