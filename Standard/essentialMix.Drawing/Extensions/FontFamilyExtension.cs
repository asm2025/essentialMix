using System.Drawing;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class FontFamilyExtension
	{
		public static bool HasAnyStyle([NotNull] this System.Drawing.FontFamily thisValue, [NotNull] params FontStyle[] styles)
		{
			return styles.Length == 0 || styles.Any(thisValue.IsStyleAvailable);
		}

		public static bool HasStyles([NotNull] this System.Drawing.FontFamily thisValue, [NotNull] params FontStyle[] styles)
		{
			return styles.Length == 0 || styles.All(thisValue.IsStyleAvailable);
		}
	}
}