using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Collections;
using asm.ComponentModel.DataAnnotations;
using asm.Numeric;

namespace asm.Enumeration
{
	// Class that acts as a bridge from the enum type to the underlying type
	// through the use of the implemented interfaces IEnumInfo<TEnum> and IEnumInfo.
	// Also acts as a bridge in the reverse from the underlying type to the enum type
	// through the use of the implemented interface IEnumInfoInternal<TInt, TIntProvider>.
	// Putting the logic in EnumCache<TInt, TIntProvider> reduces memory usage
	// because having the enum type as a generic type parameter causes code explosion
	// due to how .NET generics are handled with enums.
	internal class EnumInfo<TInt, TIntProvider> : IEnumInfo, IEnumInfo<TInt, TIntProvider>
		where TInt : struct, IComparable<TInt>, IComparable, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		private readonly EnumCache<TInt, TIntProvider> _cache;
		private readonly IEnumValidatorAttribute _customEnumValidator;

		public EnumInfo([NotNull] Type enumType)
		{
			EnumType = enumType;
			_cache = new EnumCache<TInt, TIntProvider>(EnumType, this);
			_customEnumValidator = GetCustomEnumValidator(EnumType);
		}

		public Type EnumType { get; }

		public TypeCode TypeCode => UnderlyingType.AsTypeCode();

		[NotNull]
		public Type UnderlyingType => typeof(TInt);

		public bool IsFlagEnum => _cache.IsFlagEnum;

		public Enum AllFlags => ToEnum(_cache.AllFlags);

		public bool HasCustomValidator => _customEnumValidator != null;

		Enum IEnumInfo.AllFlags => AllFlags;

		public int GetCount(EnumMemberSelection selection) { return _cache.GetMemberCount(selection); }

		[NotNull]
		public IEnumerable<EnumMember> GetMembers(EnumMemberSelection selection) { return SelectEnumMembers(_cache.GetMembers(selection)); }

		[NotNull]
		public IEnumerable<string> GetNames(EnumMemberSelection selection) { return _cache.GetNames(selection); }

		[NotNull]
		public IEnumerable<string> GetDisplayNames(EnumMemberSelection selection) { return _cache.GetDisplayNames(selection); }

		[NotNull]
		public Enum[] GetValues(EnumMemberSelection selection) { return SelectEnumValues(_cache.GetValues(selection)); }

		public Enum ToObject([NotNull] object value, EnumValidation validation = EnumValidation.None)
		{
			return value is Enum venum
						? validation == EnumValidation.None
							? venum
							: Validate(venum, nameof(value), validation)
						: ToEnum(_cache.ToObject(value, validation));
		}

		public Enum ToObject(long value, EnumValidation validation) { return ToEnum(_cache.ToObject(value, validation)); }
		
		public Enum ToObject(ulong value, EnumValidation validation) { return ToEnum(_cache.ToObject(value, validation)); }

		public bool TryToObject(object value, out Enum result, EnumValidation validation)
		{
			if (value is Enum v)
			{
				result = v;
				return IsValid(result, validation);
			}

			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool TryToObject(long value, out Enum result, EnumValidation validation)
		{
			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool TryToObject(ulong value, out Enum result, EnumValidation validation)
		{
			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool IsValid(object value, EnumValidation validation)
		{
			return validation == EnumValidation.Default
						? _customEnumValidator?.IsValid(value) ?? _cache.IsValidSimple(ToInt(value))
						: _cache.IsValid(ToInt(value), validation);
		}

		public bool IsDefined(Enum value) { return _cache.IsDefined(ToInt(value)); }

		public Enum Validate(object value, string paramName, EnumValidation validation)
		{
			_cache.Validate(ToInt(value), paramName, validation);
			return (Enum)value;
		}

		public string AsString(Enum value) { return _cache.AsString(ToInt(value)); }

		public string AsString(Enum value, string format) { return _cache.AsString(ToInt(value), format); }

		public string AsString(Enum value, EnumFormat[] formats) { return _cache.AsString(ToInt(value), formats); }

		public string Format(Enum value, [NotNull] string format) { return _cache.Format(ToInt(value), format); }

		public string AsString(Enum value, EnumFormat format) { return _cache.AsString(ToInt(value), format); }

		public string AsString(Enum value, EnumFormat format0, EnumFormat format1) { return _cache.AsString(ToInt(value), format0, format1); }

		public string AsString(Enum value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return _cache.AsString(ToInt(value), format0, format1, format2); }

		public string Format(Enum value, [NotNull] EnumFormat[] formats) { return _cache.Format(ToInt(value), formats); }

		[NotNull]
		public object GetUnderlyingValue(Enum value) { return ToInt(value); }

		public sbyte ToSByte(Enum value) { return ToInt(value).ToSByte(null); }

		public byte ToByte(Enum value) { return ToInt(value).ToByte(null); }

		public short ToInt16(Enum value) { return ToInt(value).ToInt16(null); }

		public ushort ToUInt16(Enum value) { return ToInt(value).ToUInt16(null); }

		public int ToInt32(Enum value) { return ToInt(value).ToInt32(null); }

		public uint ToUInt32(Enum value) { return ToInt(value).ToUInt32(null); }

		public long ToInt64(Enum value) { return ToInt(value).ToInt64(null); }

		public ulong ToUInt64(Enum value) { return ToInt(value).ToUInt64(null); }

		public int GetHashCode(Enum value) { return ToInt(value).GetHashCode(); }

		public bool Equals(Enum value, Enum other) { return ReferenceEquals(value, other) || ToInt(value).Equals(ToInt(other)); }

		public int Compare(Enum value, Enum other) { return ToInt(value).CompareTo(ToInt(other)); }

		public string GetName(Enum value) { return _cache.GetMember(ToInt(value))?.Name; }

		public AttributeCollection GetAttributes(Enum value) { return _cache.GetMember(ToInt(value))?.Attributes; }

		public EnumMember GetMember(Enum value) { return _cache.GetMember(ToInt(value))?.EnumMemberInternal; }

		public EnumMember GetMember(string value, bool ignoreCase, EnumFormat[] formats)
		{
			return _cache.GetMember(value, ignoreCase, formats)?.EnumMemberInternal;
		}

		public Enum Parse(string value, bool ignoreCase, EnumFormat[] formats) { return ToEnum(_cache.Parse(value, ignoreCase, formats)); }

		public bool TryParse(string value, bool ignoreCase, out Enum result, EnumFormat[] formats)
		{
			bool success = _cache.TryParse(value, ignoreCase, out TInt resultAsInt, formats);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool IsValidFlagCombination(Enum value) { return _cache.IsValidFlagCombination(ToInt(value)); }

		public string FormatFlags(Enum value, string delimiter, EnumFormat[] formats) { return _cache.FormatFlags(ToInt(value), delimiter, formats); }

		[NotNull]
		public IEnumerable<Enum> GetFlags(Enum value) { return SelectEnumValues(_cache.GetFlags(ToInt(value))); }

		public int GetFlagCount() { return _cache.GetFlagCount(); }

		public int GetFlagCount(Enum value) { return _cache.GetFlagCount(ToInt(value)); }

		public int GetFlagCount(Enum value, Enum otherFlags) { return _cache.GetFlagCount(ToInt(value), ToInt(otherFlags)); }

		[NotNull]
		public IEnumerable<EnumMember> GetFlagMembers(Enum value) { return SelectEnumMembers(_cache.GetFlagMembers(ToInt(value))); }

		public bool HasAnyFlags(Enum value) { return _cache.HasAnyFlags(ToInt(value)); }

		public bool HasAnyFlags(Enum value, Enum otherFlags) { return _cache.HasAnyFlags(ToInt(value), ToInt(otherFlags)); }

		public bool HasAllFlags(Enum value) { return _cache.HasAllFlags(ToInt(value)); }

		public bool HasAllFlags(Enum value, Enum otherFlags) { return _cache.HasAllFlags(ToInt(value), ToInt(otherFlags)); }

		public Enum ToggleFlags(Enum value) { return ToEnum(_cache.ToggleFlags(ToInt(value))); }

		public Enum ToggleFlags(Enum value, Enum otherFlags) { return ToEnum(_cache.ToggleFlags(ToInt(value), ToInt(otherFlags))); }

		public Enum CommonFlags(Enum value, Enum otherFlags) { return ToEnum(_cache.CommonFlags(ToInt(value), ToInt(otherFlags))); }

		public Enum CombineFlags(Enum value, Enum otherFlags) { return ToEnum(_cache.CombineFlags(ToInt(value), ToInt(otherFlags))); }

		public Enum CombineFlags(Enum flag0, Enum flag1, Enum flag2) { return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2))); }

		public Enum CombineFlags(Enum flag0, Enum flag1, Enum flag2, Enum flag3)
		{
			return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2), ToInt(flag3)));
		}

		public Enum CombineFlags(Enum flag0, Enum flag1, Enum flag2, Enum flag3, Enum flag4)
		{
			return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2), ToInt(flag3), ToInt(flag4)));
		}

		public Enum CombineFlags(IEnumerable<Enum> flags)
		{
			TInt result = default(TInt);
			
			if (flags != null)
			{
				foreach (Enum flag in flags)
					result = _cache.CombineFlags(result, ToInt(flag));
			}

			return ToEnum(result);
		}

		public Enum RemoveFlags(Enum value, Enum otherFlags) { return ToEnum(_cache.RemoveFlags(ToInt(value), ToInt(otherFlags))); }

		public Enum ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats) { return ToEnum(_cache.ParseFlags(value, ignoreCase, delimiter, formats)); }

		public bool TryParseFlags(string value, bool ignoreCase, string delimiter, out Enum result, EnumFormat[] formats)
		{
			bool success = _cache.TryParseFlags(value, ignoreCase, delimiter, out TInt resultAsInt, formats);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool CustomValidate(TInt value) { return _customEnumValidator.IsValid(ToEnum(value)); }

		[NotNull]
		public EnumMember CreateEnumMember(EnumMember<TInt, TIntProvider> member) { return new UntypedEnumMember<TInt, TIntProvider>(member); }

		EnumMember IEnumInfo.GetMember(string value, bool ignoreCase, EnumFormat[] formats) { return GetMember(value, ignoreCase, formats); }

		[NotNull] IEnumerable<EnumMember> IEnumInfo.GetMembers(EnumMemberSelection selection) { return GetMembers(selection); }

		[NotNull] IEnumerable<Enum> IEnumInfo.GetValues(EnumMemberSelection selection) { return GetValues(selection); }

		Enum IEnumInfo.Parse(string value, bool ignoreCase, EnumFormat[] formats) { return Parse(value, ignoreCase, formats); }

		Enum IEnumInfo.ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats) { return ParseFlags(value, ignoreCase, delimiter, formats); }

		Enum IEnumInfo.ToObject(ulong value, EnumValidation validation) { return ToObject(value, validation); }

		Enum IEnumInfo.ToObject([NotNull] object value, EnumValidation validation) { return ToObject(value, validation); }

		Enum IEnumInfo.ToObject(long value, EnumValidation validation) { return ToObject(value, validation); }

		[NotNull]
		private static IEnumerable<EnumMember> SelectEnumMembers([NotNull] IEnumerable<EnumMember<TInt, TIntProvider>> members)
		{
			return members.Select(member => member.EnumMemberInternal);
		}

		[NotNull]
		private static Enum[] SelectEnumValues([NotNull] IEnumerable<TInt> values) { return values.Select(ToEnum).ToArray(); }

		[SecuritySafeCritical]
		internal static Enum ToEnum(TInt value) { return (Enum)Convert.ChangeType(value, typeof(TInt)); }

		[SecuritySafeCritical]
		private static TInt ToInt(object value) { return (TInt)Convert.ChangeType(value, typeof(TInt)); }

		private static IEnumValidatorAttribute GetCustomEnumValidator([NotNull] Type enumType)
		{
			Type validatorInterface = typeof(IEnumValidatorAttribute);
			return (IEnumValidatorAttribute)enumType.GetCustomAttributes(false)
														.FirstOrDefault(e => Enumerable.Any(e.GetType().GetInterfaces(), ai => ai == validatorInterface));
		}
	}

	internal class EnumInfo<TEnum, TInt, TIntProvider> : IEnumInfo<TEnum>, IEnumInfo, IEnumInfo<TInt, TIntProvider>
		where TEnum : struct, Enum, IComparable
		where TInt : struct, IComparable<TInt>, IComparable, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		private readonly EnumCache<TInt, TIntProvider> _cache;
		private readonly IEnumValidatorAttribute<TEnum> _customEnumValidator;

		public EnumInfo()
		{
			EnumType = typeof(TEnum);
			_cache = new EnumCache<TInt, TIntProvider>(EnumType, this);
			_customEnumValidator = GetCustomEnumValidator();
		}

		public Type EnumType { get; }

		public TypeCode TypeCode => UnderlyingType.AsTypeCode();

		public Type UnderlyingType { get; } = typeof(TInt);

		public bool IsFlagEnum => _cache.IsFlagEnum;

		public TEnum AllFlags => ToEnum(_cache.AllFlags);

		public bool HasCustomValidator => _customEnumValidator != null;

		[NotNull]
		Enum IEnumInfo.AllFlags => AllFlags;

		public int GetCount(EnumMemberSelection selection) { return _cache.GetMemberCount(selection); }

		[NotNull]
		public IEnumerable<EnumMember<TEnum>> GetMembers(EnumMemberSelection selection) { return SelectEnumMembers(_cache.GetMembers(selection)); }

		[NotNull]
		public IEnumerable<string> GetNames(EnumMemberSelection selection) { return _cache.GetNames(selection); }

		[NotNull]
		public IEnumerable<string> GetDisplayNames(EnumMemberSelection selection) { return _cache.GetDisplayNames(selection); }

		[NotNull]
		public TEnum[] GetValues(EnumMemberSelection selection) { return SelectEnumValues(_cache.GetValues(selection)); }

		public TEnum ToObject([NotNull] object value, EnumValidation validation = EnumValidation.None)
		{
			return value is TEnum venum
						? validation == EnumValidation.None
							? venum
							: Validate(venum, nameof(value), validation)
						: ToEnum(_cache.ToObject(value, validation));
		}

		public TEnum ToObject(long value, EnumValidation validation) { return ToEnum(_cache.ToObject(value, validation)); }

		public TEnum ToObject(ulong value, EnumValidation validation) { return ToEnum(_cache.ToObject(value, validation)); }

		public bool TryToObject(object value, out TEnum result, EnumValidation validation)
		{
			if (value is TEnum v)
			{
				result = v;
				return IsValid(result, validation);
			}
			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool TryToObject(long value, out TEnum result, EnumValidation validation)
		{
			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool TryToObject(ulong value, out TEnum result, EnumValidation validation)
		{
			bool success = _cache.TryToObject(value, out TInt resultAsInt, validation);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool IsValid(TEnum value, EnumValidation validation)
		{
			return validation == EnumValidation.Default
						? _customEnumValidator?.IsValid(value) ?? _cache.IsValidSimple(ToInt(value))
						: _cache.IsValid(ToInt(value), validation);
		}

		public bool IsDefined(TEnum value) { return _cache.IsDefined(ToInt(value)); }

		public TEnum Validate(TEnum value, string paramName, EnumValidation validation)
		{
			_cache.Validate(ToInt(value), paramName, validation);
			return value;
		}

		public string AsString(TEnum value) { return _cache.AsString(ToInt(value)); }

		public string AsString(TEnum value, string format) { return _cache.AsString(ToInt(value), format); }

		public string AsString(TEnum value, EnumFormat[] formats) { return _cache.AsString(ToInt(value), formats); }

		public string Format(TEnum value, [NotNull] string format) { return _cache.Format(ToInt(value), format); }

		public string AsString(TEnum value, EnumFormat format) { return _cache.AsString(ToInt(value), format); }

		public string AsString(TEnum value, EnumFormat format0, EnumFormat format1) { return _cache.AsString(ToInt(value), format0, format1); }

		public string AsString(TEnum value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return _cache.AsString(ToInt(value), format0, format1, format2); }

		public string Format(TEnum value, [NotNull] EnumFormat[] formats) { return _cache.Format(ToInt(value), formats); }

		[NotNull]
		public object GetUnderlyingValue(TEnum value) { return ToInt(value); }

		public sbyte ToSByte(TEnum value) { return ToInt(value).ToSByte(null); }

		public byte ToByte(TEnum value) { return ToInt(value).ToByte(null); }

		public short ToInt16(TEnum value) { return ToInt(value).ToInt16(null); }

		public ushort ToUInt16(TEnum value) { return ToInt(value).ToUInt16(null); }

		public int ToInt32(TEnum value) { return ToInt(value).ToInt32(null); }

		public uint ToUInt32(TEnum value) { return ToInt(value).ToUInt32(null); }

		public long ToInt64(TEnum value) { return ToInt(value).ToInt64(null); }

		public ulong ToUInt64(TEnum value) { return ToInt(value).ToUInt64(null); }

		public int GetHashCode(TEnum value) { return ToInt(value).GetHashCode(); }

		public bool Equals(TEnum value, TEnum other) { return ToInt(value).Equals(ToInt(other)); }

		public int Compare(TEnum value, TEnum other) { return ToInt(value).CompareTo(ToInt(other)); }

		public string GetName(TEnum value) { return _cache.GetMember(ToInt(value))?.Name; }

		public AttributeCollection GetAttributes(TEnum value) { return _cache.GetMember(ToInt(value))?.Attributes; }

		public EnumMember<TEnum> GetMember(TEnum value) { return (EnumMember<TEnum>)_cache.GetMember(ToInt(value))?.EnumMemberInternal; }

		public EnumMember<TEnum> GetMember(string value, bool ignoreCase, EnumFormat[] formats)
		{
			return (EnumMember<TEnum>)_cache.GetMember(value, ignoreCase, formats)?.EnumMemberInternal;
		}

		public TEnum Parse(string value, bool ignoreCase, EnumFormat[] formats) { return ToEnum(_cache.Parse(value, ignoreCase, formats)); }

		public bool TryParse(string value, bool ignoreCase, out TEnum result, EnumFormat[] formats)
		{
			bool success = _cache.TryParse(value, ignoreCase, out TInt resultAsInt, formats);
			result = ToEnum(resultAsInt);
			return success;
		}

		public bool IsValidFlagCombination(TEnum value) { return _cache.IsValidFlagCombination(ToInt(value)); }

		public string FormatFlags(TEnum value, string delimiter, EnumFormat[] formats) { return _cache.FormatFlags(ToInt(value), delimiter, formats); }

		[NotNull]
		public IEnumerable<TEnum> GetFlags(TEnum value) { return SelectEnumValues(_cache.GetFlags(ToInt(value))); }

		public int GetFlagCount() { return _cache.GetFlagCount(); }

		public int GetFlagCount(TEnum value) { return _cache.GetFlagCount(ToInt(value)); }

		public int GetFlagCount(TEnum value, TEnum otherFlags) { return _cache.GetFlagCount(ToInt(value), ToInt(otherFlags)); }

		[NotNull]
		public IEnumerable<EnumMember<TEnum>> GetFlagMembers(TEnum value) { return SelectEnumMembers(_cache.GetFlagMembers(ToInt(value))); }

		public bool HasAnyFlags(TEnum value) { return _cache.HasAnyFlags(ToInt(value)); }

		public bool HasAnyFlags(TEnum value, TEnum otherFlags) { return _cache.HasAnyFlags(ToInt(value), ToInt(otherFlags)); }

		public bool HasAllFlags(TEnum value) { return _cache.HasAllFlags(ToInt(value)); }

		public bool HasAllFlags(TEnum value, TEnum otherFlags) { return _cache.HasAllFlags(ToInt(value), ToInt(otherFlags)); }

		public TEnum ToggleFlags(TEnum value) { return ToEnum(_cache.ToggleFlags(ToInt(value))); }

		public TEnum ToggleFlags(TEnum value, TEnum otherFlags) { return ToEnum(_cache.ToggleFlags(ToInt(value), ToInt(otherFlags))); }

		public TEnum CommonFlags(TEnum value, TEnum otherFlags) { return ToEnum(_cache.CommonFlags(ToInt(value), ToInt(otherFlags))); }

		public TEnum CombineFlags(TEnum value, TEnum otherFlags) { return ToEnum(_cache.CombineFlags(ToInt(value), ToInt(otherFlags))); }

		public TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2) { return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2))); }

		public TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3)
		{
			return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2), ToInt(flag3)));
		}

		public TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3, TEnum flag4)
		{
			return ToEnum(_cache.CombineFlags(ToInt(flag0), ToInt(flag1), ToInt(flag2), ToInt(flag3), ToInt(flag4)));
		}

		public TEnum CombineFlags(IEnumerable<TEnum> flags)
		{
			TInt result = default(TInt);
			
			if (flags != null)
			{
				foreach (TEnum flag in flags) 
					result = _cache.CombineFlags(result, ToInt(flag));
			}

			return ToEnum(result);
		}

		public TEnum RemoveFlags(TEnum value, TEnum otherFlags) { return ToEnum(_cache.RemoveFlags(ToInt(value), ToInt(otherFlags))); }

		public TEnum ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats) { return ToEnum(_cache.ParseFlags(value, ignoreCase, delimiter, formats)); }

		public bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result, EnumFormat[] formats)
		{
			bool success = _cache.TryParseFlags(value, ignoreCase, delimiter, out TInt resultAsInt, formats);
			result = ToEnum(resultAsInt);
			return success;
		}

		string IEnumInfo.AsString([NotNull] Enum value) { return AsString(ToObject(value)); }

		string IEnumInfo.AsString([NotNull] Enum value, string format) { return AsString(ToObject(value), format); }

		string IEnumInfo.AsString([NotNull] Enum value, EnumFormat format) { return AsString(ToObject(value), format); }

		string IEnumInfo.AsString([NotNull] Enum value, EnumFormat format0, EnumFormat format1) { return AsString(ToObject(value), format0, format1); }

		string IEnumInfo.AsString([NotNull] Enum value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return AsString(ToObject(value), format0, format1, format2); }

		string IEnumInfo.AsString([NotNull] Enum value, EnumFormat[] formats) { return AsString(ToObject(value), formats); }

		[NotNull] Enum IEnumInfo.RemoveFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return RemoveFlags(ToObject(value), ToObject(otherFlags)); }

		[NotNull] Enum IEnumInfo.CommonFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return CommonFlags(ToObject(value), ToObject(otherFlags)); }

		int IEnumInfo.Compare([NotNull] Enum value, [NotNull] Enum other) { return Compare(ToObject(value), ToObject(other)); }

		bool IEnumInfo.Equals([NotNull] Enum value, [NotNull] Enum other) { return Equals(ToObject(value), ToObject(other)); }

		string IEnumInfo.Format([NotNull] Enum value, [NotNull] string format) { return Format(ToObject(value), format); }

		string IEnumInfo.Format([NotNull] Enum value, [NotNull] EnumFormat[] formats) { return Format(ToObject(value), formats); }

		string IEnumInfo.FormatFlags([NotNull] Enum value, string delimiter, EnumFormat[] formats) { return FormatFlags(ToObject(value), delimiter, formats); }

		AttributeCollection IEnumInfo.GetAttributes([NotNull] Enum value) { return GetAttributes(ToObject(value)); }

		EnumMember IEnumInfo.GetMember([NotNull] Enum value) { return GetMember(ToObject(value)); }

		[NotNull] IEnumerable<Enum> IEnumInfo.GetFlags([NotNull] Enum value) { return SelectEnumObjects(GetFlags(ToObject(value))).Cast<Enum>(); }

		[NotNull] IEnumerable<EnumMember> IEnumInfo.GetFlagMembers([NotNull] Enum value) { return GetFlagMembers(ToObject(value)); }

		int IEnumInfo.GetFlagCount([NotNull] Enum value) { return GetFlagCount(ToObject(value)); }

		int IEnumInfo.GetFlagCount([NotNull] Enum value, [NotNull] Enum otherFlags) { return GetFlagCount(ToObject(value), ToObject(otherFlags)); }

		string IEnumInfo.GetName([NotNull] Enum value) { return GetName(ToObject(value)); }

		[NotNull] object IEnumInfo.GetUnderlyingValue([NotNull] Enum value) { return GetUnderlyingValue(ToObject(value)); }

		bool IEnumInfo.HasAllFlags([NotNull] Enum value) { return HasAllFlags(ToObject(value)); }

		bool IEnumInfo.HasAllFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return HasAllFlags(ToObject(value), ToObject(otherFlags)); }

		bool IEnumInfo.HasAnyFlags([NotNull] Enum value) { return HasAnyFlags(ToObject(value)); }

		bool IEnumInfo.HasAnyFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return HasAnyFlags(ToObject(value), ToObject(otherFlags)); }

		bool IEnumInfo.IsDefined([NotNull] Enum value) { return IsDefined(ToObject(value)); }

		public bool IsValid([NotNull] object value, EnumValidation validation) { return IsValid(ToObject(value), validation); }

		bool IEnumInfo.IsValidFlagCombination([NotNull] Enum value) { return IsValidFlagCombination(ToObject(value)); }

		[NotNull] Enum IEnumInfo.CombineFlags(IEnumerable<Enum> flags) { return CombineFlags(flags?.Select(flag => ToObject(flag))); }

		[NotNull] Enum IEnumInfo.CombineFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return CombineFlags(ToObject(value), ToObject(otherFlags)); }

		byte IEnumInfo.ToByte([NotNull] Enum value) { return ToByte(ToObject(value)); }

		[NotNull] Enum IEnumInfo.ToggleFlags([NotNull] Enum value) { return ToggleFlags(ToObject(value)); }

		[NotNull] Enum IEnumInfo.ToggleFlags([NotNull] Enum value, [NotNull] Enum otherFlags) { return ToggleFlags(ToObject(value), ToObject(otherFlags)); }

		short IEnumInfo.ToInt16([NotNull] Enum value) { return ToInt16(ToObject(value)); }

		int IEnumInfo.ToInt32([NotNull] Enum value) { return ToInt32(ToObject(value)); }

		long IEnumInfo.ToInt64([NotNull] Enum value) { return ToInt64(ToObject(value)); }

		sbyte IEnumInfo.ToSByte([NotNull] Enum value) { return ToSByte(ToObject(value)); }

		ushort IEnumInfo.ToUInt16([NotNull] Enum value) { return ToUInt16(ToObject(value)); }

		uint IEnumInfo.ToUInt32([NotNull] Enum value) { return ToUInt32(ToObject(value)); }

		ulong IEnumInfo.ToUInt64([NotNull] Enum value) { return ToUInt64(ToObject(value)); }

		bool IEnumInfo.TryParse(string value, bool ignoreCase, [NotNull] out Enum result, EnumFormat[] formats)
		{
			bool success = TryParse(value, ignoreCase, out TEnum resultAsTEnum, formats);
			result = resultAsTEnum;
			return success;
		}

		bool IEnumInfo.TryParseFlags(string value, bool ignoreCase, string delimiter, [NotNull] out Enum result, EnumFormat[] formats)
		{
			bool success = TryParseFlags(value, ignoreCase, delimiter, out TEnum resultAsTEnum, formats);
			result = resultAsTEnum;
			return success;
		}

		bool IEnumInfo.TryToObject(ulong value, [NotNull] out Enum result, EnumValidation validation)
		{
			bool success = TryToObject(value, out TEnum resultAsTEnum, validation);
			result = resultAsTEnum;
			return success;
		}

		bool IEnumInfo.TryToObject(object value, [NotNull] out Enum result, EnumValidation validation)
		{
			bool success = TryToObject(value, out TEnum resultAsTEnum, validation);
			result = resultAsTEnum;
			return success;
		}

		bool IEnumInfo.TryToObject(long value, [NotNull] out Enum result, EnumValidation validation)
		{
			bool success = TryToObject(value, out TEnum resultAsTEnum, validation);
			result = resultAsTEnum;
			return success;
		}

		[NotNull] Enum IEnumInfo.Validate([NotNull] object value, string paramName, EnumValidation validation) { return Validate(ToObject(value), paramName, validation); }

		public bool CustomValidate(TInt value) { return _customEnumValidator.IsValid(ToEnum(value)); }

		[NotNull]
		public EnumMember CreateEnumMember(EnumMember<TInt, TIntProvider> member) { return new EnumMember<TEnum, TInt, TIntProvider>(member); }

		EnumMember IEnumInfo.GetMember(string value, bool ignoreCase, EnumFormat[] formats) { return GetMember(value, ignoreCase, formats); }

		[NotNull] IEnumerable<EnumMember> IEnumInfo.GetMembers(EnumMemberSelection selection) { return GetMembers(selection); }

		[NotNull] IEnumerable<Enum> IEnumInfo.GetValues(EnumMemberSelection selection) { return SelectEnumObjects(GetValues(selection)).Cast<Enum>(); }

		[NotNull] Enum IEnumInfo.Parse(string value, bool ignoreCase, EnumFormat[] formats) { return Parse(value, ignoreCase, formats); }

		[NotNull] Enum IEnumInfo.ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats) { return ParseFlags(value, ignoreCase, delimiter, formats); }

		[NotNull] Enum IEnumInfo.ToObject(ulong value, EnumValidation validation) { return ToObject(value, validation); }

		[NotNull] Enum IEnumInfo.ToObject([NotNull] object value, EnumValidation validation) { return ToObject(value, validation); }

		[NotNull] Enum IEnumInfo.ToObject(long value, EnumValidation validation) { return ToObject(value, validation); }

		[NotNull]
		private static IEnumerable<EnumMember<TEnum>> SelectEnumMembers([NotNull] IEnumerable<EnumMember<TInt, TIntProvider>> members)
		{
			return members.Select(member => (EnumMember<TEnum>)member.EnumMemberInternal);
		}

		[NotNull]
		private static TEnum[] SelectEnumValues([NotNull] IEnumerable<TInt> values) { return values.Select(ToEnum).ToArray(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[SecuritySafeCritical]
		internal static TEnum ToEnum(TInt value) { return (TEnum)Convert.ChangeType(value, typeof(TInt)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[SecuritySafeCritical]
		private static TInt ToInt(TEnum value) { return (TInt)Convert.ChangeType(value, typeof(TInt)); }

		[NotNull]
		private static IEnumerable<TEnum> SelectEnumObjects([NotNull] IEnumerable<TEnum> values) { return values.Select(value => value); }

		private static IEnumValidatorAttribute<TEnum> GetCustomEnumValidator()
		{
			Type type = typeof(TEnum);
			Type validatorInterface = typeof(IEnumValidatorAttribute<>).MakeGenericType(type);
			return (IEnumValidatorAttribute<TEnum>)type.GetCustomAttributes(false)
														.FirstOrDefault(e => Enumerable.Any(e.GetType().GetInterfaces(), ai => ai == validatorInterface));
		}
	}

	internal struct EnumInfo
	{
		public readonly IEnumInfo Value;
		public readonly bool IsNullable;

		public EnumInfo(IEnumInfo value, bool isNullable)
		{
			Value = value;
			IsNullable = isNullable;
		}
	}
}