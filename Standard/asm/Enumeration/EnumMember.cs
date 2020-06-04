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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using asm.Numeric;
using AttributeCollection = asm.Collections.AttributeCollection;

namespace asm.Enumeration
{
	/// <summary>
	/// An enum member which composes its name, value, and attributes.
	/// </summary>
	public abstract class EnumMember : IComparable, IComparable<EnumMember>, IEquatable<EnumMember>, IConvertible, IFormattable
	{
		internal readonly IEnumMember Member;

		internal EnumMember(IEnumMember member)
		{
			Member = member;
		}

		/// <summary>
		/// The enum member's value.
		/// </summary>
		public Enum Value => GetValue();

		/// <summary>
		/// The enum member's name.
		/// </summary>
		public string Name => Member.Name;

		/// <summary>
		/// The enum member's display name.
		/// </summary>
		public string DisplayName => Member.DisplayName;

		/// <summary>
		/// The enum member's attributes.
		/// </summary>
		public AttributeCollection Attributes => Member.Attributes;

		/// <summary>
		/// Retrieves the enum member's name.
		/// </summary>
		/// <returns>The enum member's name.</returns>
		public sealed override string ToString() { return Member.Name; }

		/// <summary>
		/// Retrieves the enum member's name.
		/// </summary>
		/// <returns>The enum member's name.</returns>
		public string AsString() { return Member.Name; }

		/// <summary>
		/// Converts the enum member to its string representation using the specified <paramref name="format"/>.
		/// </summary>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="FormatException"><paramref name="format"/> is an invalid value.</exception>
		public string AsString(string format) { return Member.AsString(format); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified <paramref name="format"/>.
		/// </summary>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentException"><paramref name="format"/> is an invalid value.</exception>
		public string AsString(EnumFormat format) { return Member.AsString(format); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified formats.
		/// </summary>
		/// <param name="format0">The first output format to use.</param>
		/// <param name="format1">The second output format to use if using the first resolves to <c>null</c>.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentException"><paramref name="format0"/> or <paramref name="format1"/> is an invalid value.</exception>
		public string AsString(EnumFormat format0, EnumFormat format1) { return Member.AsString(format0, format1); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified formats.
		/// </summary>
		/// <param name="format0">The first output format to use.</param>
		/// <param name="format1">The second output format to use if using the first resolves to <c>null</c>.</param>
		/// <param name="format2">The third output format to use if using the first and second both resolve to <c>null</c>.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentException"><paramref name="format0"/>, <paramref name="format1"/>, or <paramref name="format2"/> is an invalid value.</exception>
		public string AsString(EnumFormat format0, EnumFormat format1, EnumFormat format2) { return Member.AsString(format0, format1, format2); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified <paramref name="formats"/>.
		/// </summary>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats"/> contains an invalid value.</exception>
		public string AsString(params EnumFormat[] formats) { return Member.AsString(formats); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified <paramref name="format"/>.
		/// </summary>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="format"/> is <c>null</c>.</exception>
		/// <exception cref="FormatException"><paramref name="format"/> is an invalid value.</exception>
		public string Format(string format) { return Member.Format(format); }

		/// <summary>
		/// Converts the enum member to its string representation using the specified <paramref name="formats"/>.
		/// </summary>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>A string representation of the enum member.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="formats"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="formats"/> contains an invalid value.</exception>
		public string Format(params EnumFormat[] formats) { return Member.Format(formats); }

		/// <summary>
		/// Retrieves the enum member's underlying integral value.
		/// </summary>
		/// <returns>The enum member's underlying integral value.</returns>
		public object GetUnderlyingValue() { return Member.GetUnderlyingValue(); }

		/// <summary>
		/// Converts <see cref="Value"/> to an <see cref="sbyte"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to an <see cref="sbyte"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="sbyte"/>'s value range without overflowing.</exception>
		public sbyte ToSByte() { return Member.ToSByte(); }

		/// <summary>
		/// Converts <see cref="Value"/> to a <see cref="byte"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to a <see cref="byte"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="byte"/>'s value range without overflowing.</exception>
		public byte ToByte() { return Member.ToByte(); }

		/// <summary>
		/// Converts <see cref="Value"/> to an <see cref="short"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to an <see cref="short"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="short"/>'s value range without overflowing.</exception>
		public short ToInt16() { return Member.ToInt16(); }

		/// <summary>
		/// Converts <see cref="Value"/> to a <see cref="ushort"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to a <see cref="ushort"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="ushort"/>'s value range without overflowing.</exception>
		public ushort ToUInt16() { return Member.ToUInt16(); }

		/// <summary>
		/// Converts <see cref="Value"/> to an <see cref="int"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to an <see cref="int"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="int"/>'s value range without overflowing.</exception>
		public int ToInt32() { return Member.ToInt32(); }

		/// <summary>
		/// Converts <see cref="Value"/> to a <see cref="uint"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to a <see cref="uint"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="uint"/>'s value range without overflowing.</exception>
		public uint ToUInt32() { return Member.ToUInt32(); }

		/// <summary>
		/// Converts <see cref="Value"/> to an <see cref="long"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to an <see cref="long"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="long"/>'s value range without overflowing.</exception>
		public long ToInt64() { return Member.ToInt64(); }

		/// <summary>
		/// Converts <see cref="Value"/> to a <see cref="ulong"/>.
		/// </summary>
		/// <returns><see cref="Value"/> converted to a <see cref="ulong"/>.</returns>
		/// <exception cref="OverflowException"><see cref="Value"/> cannot fit within <see cref="ulong"/>'s value range without overflowing.</exception>
		public ulong ToUInt64() { return Member.ToUInt64(); }

		/// <summary>
		/// Retrieves the hash code of <see cref="Value"/>.
		/// </summary>
		/// <returns>The hash code of <see cref="Value"/>.</returns>
		public sealed override int GetHashCode() { return Member.GetHashCode(); }

		/// <summary>
		/// Indicates whether the specified <see cref="EnumMember"/> is equal to the current <see cref="EnumMember"/>.
		/// </summary>
		/// <param name="other">The other <see cref="EnumMember"/>.</param>
		/// <returns>Indication whether the specified <see cref="EnumMember"/> is equal to the current <see cref="EnumMember"/>.</returns>
		public bool Equals(EnumMember other) { return ReferenceEquals(this, other); }

		/// <summary>
		/// Indicates whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
		/// </summary>
		/// <param name="other">The other <see cref="object"/>.</param>
		/// <returns>Indication whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.</returns>
		public sealed override bool Equals(object other) { return ReferenceEquals(this, other); }

		internal abstract Enum GetValue();

		internal abstract IEnumerable<Enum> GetFlags();

		internal abstract IEnumerable<EnumMember> GetFlagMembers();

		internal bool IsValidFlagCombination() { return Member.IsValidFlagCombination(); }

		internal int GetFlagCount() { return Member.GetFlagCount(); }

		internal bool HasAnyFlags() { return Member.HasAnyFlags(); }

		internal bool HasAllFlags() { return Member.HasAllFlags(); }

		string IFormattable.ToString(string format, IFormatProvider formatProvider) { return Member.ToString(format, formatProvider); }

		TypeCode IConvertible.GetTypeCode() { return Member.GetTypeCode(); }

		bool IConvertible.ToBoolean(IFormatProvider provider) { return Member.ToBoolean(provider); }

		char IConvertible.ToChar(IFormatProvider provider) { return Member.ToChar(provider); }

		sbyte IConvertible.ToSByte(IFormatProvider provider) { return Member.ToSByte(provider); }

		byte IConvertible.ToByte(IFormatProvider provider) { return Member.ToByte(provider); }

		short IConvertible.ToInt16(IFormatProvider provider) { return Member.ToInt16(provider); }

		ushort IConvertible.ToUInt16(IFormatProvider provider) { return Member.ToUInt16(provider); }

		int IConvertible.ToInt32(IFormatProvider provider) { return Member.ToInt32(provider); }

		uint IConvertible.ToUInt32(IFormatProvider provider) { return Member.ToUInt32(provider); }

		long IConvertible.ToInt64(IFormatProvider provider) { return Member.ToInt64(provider); }

		ulong IConvertible.ToUInt64(IFormatProvider provider) { return Member.ToUInt64(provider); }

		float IConvertible.ToSingle(IFormatProvider provider) { return Member.ToSingle(provider); }

		double IConvertible.ToDouble(IFormatProvider provider) { return Member.ToDouble(provider); }

		decimal IConvertible.ToDecimal(IFormatProvider provider) { return Member.ToDecimal(provider); }

		DateTime IConvertible.ToDateTime(IFormatProvider provider) { return Member.ToDateTime(provider); }

		string IConvertible.ToString(IFormatProvider provider) { return Member.ToString(provider); }

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) { return Member.ToType(conversionType, provider); }

		// implemented in derived class
		int IComparable.CompareTo(object obj) { return 0; }

		// implemented in derived class
		int IComparable<EnumMember>.CompareTo(EnumMember other) { return 0; }
	}

	/// <summary>
	/// An enum member which composes its name, value, and attributes.
	/// </summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	public abstract class EnumMember<TEnum> : EnumMember, IComparable<EnumMember<TEnum>>, IEquatable<EnumMember<TEnum>>
		where TEnum : struct, Enum, IComparable
	{
		internal EnumMember(IEnumMember member)
			: base(member)
		{
		}

		/// <summary>
		/// The enum member's value.
		/// </summary>
		public new TEnum Value => GetGenericValue();

		/// <summary>
		/// Indicates whether the specified <see cref="EnumMember{TEnum}"/> is equal to the current <see cref="EnumMember{TEnum}"/>.
		/// </summary>
		/// <param name="other">The other <see cref="EnumMember{TEnum}"/>.</param>
		/// <returns>Indication whether the specified <see cref="EnumMember{TEnum}"/> is equal to the current <see cref="EnumMember{TEnum}"/>.</returns>
		public bool Equals(EnumMember<TEnum> other) { return ReferenceEquals(this, other); }

		internal abstract TEnum GetGenericValue();

		internal abstract IEnumerable<TEnum> GetGenericFlags();

		internal abstract IEnumerable<EnumMember<TEnum>> GetGenericFlagMembers();

		[NotNull] internal sealed override Enum GetValue() { return GetGenericValue(); }

		[NotNull] internal sealed override IEnumerable<Enum> GetFlags() { return GetGenericFlags().Cast<Enum>(); }

		internal sealed override IEnumerable<EnumMember> GetFlagMembers() { return GetGenericFlagMembers(); }

		// Implemented in derived class
		int IComparable<EnumMember<TEnum>>.CompareTo(EnumMember<TEnum> other) { return 0; }
	}

	internal class EnumMember<TEnum, TInt, TIntProvider> : EnumMember<TEnum>, IComparable<EnumMember<TEnum>>, IComparable<EnumMember>, IComparable
		where TEnum : struct, Enum, IComparable
		where TInt : struct, IComparable, IComparable<TInt>, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		internal EnumMember(EnumMember<TInt, TIntProvider> member)
			: base(member)
		{
		}

		internal new EnumMember<TInt, TIntProvider> Member => (EnumMember<TInt, TIntProvider>)base.Member;

		internal override TEnum GetGenericValue() { return EnumInfo<TEnum, TInt, TIntProvider>.ToEnum(Member.Value); }

		[NotNull] internal override IEnumerable<TEnum> GetGenericFlags() { return Member.GetFlags().Select(EnumInfo<TEnum, TInt, TIntProvider>.ToEnum); }

		[NotNull]
		internal override IEnumerable<EnumMember<TEnum>> GetGenericFlagMembers()
		{
			return Member.GetFlagMembers().Select(flag => (EnumMember<TEnum>)flag.EnumMemberInternal);
		}

		public int CompareTo(object other) { return CompareTo(other as EnumMember<TEnum>); }

		public int CompareTo(EnumMember other) { return CompareTo(other as EnumMember<TEnum>); }

		public int CompareTo(EnumMember<TEnum> other)
		{
			return other != null
						? Member.CompareTo(((EnumMember<TEnum, TInt, TIntProvider>)other).Member)
						: 1;
		}
	}

	internal class UntypedEnumMember<TInt, TIntProvider> : EnumMember, IComparable<UntypedEnumMember<TInt, TIntProvider>>, IEquatable<UntypedEnumMember<TInt, TIntProvider>>, IComparable<EnumMember>, IComparable
		where TInt : struct, IComparable, IComparable<TInt>, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		internal UntypedEnumMember(EnumMember<TInt, TIntProvider> member)
			: base(member)
		{
		}

		internal new EnumMember<TInt, TIntProvider> Member => (EnumMember<TInt, TIntProvider>)base.Member;

		public bool Equals(UntypedEnumMember<TInt, TIntProvider> other) { return ReferenceEquals(this, other); }

		/// <inheritdoc />
		internal override Enum GetValue() { return Value; }


		[NotNull] internal override IEnumerable<Enum> GetFlags() { return Member.GetFlags().Select(EnumInfo<TInt, TIntProvider>.ToEnum); }


		[NotNull] internal override IEnumerable<EnumMember> GetFlagMembers() { return Member.GetFlagMembers().Select(flag => flag.EnumMemberInternal); }


		public int CompareTo(UntypedEnumMember<TInt, TIntProvider> other)
		{
			return other != null
						? Member.CompareTo(other.Member)
						: 1;
		}
	}
	
	// Putting the logic here as opposed to directly in EnumMember<TEnum, TInt, TIntProvider>
	// reduces memory usage because it doesn't have the enum type as a generic type parameter.
	internal sealed class EnumMember<TInt, TIntProvider> : IEnumMember, IConvertible
		where TInt : struct, IComparable, IComparable<TInt>, IEquatable<TInt>, IConvertible
		where TIntProvider : INumericProvider<TInt>, new()
	{
		private string _displayName;
		private EnumMember _enumMember;

		public EnumMember(TInt value, string name, AttributeCollection attributes, EnumCache<TInt, TIntProvider> enumCache)
		{
			Value = value;
			Name = name;
			Attributes = attributes;
			EnumCache = enumCache;
		}

		public override int GetHashCode() { return Value.GetHashCode(); }

		public TInt Value { get; }

		public string Name { get; }

		public string DisplayName =>
			_displayName ??= Attributes.Get<DisplayAttribute>()?.Name
							?? Attributes.Get<DisplayNameAttribute>()?.DisplayName
							?? Name;

		public AttributeCollection Attributes { get; }

		internal EnumCache<TInt, TIntProvider> EnumCache { get; }

		internal EnumMember EnumMemberInternal
		{
			get
			{
				EnumMember enumMember;
				return _enumMember ?? Interlocked.CompareExchange(ref _enumMember, enumMember = EnumCache.EnumInfo.CreateEnumMember(this), null) ?? enumMember;
			}
		}

		public string AsString(string format) { return EnumCache.AsStringInternal(Value, this, format); }

		public string AsString(EnumFormat format)
		{
			bool isInitialized = true;
			EnumMember<TInt, TIntProvider> member = this;
			return EnumCache.FormatInternal(Value, ref isInitialized, ref member, format);
		}

		public string AsString(EnumFormat format0, EnumFormat format1) { return EnumCache.FormatInternal(Value, this, format0, format1); }

		public string AsString(EnumFormat format0, EnumFormat format1, EnumFormat format2) { return EnumCache.FormatInternal(Value, this, format0, format1, format2); }

		public string AsString([NotNull] params EnumFormat[] formats) { return EnumCache.FormatInternal(Value, this, formats); }

		public string Format([NotNull] string format) { return EnumCache.FormatInternal(Value, this, format); }

		public string Format([NotNull] params EnumFormat[] formats) { return EnumCache.FormatInternal(Value, this, formats); }

		public sbyte ToSByte() { return Value.ToSByte(null); }

		public byte ToByte() { return Value.ToByte(null); }

		public short ToInt16() { return Value.ToInt16(null); }

		public ushort ToUInt16() { return Value.ToUInt16(null); }

		public int ToInt32() { return Value.ToInt32(null); }

		public uint ToUInt32() { return Value.ToUInt32(null); }

		public long ToInt64() { return Value.ToInt64(null); }

		public ulong ToUInt64() { return Value.ToUInt64(null); }

		public bool IsValidFlagCombination() { return EnumCache.IsValidFlagCombination(Value); }

		public int GetFlagCount() { return EnumCache.GetFlagCount(Value); }

		public bool HasAnyFlags() { return EnumCache.HasAnyFlags(Value); }

		public bool HasAllFlags() { return EnumCache.HasAllFlags(Value); }

		public IEnumerable<TInt> GetFlags() { return EnumCache.GetFlags(Value); }

		[NotNull]
		public IEnumerable<EnumMember<TInt, TIntProvider>> GetFlagMembers() { return EnumCache.GetFlagMembers(Value); }

		internal int CompareTo([NotNull] EnumMember<TInt, TIntProvider> other) { return Value.CompareTo(other.Value); }

		string IFormattable.ToString(string format, IFormatProvider formatProvider) { return AsString(format); }

		TypeCode IConvertible.GetTypeCode() { return Value.GetTypeCode(); }

		bool IConvertible.ToBoolean(IFormatProvider provider) { return Value.ToBoolean(provider); }

		char IConvertible.ToChar(IFormatProvider provider) { return Value.ToChar(provider); }

		sbyte IConvertible.ToSByte(IFormatProvider provider) { return Value.ToSByte(provider); }

		byte IConvertible.ToByte(IFormatProvider provider) { return Value.ToByte(provider); }

		short IConvertible.ToInt16(IFormatProvider provider) { return Value.ToInt16(provider); }

		ushort IConvertible.ToUInt16(IFormatProvider provider) { return Value.ToUInt16(provider); }

		int IConvertible.ToInt32(IFormatProvider provider) { return Value.ToInt32(provider); }

		uint IConvertible.ToUInt32(IFormatProvider provider) { return Value.ToUInt32(provider); }

		long IConvertible.ToInt64(IFormatProvider provider) { return Value.ToInt64(provider); }

		ulong IConvertible.ToUInt64(IFormatProvider provider) { return Value.ToUInt64(provider); }

		float IConvertible.ToSingle(IFormatProvider provider) { return Value.ToSingle(provider); }

		double IConvertible.ToDouble(IFormatProvider provider) { return Value.ToDouble(provider); }

		decimal IConvertible.ToDecimal(IFormatProvider provider) { return Value.ToDecimal(provider); }

		DateTime IConvertible.ToDateTime(IFormatProvider provider) { return Value.ToDateTime(provider); }

		string IConvertible.ToString(IFormatProvider provider) { return ToString(); }

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) { return Value.ToType(conversionType, provider); }

		[NotNull] object IEnumMember.GetUnderlyingValue() { return Value; }
	}
}