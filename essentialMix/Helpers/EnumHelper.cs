using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using essentialMix.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class EnumHelper
	{
		[NotNull]
		public static Type GetUnderlyingType([NotNull] Type enumType) { return Enums.GetUnderlyingType(enumType); }

		public static TypeCode GetTypeCode([NotNull] Type enumType) { return Enums.GetTypeCode(enumType); }
		public static int GetCount([NotNull] Type enumType) { return Enums.GetMemberCount(enumType); }
		public static int GetCount([NotNull] Type enumType, EnumMemberSelection selection) { return Enums.GetMemberCount(enumType, selection); }

		[NotNull]
		public static IReadOnlyList<EnumMember> GetMembers([NotNull] Type enumType) { return Enums.GetMembers(enumType); }
		[NotNull]
		public static IReadOnlyList<EnumMember> GetMembers([NotNull] Type enumType, EnumMemberSelection selection) { return Enums.GetMembers(enumType, selection); }

		[NotNull]
		public static IReadOnlyList<string> GetNames([NotNull] Type enumType) { return Enums.GetNames(enumType); }
		[NotNull]
		public static IReadOnlyList<string> GetNames([NotNull] Type enumType, EnumMemberSelection selection) { return Enums.GetNames(enumType, selection); }

		[NotNull]
		public static IEnumerable<string> GetDisplayNames([NotNull] Type enumType) { return Enums.GetMembers(enumType).Select(e => e.AsString(EnumFormat.DisplayName)); }
		[NotNull]
		public static IEnumerable<string> GetDisplayNames([NotNull] Type enumType, EnumMemberSelection selection) { return Enums.GetMembers(enumType, selection).Select(e => e.AsString(EnumFormat.DisplayName)); }

		[NotNull]
		public static IEnumerable<Enum> GetValues([NotNull] Type enumType) { return Enums.GetValues(enumType).Cast<Enum>(); }
		[NotNull]
		public static IEnumerable<Enum> GetValues([NotNull] Type enumType, EnumMemberSelection selection) { return Enums.GetValues(enumType, selection).Cast<Enum>(); }

		public static (Enum Minimum, Enum Maximum) GetBoundaries([NotNull] Type enumType) { return GetValues(enumType).FirstAndLast(); }

		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, [NotNull] object value) { return ToObject(enumType, value, EnumValidation.None); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, [NotNull] object value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, sbyte value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, sbyte value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, byte value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, byte value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, short value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, short value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, ushort value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, ushort value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, int value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, int value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, uint value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, uint value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, long value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, long value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, ulong value) { return (Enum)Enums.ToObject(enumType, value); }
		[NotNull]
		public static Enum ToObject([NotNull] Type enumType, ulong value, EnumValidation validation) { return (Enum)Enums.ToObject(enumType, value, validation); }

		public static bool TryToObject([NotNull] Type enumType, object value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, object value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, sbyte value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, sbyte value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, byte value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, byte value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, short value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, short value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, ushort value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, ushort value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, int value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, int value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, uint value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, uint value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, long value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, long value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, ulong value, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool TryToObject([NotNull] Type enumType, ulong value, EnumValidation validation, out Enum result)
		{
			if (!Enums.TryToObject(enumType, value, out object obj, validation))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool IsValid([NotNull] Type enumType, [NotNull] object value) { return IsValid(enumType, value, EnumValidation.Default); }
		public static bool IsValid([NotNull] Type enumType, [NotNull] object value, EnumValidation validation) { return Enums.IsValid(enumType, value, validation); }

		public static bool IsDefined([NotNull] Type enumType, [NotNull] Enum value) { return Enums.IsDefined(enumType, value); }

		[NotNull]
		public static Enum Validate([NotNull] Type enumType, [NotNull] object value, [NotNull] string paramName) { return Validate(enumType, value, paramName, EnumValidation.Default); }

		[NotNull]
		public static Enum Validate([NotNull] Type enumType, [NotNull] object value, [NotNull] string paramName, EnumValidation validation)
		{
			return (Enum)Enums.Validate(enumType, value, paramName, validation);
		}

		[NotNull]
		public static string AsString([NotNull] Type enumType, [NotNull] Enum value) { return Enums.AsString(enumType, value); }
		[NotNull]
		public static string AsString([NotNull] Type enumType, [NotNull] Enum value, string format) { return Enums.AsString(enumType, value, format); }
		public static string AsString([NotNull] Type enumType, [NotNull] Enum value, EnumFormat format) { return Enums.AsString(enumType, value, format); }
		public static string AsString([NotNull] Type enumType, [NotNull] Enum value, EnumFormat format0, EnumFormat format1) { return Enums.AsString(enumType, value, format0, format1); }

		public static string AsString([NotNull] Type enumType, [NotNull] Enum value, EnumFormat format0, EnumFormat format1, EnumFormat format2)
		{
			return Enums.AsString(enumType, value, format0, format1, format2);
		}

		public static string AsString([NotNull] Type enumType, [NotNull] Enum value, params EnumFormat[] formats) { return Enums.AsString(enumType, value, formats); }

		public static string Format([NotNull] Type enumType, Enum value, [NotNull] string format) { return Enums.Format(enumType, value, format); }

		[NotNull]
		public static object GetUnderlyingValue([NotNull] Type enumType, [NotNull] Enum value) { return Enums.GetUnderlyingValue(enumType, value); }

		public static sbyte ToSByte([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToSByte(enumType, value); }
		public static byte ToByte([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToByte(enumType, value); }
		public static short ToInt16([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToInt16(enumType, value); }
		public static ushort ToUInt16([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToUInt16(enumType, value); }
		public static int ToInt32([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToInt32(enumType, value); }
		public static uint ToUInt32([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToUInt32(enumType, value); }
		public static long ToInt64([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToInt64(enumType, value); }
		public static ulong ToUInt64([NotNull] Type enumType, [NotNull] Enum value) { return Enums.ToUInt64(enumType, value); }

		public static bool Equals([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum other) { return Enums.Equals(enumType, value, other); }

		public static int CompareTo([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum other) { return Enums.CompareTo(enumType, value, other); }

		public static string GetName([NotNull] Type enumType, [NotNull] Enum value) { return Enums.GetName(enumType, value); }

		public static AttributeCollection GetAttributes([NotNull] Type enumType, [NotNull] Enum value) { return Enums.GetAttributes(enumType, value); }

		public static EnumMember GetMember([NotNull] Type enumType, [NotNull] Enum value) { return Enums.GetMember(enumType, value); }
		public static EnumMember GetMember([NotNull] Type enumType, [NotNull] string name) { return Enums.GetMember(enumType, name); }
		public static EnumMember GetMember([NotNull] Type enumType, [NotNull] string name, bool ignoreCase) { return Enums.GetMember(enumType, name, ignoreCase); }
		public static EnumMember GetMember([NotNull] Type enumType, [NotNull] string value, params EnumFormat[] formats) { return Enums.GetMember(enumType, value, false, formats); }
		public static EnumMember GetMember([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return Enums.GetMember(enumType, value, ignoreCase, formats); }

		[NotNull]
		public static Enum Parse([NotNull] Type enumType, [NotNull] string value) { return Parse(enumType, value, false, null); }
		[NotNull]
		public static Enum Parse([NotNull] Type enumType, [NotNull] string value, params EnumFormat[] formats) { return Parse(enumType, value, false, formats); }
		[NotNull]
		public static Enum Parse([NotNull] Type enumType, [NotNull] string value, bool ignoreCase) { return Parse(enumType, value, ignoreCase, null); }
		[NotNull]
		public static Enum Parse([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return (Enum)Enums.Parse(enumType, value, ignoreCase, formats); }

		public static bool TryParse([NotNull] Type enumType, string value, out Enum result) { return TryParse(enumType, value, false, out result, null); }
		public static bool TryParse([NotNull] Type enumType, string value, out Enum result, params EnumFormat[] formats) { return TryParse(enumType, value, false, out result, formats); }
		public static bool TryParse([NotNull] Type enumType, string value, bool ignoreCase, out Enum result) { return TryParse(enumType, value, ignoreCase, out result, null); }
		public static bool TryParse([NotNull] Type enumType, string value, bool ignoreCase, out Enum result, params EnumFormat[] formats)
		{
			if (!Enums.TryParse(enumType, value, ignoreCase, out object obj, formats))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}

		public static bool IsFlagEnum([NotNull] Type enumType) { return FlagEnums.IsFlagEnum(enumType); }

		[NotNull]
		public static Enum GetAllFlags([NotNull] Type enumType) { return (Enum)FlagEnums.GetAllFlags(enumType); }

		public static bool IsValidFlagCombination([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.IsValidFlagCombination(enumType, value); }

		public static string FormatFlags([NotNull] Type enumType, [NotNull] Enum value) { return FormatFlags(enumType, value, null, null); }
		public static string FormatFlags([NotNull] Type enumType, [NotNull] Enum value, params EnumFormat[] formats) { return FormatFlags(enumType, value, null, formats); }
		public static string FormatFlags([NotNull] Type enumType, [NotNull] Enum value, string delimiter) { return FormatFlags(enumType, value, delimiter, null); }
		public static string FormatFlags([NotNull] Type enumType, [NotNull] Enum value, string delimiter, params EnumFormat[] formats) { return FlagEnums.FormatFlags(enumType, value, delimiter, formats); }

		[NotNull]
		public static IReadOnlyList<object> GetFlags([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.GetFlags(enumType, value); }

		[NotNull]
		public static IReadOnlyList<EnumMember> GetFlagMembers([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.GetFlagMembers(enumType, value); }

		public static int GetFlagCount([NotNull] Type enumType) { return FlagEnums.GetFlagCount(enumType); }
		public static int GetFlagCount([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.GetFlagCount(enumType, value); }

		public static int GetFlagCount([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return FlagEnums.GetFlagCount(enumType, value, otherFlags); }

		public static bool HasAnyFlags([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.HasAnyFlags(enumType, value); }
		public static bool HasAnyFlags([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return FlagEnums.HasAnyFlags(enumType, value, otherFlags); }

		public static bool HasAllFlags([NotNull] Type enumType, [NotNull] Enum value) { return FlagEnums.HasAllFlags(enumType, value); }

		[NotNull]
		public static Enum ToggleFlags([NotNull] Type enumType, [NotNull] Enum value) { return (Enum)FlagEnums.ToggleFlags(enumType, value); }
		[NotNull]
		public static Enum ToggleFlags([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return (Enum)FlagEnums.ToggleFlags(enumType, value, otherFlags); }

		[NotNull]
		public static Enum CommonFlags([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return (Enum)FlagEnums.CommonFlags(enumType, value, otherFlags); }

		[NotNull]
		public static Enum CombineFlags([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return (Enum)FlagEnums.CombineFlags(enumType, value, otherFlags); }
		[NotNull]
		public static Enum CombineFlags([NotNull] Type enumType, params Enum[] flags) { return CombineFlags(enumType, (IEnumerable<Enum>)flags); }
		[NotNull]
		public static Enum CombineFlags([NotNull] Type enumType, IEnumerable<Enum> flags) { return (Enum)FlagEnums.CombineFlags(enumType, flags); }

		[NotNull]
		public static Enum RemoveFlags([NotNull] Type enumType, [NotNull] Enum value, [NotNull] Enum otherFlags) { return (Enum)FlagEnums.RemoveFlags(enumType, value, otherFlags); }

		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value) { return ParseFlags(enumType, value, false, null, null); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, params EnumFormat[] formats) { return ParseFlags(enumType, value, false, null, formats); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase) { return ParseFlags(enumType, value, ignoreCase, null, null); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return ParseFlags(enumType, value, ignoreCase, null, formats); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, string delimiter) { return ParseFlags(enumType, value, false, delimiter, null); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, string delimiter, params EnumFormat[] formats) { return ParseFlags(enumType, value, false, delimiter, formats); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, string delimiter) { return ParseFlags(enumType, value, ignoreCase, delimiter, null); }
		[NotNull]
		public static Enum ParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, string delimiter, params EnumFormat[] formats) { return (Enum)FlagEnums.ParseFlags(enumType, value, ignoreCase, delimiter, formats); }

		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, out Enum result) { return TryParseFlags(enumType, value, false, null, out result, null); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, out Enum result, params EnumFormat[] formats) { return TryParseFlags(enumType, value, false, null, out result, formats); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, out Enum result) { return TryParseFlags(enumType, value, ignoreCase, null, out result, null); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, out Enum result, params EnumFormat[] formats) { return TryParseFlags(enumType, value, ignoreCase, null, out result, formats); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, string delimiter, out Enum result) { return TryParseFlags(enumType, value, false, delimiter, out result, null); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, string delimiter, out Enum result, params EnumFormat[] formats) { return TryParseFlags(enumType, value, false, delimiter, out result, formats); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, string delimiter, out Enum result) { return TryParseFlags(enumType, value, ignoreCase, delimiter, out result, null); }
		public static bool TryParseFlags([NotNull] Type enumType, [NotNull] string value, bool ignoreCase, string delimiter, out Enum result, params EnumFormat[] formats)
		{
			if (!FlagEnums.TryParseFlags(enumType, value, ignoreCase, delimiter, out object obj, formats))
			{
				result = null;
				return false;
			}

			result = (Enum)obj;
			return true;
		}
	}

	public static class EnumHelper<TEnum>
		where TEnum : struct, Enum, IComparable
	{
		[NotNull]
		public static Type GetUnderlyingType() { return Enums.GetUnderlyingType<TEnum>(); }

		public static TypeCode GetTypeCode() { return GetUnderlyingType().AsTypeCode(); }

		public static int GetCount() { return FlagEnums.GetFlagCount<TEnum>(); }
		public static int GetCount(TEnum selection) { return selection.GetFlagCount(); }

		[NotNull]
		public static IReadOnlyList<EnumMember<TEnum>> GetMembers() { return Enums.GetMembers<TEnum>(); }
		[NotNull]
		public static IReadOnlyList<EnumMember<TEnum>> GetMembers(EnumMemberSelection selection) { return Enums.GetMembers<TEnum>(selection); }

		[NotNull]
		public static IReadOnlyList<string> GetNames() { return Enums.GetNames<TEnum>(); }
		[NotNull]
		public static IReadOnlyList<string> GetNames(EnumMemberSelection selection) { return Enums.GetNames<TEnum>(selection); }

		[NotNull]
		public static IEnumerable<string> GetDisplayNames() { return Enums.GetMembers<TEnum>().Select(e => e.AsString(EnumFormat.DisplayName)); }
		[NotNull]
		public static IEnumerable<string> GetDisplayNames(EnumMemberSelection selection) { return Enums.GetMembers<TEnum>(selection).Select(e => e.AsString(EnumFormat.DisplayName)); }

		[NotNull]
		public static IReadOnlyList<TEnum> GetValues() { return Enums.GetValues<TEnum>(); }
		[NotNull]
		public static IReadOnlyList<TEnum> GetValues(EnumMemberSelection selection) { return Enums.GetValues<TEnum>(selection); }

		public static (TEnum Minimum, TEnum Maximum) GetBoundaries() { return GetValues().FirstAndLast(); }

		[NotNull]
		public static EnumRange<TEnum> GetRange() { return new EnumRange<TEnum>(); }

		public static TEnum ToObject([NotNull] object value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject([NotNull] object value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(sbyte value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(sbyte value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(byte value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(byte value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(short value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(short value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(ushort value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(ushort value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(int value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(int value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(uint value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(uint value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(long value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(long value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }
		public static TEnum ToObject(ulong value) { return Enums.ToObject<TEnum>(value); }
		public static TEnum ToObject(ulong value, EnumValidation validation) { return Enums.ToObject<TEnum>(value, validation); }

		public static bool TryToObject(object value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(object value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(sbyte value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(sbyte value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(byte value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(byte value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(short value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(short value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(ushort value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(ushort value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(int value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(int value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(uint value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(uint value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(long value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(long value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }
		public static bool TryToObject(ulong value, out TEnum result) { return Enums.TryToObject(value, out result); }
		public static bool TryToObject(ulong value, EnumValidation validation, out TEnum result) { return Enums.TryToObject(value, out result, validation); }

		public static EnumMember<TEnum> GetMember([NotNull] string name) { return Enums.GetMember<TEnum>(name); }
		public static EnumMember<TEnum> GetMember([NotNull] string name, bool ignoreCase) { return Enums.GetMember<TEnum>(name, ignoreCase); }
		public static EnumMember<TEnum> GetMember([NotNull] string value, params EnumFormat[] formats) { return Enums.GetMember<TEnum>(value, false, formats); }
		public static EnumMember<TEnum> GetMember([NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return Enums.GetMember<TEnum>(value, ignoreCase, formats); }

		public static TEnum Parse([NotNull] string value) { return Enums.Parse<TEnum>(value); }
		public static TEnum Parse([NotNull] string value, params EnumFormat[] formats) { return Enums.Parse<TEnum>(value, false, formats); }
		public static TEnum Parse([NotNull] string value, bool ignoreCase) { return Enums.Parse<TEnum>(value, ignoreCase); }
		public static TEnum Parse([NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return Enums.Parse<TEnum>(value, ignoreCase, formats); }

		public static bool TryParse(string value, out TEnum result) { return Enums.TryParse(value, false, out result); }
		public static bool TryParse(string value, out TEnum result, params EnumFormat[] formats) { return Enums.TryParse(value, false, out result, formats); }
		public static bool TryParse(string value, bool ignoreCase, out TEnum result) { return Enums.TryParse(value, ignoreCase, out result); }
		public static bool TryParse(string value, bool ignoreCase, out TEnum result, params EnumFormat[] formats) { return Enums.TryParse(value, ignoreCase, out result, formats); }

		public static bool IsValid(TEnum value) { return value.IsValid(); }
		public static bool IsValid(TEnum value, EnumValidation validation) { return value.IsValid(validation); }

		public static bool IsDefined(TEnum value) { return value.IsDefined(); }

		public static TEnum Validate(TEnum value, [NotNull] string paramName) { return value.Validate(paramName); }

		public static TEnum Validate(TEnum value, [NotNull] string paramName, EnumValidation validation) { return value.Validate(paramName, validation); }

		[NotNull]
		public static string AsString(TEnum value) { return Enums.AsString(value); }
		[NotNull]
		public static string AsString(TEnum value, string format) { return value.AsString(format); }
		public static string AsString(TEnum value, EnumFormat format) { return value.AsString(format); }
		public static string AsString(TEnum value, EnumFormat format0, EnumFormat format1) { return value.AsString(format0, format1); }
		public static string AsString(TEnum value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return value.AsString(format0, format1, format2); }
		public static string AsString(TEnum value, params EnumFormat[] formats) { return value.AsString(formats); }

		[NotNull]
		public static string Format(TEnum value, [NotNull] string format) { return Enums.Format(value, format); }

		[NotNull]
		public static object GetUnderlyingValue(TEnum value) { return Enums.GetUnderlyingValue(value); }

		public static sbyte ToSByte(TEnum value) { return Enums.ToSByte(value); }
		public static byte ToByte(TEnum value) { return Enums.ToByte(value); }
		public static short ToInt16(TEnum value) { return Enums.ToInt16(value); }
		public static ushort ToUInt16(TEnum value) { return Enums.ToUInt16(value); }
		public static int ToInt32(TEnum value) { return Enums.ToInt32(value); }
		public static uint ToUInt32(TEnum value) { return Enums.ToUInt32(value); }
		public static long ToInt64(TEnum value) { return Enums.ToInt64(value); }
		public static ulong ToUInt64(TEnum value) { return Enums.ToUInt64(value); }

		public static int GetHashCode(TEnum value) { return Enums.GetHashCode(value); }

		public static bool Equals(TEnum value, TEnum other) { return Enums.Equals(value, other); }

		public static int CompareTo(TEnum value, TEnum other) { return value.CompareTo<TEnum>(other); }

		public static string GetName(TEnum value) { return Enums.GetName(value); }

		public static AttributeCollection GetAttributes(TEnum value) { return value.GetAttributes(); }

		public static EnumMember<TEnum> GetMember(TEnum value) { return value.GetMember(); }

		[NotNull]
		public static IReadOnlyList<TEnum> GetFlags(TEnum value) { return value.GetFlags(); }

		public static int GetFlagCount(TEnum value) { return value.GetFlagCount(); }
		public static int GetFlagCount(TEnum value, TEnum otherFlags) { return value.GetFlagCount(otherFlags); }

		public static bool HasAnyFlags(TEnum value) { return value.HasAnyFlags(); }
		public static bool HasAnyFlags(TEnum value, TEnum otherFlags) { return value.HasAnyFlags(otherFlags); }

		public static bool HasAllFlags(TEnum value) { return value.HasAllFlags(); }

		public static TEnum CommonFlags(TEnum value, TEnum otherFlags) { return value.CommonFlags(otherFlags); }

		public static TEnum CombineFlags(TEnum value, TEnum otherFlags) { return value.CombineFlags(otherFlags); }

		public static TEnum RemoveFlags(TEnum value, TEnum otherFlags) { return value.RemoveFlags(otherFlags); }

		public static bool IsFlagEnum() { return FlagEnums.IsFlagEnum<TEnum>(); }

		public static TEnum GetAllFlags() { return FlagEnums.GetAllFlags<TEnum>(); }

		public static bool IsValidFlagCombination(TEnum value) { return FlagEnums.IsValidFlagCombination(value); }

		[NotNull]
		public static string FormatFlags(TEnum value) { return FlagEnums.FormatFlags(value); }
		public static string FormatFlags(TEnum value, params EnumFormat[] formats) { return FlagEnums.FormatFlags(value, null, formats); }
		[NotNull]
		public static string FormatFlags(TEnum value, string delimiter) { return FlagEnums.FormatFlags(value, delimiter); }
		public static string FormatFlags(TEnum value, string delimiter, params EnumFormat[] formats) { return FlagEnums.FormatFlags(value, delimiter, formats); }

		[NotNull]
		public static IReadOnlyList<EnumMember<TEnum>> GetFlagMembers(TEnum value) { return FlagEnums.GetFlagMembers(value); }

		public static int GetFlagCount() { return FlagEnums.GetFlagCount<TEnum>(); }

		public static TEnum ToggleFlags(TEnum value) { return FlagEnums.ToggleFlags(value); }
		public static TEnum ToggleFlags(TEnum value, TEnum otherFlags) { return FlagEnums.ToggleFlags(value, otherFlags); }

		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2) { return FlagEnums.CombineFlags(flag0, flag1, flag2); }
		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3) { return FlagEnums.CombineFlags(flag0, flag1, flag2, flag3); }
		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3, TEnum flag4) { return FlagEnums.CombineFlags(flag0, flag1, flag2, flag3, flag4); }
		public static TEnum CombineFlags(params TEnum[] flags) { return FlagEnums.CombineFlags(flags); }
		public static TEnum CombineFlags(IEnumerable<TEnum> flags) { return FlagEnums.CombineFlags(flags); }

		public static TEnum ParseFlags([NotNull] string value) { return FlagEnums.ParseFlags<TEnum>(value); }
		public static TEnum ParseFlags([NotNull] string value, params EnumFormat[] formats) { return FlagEnums.ParseFlags<TEnum>(value, false, null, formats); }
		public static TEnum ParseFlags([NotNull] string value, bool ignoreCase) { return FlagEnums.ParseFlags<TEnum>(value, ignoreCase); }
		public static TEnum ParseFlags([NotNull] string value, bool ignoreCase, params EnumFormat[] formats) { return FlagEnums.ParseFlags<TEnum>(value, ignoreCase, null, formats); }
		public static TEnum ParseFlags([NotNull] string value, string delimiter) { return FlagEnums.ParseFlags<TEnum>(value, false, delimiter); }
		public static TEnum ParseFlags([NotNull] string value, string delimiter, params EnumFormat[] formats) { return FlagEnums.ParseFlags<TEnum>(value, false, delimiter, formats); }
		public static TEnum ParseFlags([NotNull] string value, bool ignoreCase, string delimiter) { return FlagEnums.ParseFlags<TEnum>(value, ignoreCase, delimiter); }
		public static TEnum ParseFlags([NotNull] string value, bool ignoreCase, string delimiter, params EnumFormat[] formats) { return FlagEnums.ParseFlags<TEnum>(value, ignoreCase, delimiter, formats); }

		public static bool TryParseFlags(string value, out TEnum result) { return FlagEnums.TryParseFlags(value, false, null, out result); }
		public static bool TryParseFlags(string value, out TEnum result, params EnumFormat[] formats) { return FlagEnums.TryParseFlags(value, false, null, out result, formats); }
		public static bool TryParseFlags(string value, bool ignoreCase, out TEnum result) { return FlagEnums.TryParseFlags(value, ignoreCase, null, out result); }
		public static bool TryParseFlags(string value, bool ignoreCase, out TEnum result, params EnumFormat[] formats) { return FlagEnums.TryParseFlags(value, ignoreCase, null, out result, formats); }
		public static bool TryParseFlags(string value, string delimiter, out TEnum result) { return FlagEnums.TryParseFlags(value, false, delimiter, out result); }
		public static bool TryParseFlags(string value, string delimiter, out TEnum result, params EnumFormat[] formats) { return FlagEnums.TryParseFlags(value, false, delimiter, out result, formats); }
		public static bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result) { return FlagEnums.TryParseFlags(value, ignoreCase, delimiter, out result); }
		public static bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result, params EnumFormat[] formats) { return FlagEnums.TryParseFlags(value, ignoreCase, delimiter, out result, formats); }
	}
}