using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class HtmlTagExtension
	{
		public static HtmlTag Checked([NotNull] this HtmlTag thisValue, bool value) { return thisValue.ToggleAttribute("checked", value); }

		public static HtmlTag Disabled([NotNull] this HtmlTag thisValue, bool value) { return thisValue.ToggleAttribute("disabled", value); }

		public static HtmlTag Selected([NotNull] this HtmlTag thisValue, bool value) { return thisValue.ToggleAttribute("selected", value); }

		public static HtmlTag Readonly([NotNull] this HtmlTag thisValue, bool value) { return thisValue.ToggleAttribute("readonly", value); }

		[NotNull]
		public static string Text([NotNull] this HtmlTag thisValue) { return string.Concat(TextContents(thisValue)); }

		[ItemNotNull]
		public static IEnumerable<string> TextContents([NotNull] this HtmlTag thisValue)
		{
			foreach (IHtmlElement content in thisValue.Contents)
			{
				switch (content)
				{
					case HtmlText htmlText:
						if (!string.IsNullOrEmpty(htmlText.Text)) yield return htmlText.Text;
						continue;
					case HtmlTag _:
						yield return thisValue.Text();
						break;
				}
			}
		}

		[NotNull]
		public static HtmlTag Width([NotNull] this HtmlTag thisValue, [NotNull] string value, bool replaceExisting = true)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return thisValue.Style("width", value, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Height([NotNull] this HtmlTag thisValue, [NotNull] string value, bool replaceExisting = true)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return thisValue.Style("height", value, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Margin([NotNull] this HtmlTag thisValue, [NotNull] string value, bool replaceExisting = true)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return thisValue.Style("margin", value, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Padding([NotNull] this HtmlTag thisValue, [NotNull] string value, bool replaceExisting = true)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			return thisValue.Style("padding", value, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Color([NotNull] this HtmlTag thisValue, [NotNull] string color, bool replaceExisting = true)
		{
			if (color == null) throw new ArgumentNullException(nameof(color));
			return thisValue.Style("color", color, replaceExisting);
		}

		[NotNull]
		public static HtmlTag TextAlign([NotNull] this HtmlTag thisValue, [NotNull] string textAlign, bool replaceExisting = true)
		{
			if (textAlign == null) throw new ArgumentNullException(nameof(textAlign));
			return thisValue.Style("text-align", textAlign, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Border([NotNull] this HtmlTag thisValue, [NotNull] string border, bool replaceExisting = true)
		{
			if (border == null) throw new ArgumentNullException(nameof(border));
			return thisValue.Style("border", border, replaceExisting);
		}

		[NotNull]
		public static HtmlTag Name([NotNull] this HtmlTag thisValue, string name, bool replaceExisting = true) { return thisValue.Attribute("name", name, replaceExisting); }

		[NotNull]
		public static HtmlTag Title([NotNull] this HtmlTag thisValue, string title, bool replaceExisting = true) { return thisValue.Attribute("title", title, replaceExisting); }

		[NotNull]
		public static HtmlTag Id([NotNull] this HtmlTag thisValue, string id, bool replaceExisting = true) { return thisValue.Attribute("id", id, replaceExisting); }

		[NotNull]
		public static HtmlTag Type([NotNull] this HtmlTag thisValue, string type, bool replaceExisting = true) { return thisValue.Attribute("type", type, replaceExisting); }

		[NotNull]
		public static HtmlTag Value([NotNull] this HtmlTag thisValue, string value, bool replaceExisting = true) { return thisValue.Attribute("value", value, replaceExisting); }

		[NotNull]
		public static HtmlTag Href([NotNull] this HtmlTag thisValue, string href, bool replaceExisting = true) { return thisValue.Attribute("href", href, replaceExisting); }
	}
}