using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class RegionInfoHelper
	{
		private static ReadOnlyDictionary<int, RegionInfo> __regions;

		[NotNull]
		public static RegionInfo Default => RegionInfo.CurrentRegion;

		[NotNull]
		public static ReadOnlyDictionary<int, RegionInfo> Regions
		{
			get
			{
				return __regions ??= new ReadOnlyDictionary<int, RegionInfo>(CultureInfoHelper
																			.SpecificCultures
																			.Values
																			.Where(e => e.LCID != CultureInfoHelper.INVARIANT && !e.IsNeutralCulture)
																			.Select(e => e.Region())
																			.Distinct(e => e.GeoId)
																			.ToDictionary(k => k.GeoId));
			}
		}

		public static string GetNativeName(int geoId)
		{
			return GetByGeoId(geoId)?.NativeName;
		}

		public static string GetNativeName(string name)
		{
			return Get(name)?.NativeName;
		}

		public static string GetEnglishName(int geoId)
		{
			return GetByGeoId(geoId)?.EnglishName;
		}

		public static string GetEnglishName(string name)
		{
			return Get(name)?.EnglishName;
		}

		public static string GetDisplayName(int geoId)
		{
			return GetByGeoId(geoId)?.DisplayName;
		}

		public static string GetDisplayName(string name)
		{
			return Get(name)?.DisplayName;
		}

		public static RegionInfo GetByGeoId(int geoId)
		{
			return Regions.TryGetValue(geoId, out RegionInfo region)
				? region
				: null;
		}

		public static RegionInfo Get(string name = null)
		{
			return string.IsNullOrEmpty(name) || Default.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
						? Default
						: Regions.Values?.FirstOrDefault(e => StringComparer.OrdinalIgnoreCase.Equals(e.Name, name));
		}

		public static RegionInfo Get(string name, [NotNull] params string[] restrictTo)
		{
			name ??= Default.Name;
			return IsSupported(name, restrictTo)
						? Get(name)
						: null;
		}

		public static bool IsRegionGeoId(int geoId) { return geoId > 0 && Regions.ContainsKey(geoId); }

		public static bool IsRegionName(string name) { return !string.IsNullOrEmpty(name) && Regions.Values?.FirstOrDefault(e => StringComparer.OrdinalIgnoreCase.Equals(e.Name, name)) != null; }

		public static bool IsSupported(string name, [NotNull] params string[] restrictTo)
		{
			return IsRegionName(name) && (restrictTo.Length == 0 || restrictTo.Contains(name, StringComparer.OrdinalIgnoreCase));
		}

		public static bool IsSupported(string name, [NotNull] ICollection<string> restrictTo)
		{
			return IsRegionName(name) && (restrictTo.Count == 0 || restrictTo.Contains(name));
		}
	}
}