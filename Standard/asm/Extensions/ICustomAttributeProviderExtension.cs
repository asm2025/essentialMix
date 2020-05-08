using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class ICustomAttributeProviderExtension
	{
		private static readonly Type BASE_TYPE = typeof(Attribute);
		private static readonly Type DISPLAY_ATTRIBUTE_TYPE = typeof(DisplayAttribute);
		private static readonly Type DISPLAY_NAME_ATTRIBUTE_TYPE = typeof(DisplayNameAttribute);
		private static readonly Type DESCRIPTION_ATTRIBUTE_TYPE = typeof(DescriptionAttribute);

		public static string GetDisplayName([NotNull] this ICustomAttributeProvider thisValue, string defaultValue = null)
		{
			DisplayAttribute displayAttribute = (DisplayAttribute)GetAttribute(thisValue, DISPLAY_ATTRIBUTE_TYPE);
			string displayName = displayAttribute?.Name;
			if (displayName != null) return displayName;

			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)GetAttribute(thisValue, DISPLAY_NAME_ATTRIBUTE_TYPE);
			return displayNameAttribute?.DisplayName ?? defaultValue;
		}

		public static DisplayAttribute GetDisplay([NotNull] this ICustomAttributeProvider thisValue) { return (DisplayAttribute)GetAttribute(thisValue, DISPLAY_ATTRIBUTE_TYPE); }

		public static string GetDescription([NotNull] this ICustomAttributeProvider thisValue, string defaultValue = null)
		{
			DescriptionAttribute attribute = (DescriptionAttribute)GetAttribute(thisValue, DESCRIPTION_ATTRIBUTE_TYPE);
			return attribute?.Description ?? defaultValue;
		}

		[NotNull]
		public static IEnumerable<T> GetAttributes<T>([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false)
			where T : Attribute
		{
			return GetAttributes(thisValue, typeof(T), inherit).OfType<T>();
		}

		public static T GetAttribute<T>([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false)
			where T : Attribute
		{
			return GetAttributes<T>(thisValue, inherit).FirstOrDefault();
		}

		public static bool HasAttribute<T>([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false)
			where T : Attribute
		{
			return GetAttribute<T>(thisValue, inherit) != null;
		}

		[NotNull]
		public static IEnumerable<Attribute> GetAttributes([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false)
		{
			return GetAttributes(thisValue, typeof(Attribute), inherit);
		}

		public static Attribute GetAttribute([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false) { return GetAttributes(thisValue, inherit).FirstOrDefault(); }

		public static bool HasAttribute([NotNull] this ICustomAttributeProvider thisValue, bool inherit = false) { return GetAttributes(thisValue, inherit).Any(); }

		[NotNull]
		public static IEnumerable<Attribute> GetAttributes([NotNull] this ICustomAttributeProvider thisValue, Type type = null, bool inherit = false)
		{
			if (type != null && !type.Is(BASE_TYPE)) throw new InvalidCastException($"The type must be derived from type of {BASE_TYPE.FullName}");

			object[] attributes = type != null 
				? thisValue.GetCustomAttributes(type, inherit)
				: thisValue.GetCustomAttributes(inherit);
			return attributes.Cast<Attribute>();
		}

		public static Attribute GetAttribute([NotNull] this ICustomAttributeProvider thisValue, Type type = null, bool inherit = false)
		{
			return GetAttributes(thisValue, type, inherit).FirstOrDefault();
		}

		public static bool HasAttribute([NotNull] this ICustomAttributeProvider thisValue, Type type = null, bool inherit = false)
		{
			return GetAttributes(thisValue, type, inherit).Any();
		}
	}
}