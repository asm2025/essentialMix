using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class CultureInfoHelper
{
	public const int INVARIANT = 127;

	private static readonly ConcurrentDictionary<int, ReadOnlyDictionary<string, CultureInfo>> __culturesByRegion = new ConcurrentDictionary<int, ReadOnlyDictionary<string, CultureInfo>>();

	private static readonly Lazy<ISet<string>> __dateTimeFormats = new Lazy<ISet<string>>(() =>
	{
		HashSet<string> set = new HashSet<string>(StringComparer.Ordinal)
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
		};

		foreach (string pattern in CultureInfo.InvariantCulture.DateTimeFormat.GetAllDateTimePatterns())
			set.Add(pattern);

		foreach (string pattern in Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns())
			set.Add(pattern);

		return set;
	}, LazyThreadSafetyMode.PublicationOnly);

	private static readonly Lazy<IReadOnlyDictionary<string, CultureInfo>> __cultures = new Lazy<IReadOnlyDictionary<string, CultureInfo>>(() => new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures)
		.ToDictionary(k => k.Name, StringComparer.OrdinalIgnoreCase)), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<IReadOnlyDictionary<string, CultureInfo>> __specificCultures = new Lazy<IReadOnlyDictionary<string, CultureInfo>>(() => new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures)
		.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase)), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<IReadOnlyDictionary<string, CultureInfo>> __neutralCultures = new Lazy<IReadOnlyDictionary<string, CultureInfo>>(() => new ReadOnlyDictionary<string, CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures)
		.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase)), LazyThreadSafetyMode.PublicationOnly);
	private static readonly ConcurrentDictionary<string, CultureInfo> __cultureCache = new ConcurrentDictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase);

	private static CultureInfo __english;

	[NotNull]
	public static CultureInfo Default => Thread.CurrentThread.CurrentCulture;

	[NotNull]
	public static CultureInfo English => __english ??= Get("en");

	[NotNull]
	public static IReadOnlyDictionary<string, CultureInfo> Cultures => __cultures.Value;

	[NotNull]
	public static IReadOnlyDictionary<string, CultureInfo> SpecificCultures => __specificCultures.Value;

	[NotNull]
	public static IReadOnlyDictionary<string, CultureInfo> NeutralCultures => __neutralCultures.Value;

	[NotNull]
	public static ISet<string> GetDateTimeFormats(string addFormat)
	{
		if (!string.IsNullOrEmpty(addFormat)) __dateTimeFormats.Value.Add(addFormat);
		return __dateTimeFormats.Value;
	}

	public static char[] GetDefaultListSeparators()
	{
		string ls = Default.TextInfo.ListSeparator;

		if (string.IsNullOrEmpty(ls))
		{
			return
			[
				',',
				';'
			];
		}

		char[] separators = new char[ls.Length + 2];
		separators[0] = ',';
		separators[1] = ';';
		ls.CopyTo(0, separators, 2, ls.Length);
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
		return __culturesByRegion.GetOrAdd(region.GeoId, geoId => new ReadOnlyDictionary<string, CultureInfo>(SpecificCultures.Values
																												.Where(e => e.LCID != INVARIANT && !e.IsNeutralCulture && e.Region().GeoId == geoId)
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