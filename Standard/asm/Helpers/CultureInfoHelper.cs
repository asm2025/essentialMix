using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class CultureInfoHelper
	{
		public const int INVARIANT = 127;

		private static readonly ConcurrentDictionary<int, ReadOnlyDictionary<string, CultureInfo>> __culturesByRegion = new ConcurrentDictionary<int, ReadOnlyDictionary<string, CultureInfo>>();
		private static readonly char[] __defaultSeparators = { ',', ';' };

		private static readonly HashSet<string> __dateTimeFormats = new HashSet<string>(new[]
		{
			"d",
			"D",
			"f",
			"F",
			"g",
			"G",
			"m",
			"M",
			"o",
			"O",
			"r",
			"R",
			"s",
			"t",
			"T",
			"u",
			"U",
			"y",
			"Y"
		}.Concat(CultureInfo.InvariantCulture.DateTimeFormat.GetAllDateTimePatterns())
		.Concat(Default.DateTimeFormat.GetAllDateTimePatterns()));

		private static CultureInfo __english;
		private static ReadOnlyDictionary<string, CultureInfo> __cultures;
		private static ReadOnlyDictionary<string, CultureInfo> __specificCultures;
		private static ReadOnlyDictionary<string, CultureInfo> __neutralCultures;
		private static readonly ConcurrentDictionary<string, CultureInfo> __cultureCache = new ConcurrentDictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase);

		[NotNull]
		public static CultureInfo Default => Thread.CurrentThread.CurrentCulture;

		[NotNull]
		public static CultureInfo English => __english ??= Get("en");

		[NotNull]
		public static ReadOnlyDictionary<string, CultureInfo> Cultures
		{
			get
			{
				return __cultures ??= new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures)
																							.ToDictionary(k => k.Name, StringComparer.OrdinalIgnoreCase));
			}
		}

		[NotNull]
		public static ReadOnlyDictionary<string, CultureInfo> SpecificCultures
		{
			get
			{
				return __specificCultures ??= new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures)
																									.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase));
			}
		}

		[NotNull]
		public static ReadOnlyDictionary<string, CultureInfo> NeutralCultures
		{
			get
			{
				return __neutralCultures ??= new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures)
																									.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase));
			}
		}

		[NotNull]
		public static IReadOnlyCollection<string> GetDateTimeFormats(string addFormat)
		{
			if (!string.IsNullOrEmpty(addFormat)) __dateTimeFormats.Add(addFormat);
			return __dateTimeFormats.AsReadOnly();
		}

		public static char[] GetDefaultListSeparators()
		{
			string ls = Default.TextInfo.ListSeparator;
			if (string.IsNullOrEmpty(ls))
				return __defaultSeparators;

			char[] separators = new char[__defaultSeparators.Length + ls.Length];
			__defaultSeparators.CopyTo(separators, 0);
			ls.CopyTo(0, separators, __defaultSeparators.Length, ls.Length);
			return separators;
		}

		[NotNull]
		public static string GetNeutralName(string name, bool allowOverrides = true)
		{
			CultureInfo culture = Get(name, allowOverrides);
			return culture.Neutral().Name;
		}

		public static string GetNameFromFileName(string path)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			if (string.IsNullOrEmpty(fileNameWithoutExtension)) return null;

			int i = fileNameWithoutExtension.LastIndexOf('.');
			if (i < 0) return null;

			string text = fileNameWithoutExtension.Substring(i + 1);
			return IsCultureName(text) ? text : null;
		}

		[NotNull]
		public static string GetNativeName(string name, bool neutral = false)
		{
			CultureInfo culture = Get(name);
			return neutral ? culture.Neutral().NativeName : culture.NativeName;
		}

		[NotNull]
		public static string GetEnglishName(string name, bool neutral = false)
		{
			CultureInfo culture = Get(name);
			return neutral ? culture.Neutral().EnglishName : culture.EnglishName;
		}

		[NotNull]
		public static string GetDisplayName(string name, bool neutral = false)
		{
			CultureInfo culture = Get(name);
			return neutral ? culture.Neutral().DisplayName : culture.DisplayName;
		}

		[NotNull]
		public static CultureInfo Get(string name = null, bool allowOverrides = true)
		{
			name ??= Default.Name;
			return !allowOverrides
						? new CultureInfo(name, false)
						: __cultureCache.GetOrAdd(name, CultureInfo.GetCultureInfo);
		}

		public static CultureInfo Get(string name, [NotNull] params string[] restrictTo)
		{
			name ??= Default.Name;
			if (!IsSupported(name, restrictTo)) return null;

			Cultures.TryGetValue(name, out CultureInfo cultureInfo);
			return cultureInfo;
		}

		public static ReadOnlyDictionary<string, CultureInfo> ListByGeoId(int geoId)
		{
			RegionInfo region = RegionInfoHelper.GetByGeoId(geoId);
			return ListByRegion(region);
		}

		public static ReadOnlyDictionary<string, CultureInfo> ListByRegion([NotNull] RegionInfo region)
		{
			return __culturesByRegion.GetOrAdd(region.GeoId, geoId => new ReadOnlyDictionary<string, CultureInfo>((SpecificCultures.Values ?? throw new InvalidOperationException("Cannot access cultures."))
																																								.Where(e => e.LCID != INVARIANT && !e.IsNeutralCulture && e.Region().GeoId == region.GeoId)
																																								.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase)));
		}

		public static bool IsCultureName(string name) { return !string.IsNullOrEmpty(name) && Cultures.ContainsKey(name); }

		public static bool IsSupported(string name, [NotNull] params string[] restrictTo)
		{
			return IsCultureName(name) && (restrictTo.Length == 0 || restrictTo.Contains(name, StringComparer.OrdinalIgnoreCase));
		}

		public static bool IsSupported(string name, [NotNull] ICollection<string> restrictTo)
		{
			return IsCultureName(name) && (restrictTo.Count == 0 || restrictTo.Contains(name));
		}
	}
}