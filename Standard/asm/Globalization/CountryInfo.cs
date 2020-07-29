using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Globalization
{
	[DebuggerDisplay("{Name}")]
	public class CountryInfo
	{
		private static ReadOnlyDictionary<string, CountryInfo> __countries;

		private ReadOnlyDictionary<string, CultureInfo> _cultures;

		/// <inheritdoc />
		public CountryInfo(int geoId)
			: this(RegionInfoHelper.GetByGeoId(geoId))
		{
		}

		/// <inheritdoc />
		public CountryInfo(string name = null)
			: this(RegionInfoHelper.Get(name))
		{
		}

		public CountryInfo([NotNull] RegionInfo region)
		{
			Region = region;
		}

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Name; }

		public int GeoId => Region.GeoId;

		[NotNull]
		public string Name => Region.Name;

		[NotNull]
		public string DisplayName => (UseNativeName ? Region.NativeName : Region.DisplayName) ?? Name;

		[NotNull]
		public RegionInfo Region { get; }

		[NotNull]
		public IReadOnlyDictionary<string, CultureInfo> Cultures => _cultures ??= CultureInfoHelper.ListByRegion(Region);

		public static bool UseNativeName { get; set; }

		[NotNull]
		public static ReadOnlyDictionary<string, CountryInfo> Countries
		{
			get
			{
				return __countries ??= new ReadOnlyDictionary<string, CountryInfo>((RegionInfoHelper.Regions.Values ?? throw new InvalidOperationException("Cannot access regions"))
													.Where(e => e.Name.Length == 2)
													.Select(e => new CountryInfo(e))
													.ToDictionary(k => k.Name, StringComparer.OrdinalIgnoreCase));
			}
		}

		[NotNull]
		public static IEnumerable<CountryInfo> GetCountriesByCulture(string culture = null)
		{
			return GetCountriesByCulture(CultureInfoHelper.Get(culture));
		}

		[NotNull]
		public static IEnumerable<CountryInfo> GetCountriesByCulture([NotNull] CultureInfo culture)
		{
			ReadOnlyDictionary<string, CountryInfo>.ValueCollection countries = Countries.Values ?? throw new InvalidOperationException("Cannot access Countries.");
			return culture.IsNeutralCulture
				? countries.Where(e => e.Cultures.Values.Any(c => c.Name.StartsWith(culture.Name)))
				: countries.Where(e => e.Cultures.ContainsKey(culture.Name));
		}
	}
}
