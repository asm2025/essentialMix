using System;
using System.Globalization;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Globalization;

public class Language
{
	private RegionInfo _region;

	/// <inheritdoc />
	public Language(int lcid)
		: this(lcid, null)
	{
	}

	/// <inheritdoc />
	public Language(int lcid, string displayName)
		: this(CultureInfo.GetCultureInfo(lcid), displayName)
	{
	}

	/// <inheritdoc />
	public Language([NotNull] string name)
		: this(name, null)
	{
	}

	/// <inheritdoc />
	public Language([NotNull] string name, string displayName)
		: this(CultureInfo.GetCultureInfo(name), displayName)
	{
	}

	/// <inheritdoc />
	public Language([NotNull] CultureInfo culture)
		: this(culture, null)
	{
	}

	public Language([NotNull] CultureInfo culture, string displayName)
	{
		if (culture == null) throw new ArgumentNullException(nameof(culture));
		Culture = CultureInfo.ReadOnly(culture);
		if (string.IsNullOrWhiteSpace(displayName)) displayName = null;
		DisplayName = displayName ?? Culture.NativeName;
	}

	/// <summary>
	/// The ISO 639-1 Code of the language
	/// </summary>
	[NotNull]
	public string Name => Culture.Name;

	[NotNull]
	public string DisplayName { get; }

	[NotNull]
	public CultureInfo Culture { get; }
	[NotNull]
	public RegionInfo Region => _region ??= Culture.Region();
}