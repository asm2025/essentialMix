using System.Web.UI.WebControls;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Web.Extensions
{
	public static class WebControlExtension
	{
		public static bool HasCssClass([NotNull] this WebControl thisValue, string className)
		{
			return !string.IsNullOrWhiteSpace(className) &&
					thisValue.CssClass.IsLike(string.Format(RegexHelper.RGX_WORD, className.Trim()));
		}

		public static void AddCssClass([NotNull] this WebControl thisValue, string className)
		{
			if (string.IsNullOrWhiteSpace(className) || HasCssClass(thisValue, className)) return;
			thisValue.CssClass = thisValue.CssClass == null ? className.Trim() : thisValue.CssClass.Trim().Join(' ', className.Trim());
		}

		public static void RemoveCssClass([NotNull] this WebControl thisValue, string className)
		{
			if (!HasCssClass(thisValue, className)) return;
			thisValue.CssClass = thisValue.CssClass
				.Replace(string.Format(RegexHelper.RGX_WORD, className.Trim()), string.Empty, RegexHelper.OPTIONS_I).
					Trim();
		}

		public static void ToggleCssClass([NotNull] this WebControl thisValue, string className)
		{
			if (string.IsNullOrWhiteSpace(className)) return;

			if (HasCssClass(thisValue, className)) RemoveCssClass(thisValue, className);
			else AddCssClass(thisValue, className);
		}
	}
}
