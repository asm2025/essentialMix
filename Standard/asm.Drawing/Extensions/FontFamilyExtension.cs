using System.Drawing;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Drawing.Extensions
{
	public static class FontFamilyExtension
	{
		public static bool HasAnyStyle([NotNull] this FontFamily thisValue, [NotNull] params FontStyle[] styles)
		{
			return styles.Length == 0 || styles.Any(thisValue.IsStyleAvailable);
		}

		public static bool HasStyles([NotNull] this FontFamily thisValue, [NotNull] params FontStyle[] styles)
		{
			return styles.Length == 0 || styles.All(thisValue.IsStyleAvailable);
		}
	}
}