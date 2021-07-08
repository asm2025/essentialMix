// Copyright (c) 2016 Tyler Brinkley
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Other.TylerBrinkley.Collections;
using Other.TylerBrinkley.Enumeration.Numeric;

// ReSharper disable once CheckNamespace
namespace Other.TylerBrinkley.Enumeration
{
	public interface IEnumInfo : IEnumInfoCommon
	{
		Enum AllFlags { get; }

		string AsString(Enum value);
		string AsString(Enum value, string format);
		string AsString(Enum value, EnumFormat format);
		string AsString(Enum value, EnumFormat format0, EnumFormat format1);
		string AsString(Enum value, EnumFormat format0, EnumFormat format1, EnumFormat format2);
		string AsString(Enum value, EnumFormat[] formats);
		Enum RemoveFlags(Enum value, Enum otherFlags);
		Enum CommonFlags(Enum value, Enum otherFlags);
		int Compare(Enum value, Enum other);
		bool Equals(Enum value, Enum other);
		string Format(Enum value, string format);
		string Format(Enum value, EnumFormat[] formats);
		string FormatFlags(Enum value, string delimiter, EnumFormat[] formats);
		AttributeCollection GetAttributes(Enum value);
		EnumMember GetMember(Enum value);
		EnumMember GetMember(string value, bool ignoreCase = false, EnumFormat[] formats = null);
		IEnumerable<EnumMember> GetMembers(EnumMemberSelection selection = EnumMemberSelection.All);
		IEnumerable<Enum> GetFlags(Enum value);
		int GetFlagCount(Enum value);
		int GetFlagCount(Enum value, Enum otherFlags);
		IEnumerable<EnumMember> GetFlagMembers(Enum value);
		string GetName(Enum value);
		object GetUnderlyingValue(Enum value);
		IEnumerable<Enum> GetValues(EnumMemberSelection selection = EnumMemberSelection.All);
		bool HasAllFlags(Enum value);
		bool HasFlag(Enum value, Enum otherFlags);
		bool HasAnyFlags(Enum value);
		bool HasAnyFlag(Enum value, Enum otherFlags);
		bool IsDefined(Enum value);
		bool IsValid(object value, EnumValidation validation = EnumValidation.Default);
		bool IsValidFlagCombination(Enum value);
		Enum Parse(string value, bool ignoreCase, EnumFormat[] formats);
		Enum ParseFlags(string value, bool ignoreCase, string delimiter, EnumFormat[] formats);
		Enum CombineFlags(IEnumerable<Enum> flags);
		Enum CombineFlags(Enum value, Enum otherFlags);
		byte ToByte(Enum value);
		Enum ToggleFlags(Enum value);
		Enum ToggleFlags(Enum value, Enum otherFlags);
		short ToInt16(Enum value);
		int ToInt32(Enum value);
		long ToInt64(Enum value);
		Enum ToObject(ulong value, EnumValidation validation = EnumValidation.None);
		Enum ToObject(object value, EnumValidation validation = EnumValidation.None);
		Enum ToObject(long value, EnumValidation validation = EnumValidation.None);
		sbyte ToSByte(Enum value);
		ushort ToUInt16(Enum value);
		uint ToUInt32(Enum value);
		ulong ToUInt64(Enum value);
		bool TryParse(string value, bool ignoreCase, out Enum result, EnumFormat[] formats);
		bool TryParseFlags(string value, bool ignoreCase, string delimiter, out Enum result, EnumFormat[] formats);
		bool TryToObject(ulong value, out Enum result, EnumValidation validation = EnumValidation.None);
		bool TryToObject(object value, out Enum result, EnumValidation validation = EnumValidation.None);
		bool TryToObject(long value, out Enum result, EnumValidation validation = EnumValidation.None);
		Enum Validate(object value, string paramName, EnumValidation validation = EnumValidation.Default);
	}

	public interface IEnumInfo<TEnum> : IEnumInfoCommon
		where TEnum : struct, Enum, IComparable
	{
		TEnum AllFlags { get; }

		string AsString(TEnum value);
		string AsString(TEnum value, string format);
		string AsString(TEnum value, EnumFormat format);
		string AsString(TEnum value, EnumFormat format0, EnumFormat format1);
		string AsString(TEnum value, EnumFormat format0, EnumFormat format1, EnumFormat format2);
		string AsString(TEnum value, EnumFormat[] formats);
		TEnum RemoveFlags(TEnum value, TEnum otherFlags);
		TEnum CommonFlags(TEnum value, TEnum otherFlags);
		int Compare(TEnum value, TEnum other);
		bool Equals(TEnum value, TEnum other);
		string Format(TEnum value, string format);
		string Format(TEnum value, EnumFormat[] formats);
		string FormatFlags(TEnum value, string delimiter = null, EnumFormat[] formats = null);
		AttributeCollection GetAttributes(TEnum value);
		EnumMember<TEnum> GetMember(TEnum value);
		EnumMember<TEnum> GetMember(string value, bool ignoreCase = false, EnumFormat[] formats = null);
		IEnumerable<EnumMember<TEnum>> GetMembers(EnumMemberSelection selection = EnumMemberSelection.All);
		IEnumerable<TEnum> GetFlags(TEnum value);
		int GetFlagCount(TEnum value);
		int GetFlagCount(TEnum value, TEnum otherFlags);
		IEnumerable<EnumMember<TEnum>> GetFlagMembers(TEnum value);
		int GetHashCode(TEnum value);
		string GetName(TEnum value);
		object GetUnderlyingValue(TEnum value);
		TEnum[] GetValues(EnumMemberSelection selection = EnumMemberSelection.All);
		bool HasAllFlags(TEnum value);
		bool HasFlag(TEnum value, TEnum otherFlags);
		bool HasAnyFlags(TEnum value);
		bool HasAnyFlag(TEnum value, TEnum otherFlags);
		bool IsDefined(TEnum value);
		bool IsValid(object value, EnumValidation validation = EnumValidation.Default);
		bool IsValidFlagCombination(TEnum value);
		TEnum Parse(string value, bool ignoreCase = false, EnumFormat[] formats = null);
		TEnum ParseFlags(string value, bool ignoreCase = false, string delimiter = null, EnumFormat[] formats = null);
		TEnum CombineFlags(IEnumerable<TEnum> flags);
		TEnum CombineFlags(TEnum value, TEnum otherFlags);
		TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2);
		TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3);
		TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3, TEnum flag4);
		byte ToByte(TEnum value);
		TEnum ToggleFlags(TEnum value);
		TEnum ToggleFlags(TEnum value, TEnum otherFlags);
		short ToInt16(TEnum value);
		int ToInt32(TEnum value);
		long ToInt64(TEnum value);
		TEnum ToObject(ulong value, EnumValidation validation = EnumValidation.None);
		TEnum ToObject(object value, EnumValidation validation = EnumValidation.None);
		TEnum ToObject(long value, EnumValidation validation = EnumValidation.None);
		sbyte ToSByte(TEnum value);
		ushort ToUInt16(TEnum value);
		uint ToUInt32(TEnum value);
		ulong ToUInt64(TEnum value);
		bool TryParse(string value, bool ignoreCase, out TEnum result, EnumFormat[] formats = null);
		bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result, EnumFormat[] formats = null);
		bool TryToObject(ulong value, out TEnum result, EnumValidation validation = EnumValidation.None);
		bool TryToObject(object value, out TEnum result, EnumValidation validation = EnumValidation.None);
		bool TryToObject(long value, out TEnum result, EnumValidation validation = EnumValidation.None);
		TEnum Validate(TEnum value, string paramName, EnumValidation validation = EnumValidation.Default);
	}

	internal interface IEnumInfo<TInt, TIntProvider>
		where TInt : struct, IComparable, IComparable<TInt>, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		bool HasCustomValidator { get; }

		EnumMember CreateEnumMember(EnumMember<TInt, TIntProvider> member);
		bool CustomValidate(TInt value);
	}
}