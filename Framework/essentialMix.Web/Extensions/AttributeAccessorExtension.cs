using System.Web.UI;
using JetBrains.Annotations;
using essentialMix.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class AttributeAccessorExtension
{
	public static string Attribute([NotNull] this IAttributeAccessor thisValue, HtmlTextWriterAttribute attribute)
	{
		return Attribute<string>(thisValue, attribute.ToString());
	}

	public static T Attribute<T>([NotNull] this IAttributeAccessor thisValue, HtmlTextWriterAttribute attribute)
	{
		return Attribute(thisValue, attribute.ToString(), default(T));
	}

	public static T Attribute<T>([NotNull] this IAttributeAccessor thisValue, HtmlTextWriterAttribute attribute, T defaultValue)
	{
		return Attribute(thisValue, attribute.ToString(), defaultValue);
	}

	public static string Attribute([NotNull] this IAttributeAccessor thisValue, string name) { return Attribute<string>(thisValue, name); }

	public static T Attribute<T>([NotNull] this IAttributeAccessor thisValue, string name) { return Attribute(thisValue, name, default(T)); }

	public static T Attribute<T>([NotNull] this IAttributeAccessor thisValue, string name, T defaultValue)
	{
		string value;

		try
		{
			value = thisValue.GetAttribute(name);
		}
		catch
		{
			value = null;
		}

		return value.To(defaultValue);
	}

	public static bool Attribute([NotNull] this IAttributeAccessor thisValue, HtmlTextWriterAttribute attribute, string value)
	{
		return Attribute(thisValue, attribute.ToString(), value);
	}

	public static bool Attribute([NotNull] this IAttributeAccessor thisValue, string attribute, string value)
	{
		bool result;

		try
		{
			thisValue.SetAttribute(attribute, value);
			result = true;
		}
		catch
		{
			result = false;
		}

		return result;
	}

	public static bool HasCssClass([NotNull] this IAttributeAccessor thisValue, string className)
	{
		return !string.IsNullOrEmpty(className) && Attribute(thisValue, HtmlTextWriterAttribute.Class).IsLike(string.Format(RegexHelper.RGX_WORD, className.Trim()));
	}

	public static void AddCssClass([NotNull] this IAttributeAccessor thisValue, string className)
	{
		if (string.IsNullOrWhiteSpace(className)) return;
		string classAttr = HtmlTextWriterAttribute.Class.ToString();
		string attribute = Attribute<string>(thisValue, classAttr, string.Empty).Trim();
		string cssClass = className.Trim();
		if (attribute.IsLike(string.Format(RegexHelper.RGX_WORD, cssClass))) return;
		Attribute(thisValue, classAttr, string.IsNullOrEmpty(attribute) ? cssClass : attribute.Join(' ', cssClass));
	}

	public static void RemoveCssClass([NotNull] this IAttributeAccessor thisValue, string className)
	{
		if (!HasCssClass(thisValue, className)) return;
		string classAttr = HtmlTextWriterAttribute.Class.ToString();
		Attribute(thisValue,
				classAttr,
				Attribute<string>(thisValue, classAttr, string.Empty)
					.Replace(string.Format(RegexHelper.RGX_WORD, className.Trim()), string.Empty, RegexHelper.OPTIONS_I)
					.Trim());
	}

	public static void ToggleCssClass([NotNull] this IAttributeAccessor thisValue, string className)
	{
		if (string.IsNullOrWhiteSpace(className)) return;

		if (HasCssClass(thisValue, className)) RemoveCssClass(thisValue, className);
		else AddCssClass(thisValue, className);
	}
}