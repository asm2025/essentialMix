using System.Drawing;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysFontFamily = System.Drawing.FontFamily;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class FontFamilyExtension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool HasAnyStyle([NotNull] this SysFontFamily thisValue, [NotNull] params FontStyle[] styles)
	{
		return styles.Length == 0 || styles.Any(thisValue.IsStyleAvailable);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool HasAllStyles([NotNull] this SysFontFamily thisValue, [NotNull] params FontStyle[] styles)
	{
		return styles.Length == 0 || styles.All(thisValue.IsStyleAvailable);
	}
}