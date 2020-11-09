using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using asm.Exceptions;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;
using Other.TylerBrinkley.Enumeration.Numeric;
using SysMath = System.Math;

// ReSharper disable once CheckNamespace
namespace Other.TylerBrinkley.Enumeration
{
	internal sealed class EnumCache<TInt, TIntProvider>
		where TInt : struct, IComparable, IComparable<TInt>, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		internal sealed class EnumMemberParser
		{
			private readonly Dictionary<string, EnumMember<TInt, TIntProvider>> _formatValueMap;
			private Dictionary<string, EnumMember<TInt, TIntProvider>> _formatIgnoreCase;

			public EnumMemberParser(EnumFormat format, [NotNull] EnumCache<TInt, TIntProvider> enumCache)
			{
				_formatValueMap =
					new Dictionary<string, EnumMember<TInt, TIntProvider>>(enumCache.GetMemberCount(EnumMemberSelection.All),
																					StringComparer.Ordinal);
				foreach (EnumMember<TInt, TIntProvider> member in enumCache.GetMembers(EnumMemberSelection.All))
				{
					string formattedValue = member.AsString(format);
					if (formattedValue != null) _formatValueMap[formattedValue] = member;
				}
			}

			[NotNull]
			private Dictionary<string, EnumMember<TInt, TIntProvider>> FormatIgnoreCase
			{
				get
				{
					Dictionary<string, EnumMember<TInt, TIntProvider>> formatIgnoreCase = _formatIgnoreCase;
					if (formatIgnoreCase != null) return formatIgnoreCase;
					formatIgnoreCase = new Dictionary<string, EnumMember<TInt, TIntProvider>>(_formatValueMap.Count, StringComparer.OrdinalIgnoreCase);
					
					foreach (KeyValuePair<string, EnumMember<TInt, TIntProvider>> pair in _formatValueMap) 
						formatIgnoreCase[pair.Key] = pair.Value;

					_formatIgnoreCase = formatIgnoreCase;
					return formatIgnoreCase;
				}
			}

			internal bool TryParse([NotNull] string formattedValue, bool ignoreCase, out EnumMember<TInt, TIntProvider> result)
			{
				return _formatValueMap.TryGetValue(formattedValue, out result) || ignoreCase && FormatIgnoreCase.TryGetValue(formattedValue, out result);
			}
		}

		internal static readonly TIntProvider Provider = new TIntProvider();

		internal readonly TInt AllFlags;

		internal readonly bool IsFlagEnum;

		internal readonly IEnumInfo<TInt, TIntProvider> EnumInfo;

		private readonly bool _isContiguous;

		private readonly bool _hasCustomValidator;

		private readonly string _enumTypeName;

		private readonly TInt _maxDefined;

		private readonly TInt _minDefined;

		private readonly Dictionary<TInt, EnumMember<TInt, TIntProvider>> _valueMap;

		private readonly List<EnumMember<TInt, TIntProvider>> _duplicateValues;

		private EnumMemberParser[] _enumMemberParsers;

		public EnumCache([NotNull] Type enumType, [NotNull] IEnumInfo<TInt, TIntProvider> enumInfo)
		{
			if (!enumType.IsEnum) throw new NotEnumTypeException(enumType);
			_enumTypeName = enumType.Name;
			EnumInfo = enumInfo;
			_hasCustomValidator = enumInfo.HasCustomValidator;

			IsFlagEnum = enumType.IsDefined(typeof(FlagsAttribute), false);

			FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
			_valueMap = new Dictionary<TInt, EnumMember<TInt, TIntProvider>>(fields.Length);
			if (fields.Length == 0) return;
			List<EnumMember<TInt, TIntProvider>> duplicateValues = null;

			// This is necessary due to a .NET reflection bug with retrieving Boolean Enums values
			Dictionary<string, TInt> fieldDictionary = null;
			bool isBoolean = typeof(TInt) == typeof(bool);

			if (isBoolean)
			{
				fieldDictionary = new Dictionary<string, TInt>();
				TInt[] values = (TInt[])Enum.GetValues(enumType);
				string[] names = Enum.GetNames(enumType);
				
				for (int i = 0; i < names.Length; ++i) 
					fieldDictionary.Add(names[i], values[i]);
			}

			foreach (FieldInfo field in fields)
			{
				string name = field.Name;
				TInt value = isBoolean
								? fieldDictionary[name]
								: (TInt)field.GetValue(null);
				Collections.AttributeCollection attributes = new Collections.AttributeCollection(Attribute.GetCustomAttributes(field, false));
				EnumMember<TInt, TIntProvider> member = new EnumMember<TInt, TIntProvider>(value, name, attributes, this);

				if (_valueMap.TryGetValue(value, out EnumMember<TInt, TIntProvider> existing))
				{
					if (attributes.Has<PrimaryEnumMemberAttribute>())
					{
						_valueMap[value] = member;
						member = existing;
					}

					(duplicateValues ??= new List<EnumMember<TInt, TIntProvider>>()).Add(member);
				}
				else
				{
					_valueMap.Add(value, member);
					// Is Power of Two
					if (Provider.BitCount(value) == 1) AllFlags = Provider.Or(AllFlags, value);
				}
			}

			bool isInOrder = true;
			TInt previous = default(TInt);
			bool isFirst = true;

			foreach (TInt key in _valueMap.Select(pair => pair.Key))
			{
				if (isFirst)
				{
					_minDefined = key;
					isFirst = false;
				}
				else if (previous.IsGreaterThan(key))
				{
					isInOrder = false;
					break;
				}

				previous = key;
			}

			if (isInOrder)
			{
				_maxDefined = previous;
			}
			else
			{
				// Makes sure is in increasing value order, due to no removals
				KeyValuePair<TInt, EnumMember<TInt, TIntProvider>>[] values = _valueMap.ToArray();
				Array.Sort(values, (first, second) => first.Key.CompareTo(second.Key));
				_valueMap = new Dictionary<TInt, EnumMember<TInt, TIntProvider>>(_valueMap.Count);

				foreach (KeyValuePair<TInt, EnumMember<TInt, TIntProvider>> pair in values) _valueMap.Add(pair.Key, pair.Value);

				_maxDefined = values[values.Length - 1].Key;
				_minDefined = values[0].Key;
			}

			_isContiguous = Provider.Subtract(_maxDefined, Provider.Create(_valueMap.Count - 1)).Equals(_minDefined);

			if (duplicateValues == null) return;
			duplicateValues.TrimExcess();
			// Makes sure is in increasing order
			duplicateValues.Sort((first, second) => first.Value.CompareTo(second.Value));
			_duplicateValues = duplicateValues;
			_duplicateValues.Capacity = _duplicateValues.Count;
		}

		public int GetMemberCount(EnumMemberSelection selection)
		{
			switch (selection)
			{
				case EnumMemberSelection.All:
				case EnumMemberSelection.DisplayOrder:
					return _valueMap.Count + (_duplicateValues?.Count ?? 0);
				default:
					EnumHelper<EnumMemberSelection>.Validate(selection, nameof(selection));
					if (EnumHelper<EnumMemberSelection>.HasAnyFlags(selection, EnumMemberSelection.Flags)) return GetFlagCount();
					return EnumHelper<EnumMemberSelection>.HasAnyFlags(selection, EnumMemberSelection.Distinct)
								? _valueMap.Count
								: 0;
			}
		}

		public IEnumerable<EnumMember<TInt, TIntProvider>> GetMembers(EnumMemberSelection selection)
		{
			IEnumerable<EnumMember<TInt, TIntProvider>> members;

			switch (selection)
			{
				case EnumMemberSelection.All:
				case EnumMemberSelection.DisplayOrder:
					members = _duplicateValues == null
								? _valueMap.Values
								: GetMembersInternal();
					break;
				default:
					EnumHelper<EnumMemberSelection>.Validate(selection, nameof(selection));

					if (EnumHelper<EnumMemberSelection>.HasAnyFlags(selection, EnumMemberSelection.Flags)) members = GetFlagMembers(AllFlags);
					else if (EnumHelper<EnumMemberSelection>.HasAnyFlags(selection, EnumMemberSelection.Distinct)) members = _valueMap.Values;
					else return null;

					break;
			}

			return EnumHelper<EnumMemberSelection>.HasAnyFlags(selection, EnumMemberSelection.DisplayOrder)
						? members.OrderBy(member => member.Attributes.Get<DisplayAttribute>()?.GetOrder() ?? int.MaxValue)
						: members;
		}

		[NotNull]
		public IEnumerable<string> GetNames(EnumMemberSelection selection) { return GetMembers(selection).Select(member => member.Name); }

		[NotNull]
		public IEnumerable<string> GetDisplayNames(EnumMemberSelection selection) { return GetMembers(selection).Select(member => member.DisplayName); }

		[NotNull]
		public IEnumerable<TInt> GetValues(EnumMemberSelection selection) { return GetMembers(selection).Select(member => member.Value); }

		public TInt ToObject([NotNull] object value, EnumValidation validation)
		{
			if (value is TInt result1)
			{
				Validate(result1, nameof(value), validation);
				return result1;
			}

			Type type = value.GetType();

			switch ((Nullable.GetUnderlyingType(type) ?? type).AsTypeCode())
			{
				case TypeCode.SByte:
					return ToObject((sbyte)value, validation);
				case TypeCode.Byte:
					return ToObject((byte)value, validation);
				case TypeCode.Int16:
					return ToObject((short)value, validation);
				case TypeCode.UInt16:
					return ToObject((ushort)value, validation);
				case TypeCode.Int32:
					return ToObject((int)value, validation);
				case TypeCode.UInt32:
					return ToObject((uint)value, validation);
				case TypeCode.Int64:
					return ToObject((long)value, validation);
				case TypeCode.UInt64:
					return ToObject((ulong)value, validation);
				case TypeCode.String:
					TInt result = Parse((string)value, false, null);
					Validate(result, nameof(value), validation);
					return result;
				case TypeCode.Boolean:
					return ToObject(Convert.ToByte((bool)value), validation);
				case TypeCode.Char:
					return ToObject((char)value, validation);
			}

			throw new ArgumentException($"value is not type {_enumTypeName}, SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, UInt64, or String.");
		}

		public TInt ToObject(long value, EnumValidation validation)
		{
			if (!Provider.IsInValueRange(value)) throw new OverflowException("value is outside the underlying type's value range");

			TInt result = Provider.Create(value);
			Validate(result, nameof(value), validation);
			return result;
		}

		public TInt ToObject(ulong value, EnumValidation validation)
		{
			if (!Provider.IsInValueRange(value)) throw new OverflowException("value is outside the underlying type's value range");

			TInt result = Provider.Create(value);
			Validate(result, nameof(value), validation);
			return result;
		}

		public bool TryToObject(object value, out TInt result, EnumValidation validation)
		{
			if (value != null)
			{
				if (value is TInt i)
				{
					result = i;
					return IsValid(result, validation);
				}

				Type type = value.GetType();

				switch ((Nullable.GetUnderlyingType(type) ?? type).AsTypeCode())
				{
					case TypeCode.SByte:
						return TryToObject((sbyte)value, out result, validation);
					case TypeCode.Byte:
						return TryToObject((byte)value, out result, validation);
					case TypeCode.Int16:
						return TryToObject((short)value, out result, validation);
					case TypeCode.UInt16:
						return TryToObject((ushort)value, out result, validation);
					case TypeCode.Int32:
						return TryToObject((int)value, out result, validation);
					case TypeCode.UInt32:
						return TryToObject((uint)value, out result, validation);
					case TypeCode.Int64:
						return TryToObject((long)value, out result, validation);
					case TypeCode.UInt64:
						return TryToObject((ulong)value, out result, validation);
					case TypeCode.String:
						if (TryParse((string)value, false, out result, null)) return IsValid(result, validation);
						break;
					case TypeCode.Boolean:
						return TryToObject(Convert.ToByte((bool)value), out result, validation);
					case TypeCode.Char:
						return TryToObject((char)value, out result, validation);
				}
			}

			result = Provider.Zero;
			return false;
		}

		public bool TryToObject(long value, out TInt result, EnumValidation validation)
		{
			if (Provider.IsInValueRange(value))
			{
				result = Provider.Create(value);
				return IsValid(result, validation);
			}

			result = Provider.Zero;
			return false;
		}

		public bool TryToObject(ulong value, out TInt result, EnumValidation validation)
		{
			if (Provider.IsInValueRange(value))
			{
				result = Provider.Create(value);
				return IsValid(result, validation);
			}

			result = Provider.Zero;
			return false;
		}

		public bool IsValid(TInt value, EnumValidation validation)
		{
			switch (validation)
			{
				case EnumValidation.Default:
					return _hasCustomValidator
								? EnumInfo.CustomValidate(value)
								: IsValidSimple(value);
				case EnumValidation.IsDefined:
					return IsDefined(value);
				case EnumValidation.IsValidFlagCombination:
					return IsValidFlagCombination(value);
				case EnumValidation.None:
					return true;
				default:
					EnumHelper<EnumValidation>.Validate(validation, nameof(validation));
					return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidSimple(TInt value) { return IsFlagEnum && IsValidFlagCombination(value) || IsDefined(value); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsDefined(TInt value)
		{
			return _isContiguous
						? !(Provider.LessThan(value, _minDefined) || Provider.LessThan(_maxDefined, value))
						: _valueMap.ContainsKey(value);
		}

		public void Validate(TInt value, string paramName, EnumValidation validation)
		{
			if (!IsValid(value, validation)) throw new ArgumentException($"invalid value of {AsString(value)} for {_enumTypeName}", paramName);
		}

		public string AsString(TInt value) { return AsStringInternal(value, null); }

		public string AsString(TInt value, EnumFormat format)
		{
			bool isInitialized = false;
			EnumMember<TInt, TIntProvider> member = null;
			return FormatInternal(value, ref isInitialized, ref member, format);
		}

		public string AsString(TInt value, EnumFormat format0, EnumFormat format1) { return FormatInternal(value, null, format0, format1); }

		public string AsString(TInt value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return FormatInternal(value, null, format0, format1, format2); }

		public string AsString(TInt value, EnumFormat[] formats) { return AsStringInternal(value, null, formats); }

		public string AsString(TInt value, string format) { return AsStringInternal(value, null, format); }

		public string Format(TInt value, [NotNull] EnumFormat[] formats) { return FormatInternal(value, null, formats); }

		public string Format(TInt value, [NotNull] string format) { return FormatInternal(value, null, format); }

		public EnumMember<TInt, TIntProvider> GetMember(TInt value)
		{
			_valueMap.TryGetValue(value, out EnumMember<TInt, TIntProvider> member);
			return member;
		}

		public EnumMember<TInt, TIntProvider> GetMember(string value, bool ignoreCase, EnumFormat[] formats)
		{
			value = value.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(value));
			if (!(formats?.Length > 0)) formats = EnumHelper.NameFormatArray;
			TryParseInternal(value, ignoreCase, out _, out EnumMember<TInt, TIntProvider> member, formats, false);
			return member;
		}

		public TInt Parse(string value, bool ignoreCase, EnumFormat[] formats)
		{
			if (IsFlagEnum) return ParseFlags(value, ignoreCase, null, formats);
			value = value.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(value));
			if (!(formats?.Length > 0)) formats = EnumHelper.DefaultFormats;
			if (TryParseInternal(value, ignoreCase, out TInt result, out _, formats, true)) return result;
			if (IsNumeric(value)) throw new OverflowException("value is outside the underlying type's value range");
			throw new ArgumentException($"string was not recognized as being a member of {_enumTypeName}", nameof(value));
		}

		public bool TryParse(string value, bool ignoreCase, out TInt result, EnumFormat[] formats)
		{
			if (IsFlagEnum) return TryParseFlags(value, ignoreCase, null, out result, formats);
			value = value.ToNullIfEmpty();

			if (value != null)
			{
				if (!(formats?.Length > 0)) formats = EnumHelper.DefaultFormats;
				return TryParseInternal(value, ignoreCase, out result, out _, formats, true);
			}

			result = Provider.Zero;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidFlagCombination(TInt value) { return Provider.And(AllFlags, value).Equals(value); }

		public string FormatFlags(TInt value, string delimiter, EnumFormat[] formats) { return FormatFlagsInternal(value, null, delimiter, formats); }

		public IEnumerable<TInt> GetFlags(TInt value)
		{
			TInt validValue = Provider.And(value, AllFlags);
			bool isLessThanZero = Provider.LessThan(validValue, Provider.Zero);

			for (TInt currentValue = Provider.One; (isLessThanZero || !Provider.LessThan(validValue, currentValue)) && !currentValue.Equals(Provider.Zero);
				currentValue = Provider.LeftShift(currentValue, 1))
			{
				if (HasAnyFlags(validValue, currentValue))
					yield return currentValue;
			}
		}

		[NotNull]
		public IEnumerable<EnumMember<TInt, TIntProvider>> GetFlagMembers(TInt value) { return GetFlags(value).Select(GetMember); }

		public int GetFlagCount() { return Provider.BitCount(AllFlags); }

		public int GetFlagCount(TInt value) { return Provider.BitCount(Provider.And(value, AllFlags)); }

		public int GetFlagCount(TInt value, TInt otherFlags) { return Provider.BitCount(Provider.And(Provider.And(value, otherFlags), AllFlags)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAnyFlags(TInt value) { return !value.Equals(Provider.Zero); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAnyFlags(TInt value, TInt otherFlags) { return !Provider.And(value, otherFlags).Equals(Provider.Zero); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAllFlags(TInt value) { return HasAllFlags(value, AllFlags); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAllFlags(TInt value, TInt otherFlags) { return Provider.And(value, otherFlags).Equals(otherFlags); }

		public TInt ToggleFlags(TInt value) { return Provider.Xor(value, AllFlags); }

		public TInt ToggleFlags(TInt value, TInt otherFlags) { return Provider.Xor(value, otherFlags); }

		public TInt CommonFlags(TInt value, TInt otherFlags) { return Provider.And(value, otherFlags); }

		public TInt CombineFlags(TInt value, TInt otherFlags) { return Provider.Or(value, otherFlags); }

		public TInt CombineFlags(TInt flag0, TInt flag1, TInt flag2) { return Provider.Or(Provider.Or(flag0, flag1), flag2); }

		public TInt CombineFlags(TInt flag0, TInt flag1, TInt flag2, TInt flag3) { return Provider.Or(Provider.Or(Provider.Or(flag0, flag1), flag2), flag3); }

		public TInt CombineFlags(TInt flag0, TInt flag1, TInt flag2, TInt flag3, TInt flag4)
		{
			return Provider.Or(Provider.Or(Provider.Or(Provider.Or(flag0, flag1), flag2), flag3), flag4);
		}

		public TInt RemoveFlags(TInt value, TInt otherFlags) { return Provider.And(value, Provider.Not(otherFlags)); }

		public TInt ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats)
		{
			value = value.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(value));
			if (string.IsNullOrEmpty(delimiter)) delimiter = EnumHelper.DEFAULT_DELIMITER;

			string effectiveDelimiter = delimiter.Trim();
			if (effectiveDelimiter.Length == 0) effectiveDelimiter = delimiter;

			if (!(formats?.Length > 0)) formats = EnumHelper.DefaultFormats;

			TInt result = Provider.Zero;
			int startIndex = 0;
			int valueLength = value.Length;
			while (startIndex < valueLength)
			{
				while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex]))
					++startIndex;
				int delimiterIndex = value.IndexOf(effectiveDelimiter, startIndex, StringComparison.Ordinal);
				if (delimiterIndex < 0) delimiterIndex = valueLength;
				int newStartIndex = delimiterIndex + effectiveDelimiter.Length;
				while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1]))
					--delimiterIndex;
				string indValue = value.Substring(startIndex, delimiterIndex - startIndex);
				if (TryParseInternal(indValue, ignoreCase, out TInt valueAsTInt, out _, formats, true))
				{
					result = Provider.Or(result, valueAsTInt);
				}
				else
				{
					if (IsNumeric(indValue)) throw new OverflowException("value is outside the underlying type's value range");
					throw new ArgumentException("value is not a valid combination of flag enum values");
				}

				startIndex = newStartIndex;
			}

			return result;
		}

		public bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TInt result, EnumFormat[] formats)
		{
			value = value.ToNullIfEmpty();
			if (value == null)
			{
				result = Provider.Zero;
				return false;
			}

			if (string.IsNullOrEmpty(delimiter)) delimiter = EnumHelper.DEFAULT_DELIMITER;

			string effectiveDelimiter = delimiter.Trim();
			if (effectiveDelimiter.Length == 0) effectiveDelimiter = delimiter;

			if (!(formats?.Length > 0)) formats = EnumHelper.DefaultFormats;

			TInt resultAsInt = Provider.Zero;
			int startIndex = 0;
			int valueLength = value.Length;
			while (startIndex < valueLength)
			{
				while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex])) ++startIndex;
				int delimiterIndex = value.IndexOf(effectiveDelimiter, startIndex, StringComparison.Ordinal);
				if (delimiterIndex < 0) delimiterIndex = valueLength;
				int newStartIndex = delimiterIndex + effectiveDelimiter.Length;
				while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1])) --delimiterIndex;
				string indValue = value.Substring(startIndex, delimiterIndex - startIndex);
				if (!TryParseInternal(indValue, ignoreCase, out TInt valueAsTInt, out _, formats, true))
				{
					result = Provider.Zero;
					return false;
				}

				resultAsInt = Provider.Or(resultAsInt, valueAsTInt);
				startIndex = newStartIndex;
			}

			result = resultAsInt;
			return true;
		}

		internal string AsStringInternal(TInt value, EnumMember<TInt, TIntProvider> member)
		{
			return IsFlagEnum
						? FormatFlagsInternal(value, member, null, null)
						: FormatInternal(value, member, EnumFormat.Name, EnumFormat.UnderlyingValue);
		}

		internal string AsStringInternal(TInt value, EnumMember<TInt, TIntProvider> member, EnumFormat[] formats)
		{
			return formats?.Length > 0
						? FormatInternal(value, member, formats)
						: AsStringInternal(value, member);
		}

		internal string AsStringInternal(TInt value, EnumMember<TInt, TIntProvider> member, string format)
		{
			return string.IsNullOrEmpty(format)
						? AsStringInternal(value, member)
						: FormatInternal(value, member, format);
		}

		internal string FormatInternal(TInt value, EnumMember<TInt, TIntProvider> member, [NotNull] string format)
		{
			switch (format)
			{
				case "G":
				case "g":
					return AsStringInternal(value, member);
				case "F":
				case "f":
					return FormatFlagsInternal(value, member, null, null);
				case "D":
				case "d":
					return value.ToString(CultureInfo.InvariantCulture);
				case "X":
				case "x":
					return Provider.ToHexadecimalString(value);
			}

			throw new FormatException("format string can be only \"G\", \"g\", \"X\", \"x\", \"F\", \"f\", \"D\" or \"d\".");
		}

		internal string FormatInternal(TInt value, ref bool isInitialized, ref EnumMember<TInt, TIntProvider> member, EnumFormat format)
		{
			switch (format)
			{
				case EnumFormat.UnderlyingValue:
					return value.ToString(CultureInfo.InvariantCulture);
				case EnumFormat.DecimalValue:
					return Provider.ToDecimalString(value);
				case EnumFormat.HexadecimalValue:
					return Provider.ToHexadecimalString(value);
			}

			if (!isInitialized)
			{
				member = GetMember(value);
				isInitialized = true;
			}

			switch (format)
			{
				case EnumFormat.Name:
					return member?.Name;
				case EnumFormat.Description:
					return member?.Attributes.Get<DescriptionAttribute>()?.Description;
				case EnumFormat.EnumMemberValue:
					return member?.Attributes.Get<EnumMemberAttribute>()?.Value;
				case EnumFormat.DisplayName:
					return member?.Attributes.Get<DisplayAttribute>()?.GetName();
				default:
					EnumHelper<EnumFormat>.Validate(format, nameof(format));
					return member != null
								? EnumHelper.CustomEnumMemberFormat(member.EnumMemberInternal, format)
								: null;
			}
		}

		internal string FormatInternal(TInt value, EnumMember<TInt, TIntProvider> member, EnumFormat format0, EnumFormat format1)
		{
			bool isInitialized = member != null;
			return FormatInternal(value, ref isInitialized, ref member, format0) ?? FormatInternal(value, ref isInitialized, ref member, format1);
		}

		internal string FormatInternal(TInt value, EnumMember<TInt, TIntProvider> member, EnumFormat format0, EnumFormat format1, EnumFormat format2)
		{
			bool isInitialized = member != null;
			return FormatInternal(value, ref isInitialized, ref member, format0) ?? FormatInternal(value, ref isInitialized, ref member, format1) ??
					FormatInternal(value, ref isInitialized, ref member, format2);
		}

		internal string FormatInternal(TInt value, EnumMember<TInt, TIntProvider> member, [NotNull] EnumFormat[] formats)
		{
			bool isInitialized = member != null;
			foreach (EnumFormat format in formats)
			{
				string formattedValue = FormatInternal(value, ref isInitialized, ref member, format);
				if (formattedValue != null) return formattedValue;
			}

			return null;
		}

		internal string FormatFlagsInternal(TInt value, EnumMember<TInt, TIntProvider> member, string delimiter, EnumFormat[] formats)
		{
			if (!(formats?.Length > 0)) formats = EnumHelper.DefaultFormats;
			member ??= GetMember(value);
			if (member != null || value.Equals(Provider.Zero) || !IsValidFlagCombination(value)) return FormatInternal(value, member, formats);
			if (string.IsNullOrEmpty(delimiter)) delimiter = EnumHelper.DEFAULT_DELIMITER;
			return string.Join(delimiter, GetFlags(value).Select(flag => FormatInternal(flag, null, formats)));
		}

		[NotNull]
		private EnumMemberParser GetEnumMemberParser(EnumFormat format)
		{
			int index = format - EnumFormat.Name;
			EnumMemberParser[] parsers = _enumMemberParsers;
			EnumMemberParser parser;

			if (index >= 0 && parsers != null && index < parsers.Length && (parser = parsers[index]) != null) return parser;
			EnumHelper<EnumFormat>.Validate(format, nameof(format));

			parser = new EnumMemberParser(format, this);
			EnumMemberParser[] oldParsers;

			do
			{
				oldParsers = parsers;
				parsers = new EnumMemberParser[SysMath.Max(oldParsers?.Length ?? 0, index + 1)];
				oldParsers?.CopyTo(parsers, 0);
				parsers[index] = parser;
			} while ((parsers = Interlocked.CompareExchange(ref _enumMemberParsers, parsers, oldParsers)) != oldParsers);

			return parser;
		}

		private IEnumerable<EnumMember<TInt, TIntProvider>> GetMembersInternal()
		{
			using (Dictionary<TInt, EnumMember<TInt, TIntProvider>>.Enumerator primaryEnumerator = _valueMap.GetEnumerator())
			{
				bool primaryIsActive = primaryEnumerator.MoveNext();
				EnumMember<TInt, TIntProvider> primaryMember = primaryEnumerator.Current.Value;
				using (List<EnumMember<TInt, TIntProvider>>.Enumerator duplicateEnumerator = _duplicateValues.GetEnumerator())
				{
					bool duplicateIsActive = duplicateEnumerator.MoveNext();
					EnumMember<TInt, TIntProvider> duplicateMember = duplicateEnumerator.Current;

					while (primaryIsActive || duplicateIsActive)
						if (duplicateIsActive && (!primaryIsActive || duplicateMember != null && Provider.LessThan(duplicateMember.Value, primaryMember.Value)))
						{
							yield return duplicateMember;
							duplicateIsActive = duplicateEnumerator.MoveNext();
							if (duplicateIsActive) duplicateMember = duplicateEnumerator.Current;
						}
						else
						{
							yield return primaryMember;
							primaryIsActive = primaryEnumerator.MoveNext();
							if (primaryIsActive) primaryMember = primaryEnumerator.Current.Value;
						}
				}
			}
		}

		private bool TryParseInternal(string value, bool ignoreCase, out TInt result, out EnumMember<TInt, TIntProvider> member, [NotNull] EnumFormat[] formats, bool getValueOnly)
		{
			foreach (EnumFormat format in formats)
			{
				switch (format)
				{
					case EnumFormat.UnderlyingValue:
					{
						if (Provider.TryParseNative(value, out result))
						{
							member = getValueOnly
										? null
										: GetMember(result);
							return true;
						}

						break;
					}
					case EnumFormat.DecimalValue:
					case EnumFormat.HexadecimalValue:
					{
						if (Provider.TryParseNumber(value, format == EnumFormat.DecimalValue
																? NumberStyles.AllowLeadingSign
																: NumberStyles.AllowHexSpecifier,
													CultureInfo.InvariantCulture, out result))
						{
							member = getValueOnly
										? null
										: GetMember(result);
							return true;
						}

						break;
					}
					default:
					{
						EnumMemberParser parser = GetEnumMemberParser(format);
						if (!parser.TryParse(value, ignoreCase, out member) || member == null) continue;
						result = member.Value;
						return true;
					}
				}
			}

			result = default(TInt);
			member = null;
			return false;
		}

		private static bool IsNumeric([NotNull] string value)
		{
			char firstChar;
			return value.Length > 0 && (char.IsDigit(firstChar = value[0]) || firstChar == '-' || firstChar == '+');
		}
	}
}