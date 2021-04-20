using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using essentialMix.Collections;
using JetBrains.Annotations;
using essentialMix.Exceptions;
using essentialMix.Extensions;
using Other.TylerBrinkley.Enumeration;
using Other.TylerBrinkley.Enumeration.Numeric;
using AttributeCollection = Other.TylerBrinkley.Collections.AttributeCollection;

namespace essentialMix.Helpers
{
	/// <summary>
	/// Static class that provides efficient type-safe enum operations through the use of cached enum members.
	/// Many operations are exposed as C# extension methods for convenience.
	/// </summary>
	public static class EnumHelper
	{
		internal const string DEFAULT_DELIMITER = ", ";

		internal static readonly EnumFormat[] DefaultFormats =
		{
			EnumFormat.Name,
			EnumFormat.UnderlyingValue
		};

		internal static readonly EnumFormat[] NameFormatArray =
		{
			EnumFormat.Name
		};

		private const int S_STARTING_CUSTOM_ENUM_FORMAT_VALUE = (int)EnumFormat.DisplayName + 1;

		private static readonly ConcurrentDictionary<Type, IEnumInfo> __enumInfo = new ConcurrentDictionary<Type, IEnumInfo>();

		private static Func<EnumMember, string>[] __customEnumMemberFormatters = new Func<EnumMember, string>[0];

		/// <summary>
		///     Retrieves the underlying type of <paramref name="enumType" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns>The underlying type of <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static Type GetUnderlyingType([NotNull] Type enumType) { return GetInfo(enumType).UnderlyingType; }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s underlying type's <see cref="TypeCode" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s underlying type's <see cref="TypeCode" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static TypeCode GetTypeCode([NotNull] Type enumType) { return GetInfo(enumType).TypeCode; }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s member count.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s member count.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static int GetCount([NotNull] Type enumType) { return GetInfo(enumType).GetCount(); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s member count.
		///     The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><paramref name="enumType" />'s member count.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="selection" /> is an invalid value.
		/// </exception>
		public static int GetCount([NotNull] Type enumType, EnumMemberSelection selection) { return GetInfo(enumType).GetCount(selection); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members in increasing value order.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s members in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static IEnumerable<EnumMember> GetMembers([NotNull] Type enumType) { return GetInfo(enumType).GetMembers(); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members in increasing value order.
		///     The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><paramref name="enumType" />'s members in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="selection" /> is an invalid value.
		/// </exception>
		public static IEnumerable<EnumMember> GetMembers([NotNull] Type enumType, EnumMemberSelection selection) { return GetInfo(enumType).GetMembers(selection); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' names in increasing value order.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s members' names in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static IEnumerable<string> GetNames([NotNull] Type enumType) { return GetInfo(enumType).GetNames(); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' names in increasing value order.
		///     The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><paramref name="enumType" />'s members' names in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="selection" /> is an invalid value.
		/// </exception>
		public static IEnumerable<string> GetNames([NotNull] Type enumType, EnumMemberSelection selection) { return GetInfo(enumType).GetNames(selection); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' display names in increasing value order.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s members' display names in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static IEnumerable<string> GetDisplayNames([NotNull] Type enumType) { return GetInfo(enumType).GetDisplayNames(); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' display names in increasing value order.
		///     The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><paramref name="enumType" />'s members' display names in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="selection" /> is an invalid value.
		/// </exception>
		public static IEnumerable<string> GetDisplayNames([NotNull] Type enumType, EnumMemberSelection selection) { return GetInfo(enumType).GetDisplayNames(selection); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' values in increasing value order.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns><paramref name="enumType" />'s members' values in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static IEnumerable<Enum> GetValues([NotNull] Type enumType) { return GetInfo(enumType).GetValues(); }

		/// <summary>
		///     Retrieves <paramref name="enumType" />'s members' values in increasing value order.
		///     The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><paramref name="enumType" />'s members' values in increasing value order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="selection" /> is an invalid value.
		/// </exception>
		public static IEnumerable<Enum> GetValues([NotNull] Type enumType, EnumMemberSelection selection) { return GetInfo(enumType).GetValues(selection); }

		public static void GetBoundaries([NotNull] Type enumType, out Enum minimum, out Enum maximum)
		{
			Enum[] values = GetValues(enumType).ToArray();

			if (values.Length > 0)
			{
				minimum = values[0];
				maximum = values[values.Length - 1];
			}
			else
			{
				minimum = maximum = (Enum)enumType.Default();
			}
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">
		///     Value to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />, <see cref="ushort" />
		///     ,
		///     <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <paramref name="enumType" />,
		///     <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is not a valid type.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, object value) { return ToObject(enumType, value, EnumValidation.None); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">
		///     Value to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />, <see cref="ushort" />
		///     ,
		///     <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <paramref name="enumType" />,
		///     <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is not a valid type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, object value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, sbyte value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, sbyte value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, byte value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, byte value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, short value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, short value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, ushort value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, ushort value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, int value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, int value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, uint value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, uint value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, long value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, long value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, ulong value) { return GetInfo(enumType).ToObject(value); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while checking
		///     that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <paramref name="enumType" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static Enum ToObject([NotNull] Type enumType, ulong value, EnumValidation validation) { return GetInfo(enumType).ToObject(value, validation); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">
		///     Value to try to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />,
		///     <see cref="ushort" />,
		///     <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <paramref name="enumType" />,
		///     <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, object value, out Enum result) { return TryToObject(enumType, value, EnumValidation.None, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">
		///     Value to try to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />,
		///     <see cref="ushort" />,
		///     <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <paramref name="enumType" />,
		///     <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, object value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, sbyte value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, sbyte value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, byte value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, byte value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, short value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, short value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, ushort value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, ushort value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, int value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, int value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, uint value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, uint value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, long value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, long value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryToObject([NotNull] Type enumType, ulong value, out Enum result) { return GetInfo(enumType).TryToObject(value, out result); }

		/// <summary>
		///     Tries to convert the specified <paramref name="value" /> to a value of type <paramref name="enumType" /> while
		///     checking that it doesn't overflow the
		///     underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a value of type <paramref name="enumType" /> whose value
		///     is <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool TryToObject([NotNull] Type enumType, ulong value, EnumValidation validation, out Enum result)
		{
			return GetInfo(enumType).TryToObject(value, out result, validation);
		}

		/// <summary>
		///     Indicates if <paramref name="value" /> is valid. If <paramref name="enumType" /> is a standard enum it returns
		///     whether the value is defined.
		///     If <paramref name="enumType" /> is marked with <see cref="FlagsAttribute" /> it returns whether it's a valid flag
		///     combination of <paramref name="enumType" />'s defined values
		///     or is defined. Or if <paramref name="enumType" /> has an attribute that implements
		///     <see cref="IEnumValidatorAttribute{TEnum}" />
		///     then that attribute's <see cref="IEnumValidatorAttribute{TEnum}.IsValid(TEnum)" /> method is used.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns>Indication if <paramref name="value" /> is valid.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static bool IsValid([NotNull] Type enumType, object value) { return IsValid(enumType, value, EnumValidation.Default); }

		/// <summary>
		///     Indicates if <paramref name="value" /> is valid. If <paramref name="enumType" /> is a standard enum it returns
		///     whether the value is defined.
		///     If <paramref name="enumType" /> is marked with <see cref="FlagsAttribute" /> it returns whether it's a valid flag
		///     combination of <paramref name="enumType" />'s defined values
		///     or is defined. Or if <paramref name="enumType" /> has an attribute that implements
		///     <see cref="IEnumValidatorAttribute{TEnum}" />
		///     then that attribute's <see cref="IEnumValidatorAttribute{TEnum}.IsValid(TEnum)" /> method is used.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="validation">The validation to perform on the value.</param>
		/// <returns>Indication if <paramref name="value" /> is valid.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type
		///     -or-
		///     <paramref name="validation" /> is an invalid value.
		/// </exception>
		public static bool IsValid([NotNull] Type enumType, object value, EnumValidation validation)
		{
			return GetInfo(enumType).IsValid(value, validation);
		}

		/// <summary>
		///     Indicates if <paramref name="value" /> is defined.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns>Indication if <paramref name="value" /> is defined.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static bool IsDefined([NotNull] Type enumType, Enum value) { return GetInfo(enumType).IsDefined(value); }

		/// <summary>
		///     Validates that <paramref name="value" /> is valid. If it's not it throws an <see cref="ArgumentException" /> with
		///     the specified <paramref name="paramName" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="paramName">The parameter name to be used if throwing an <see cref="ArgumentException" />.</param>
		/// <returns><paramref name="value" /> for use in fluent API and base constructor method calls.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type
		///     -or-
		///     <paramref name="value" /> is invalid.
		/// </exception>
		public static Enum Validate([NotNull] Type enumType, object value, string paramName) { return Validate(enumType, value, paramName, EnumValidation.Default); }

		/// <summary>
		///     Validates that <paramref name="value" /> is valid. If it's not it throws an <see cref="ArgumentException" /> with
		///     the specified <paramref name="paramName" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="paramName">The parameter name to be used if throwing an <see cref="ArgumentException" />.</param>
		/// <param name="validation">The validation to perform on the value.</param>
		/// <returns><paramref name="value" /> for use in fluent API and base constructor method calls.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type
		///     -or-
		///     <paramref name="validation" /> is an invalid value
		///     -or-
		///     <paramref name="value" /> is invalid.
		/// </exception>
		public static Enum Validate([NotNull] Type enumType, object value, string paramName, EnumValidation validation) { return GetInfo(enumType).Validate(value, paramName, validation); }

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static string AsString([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).AsString(value);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified
		///     <paramref name="format" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="format" /> is an invalid value.</exception>
		public static string AsString([NotNull] Type enumType, Enum value, string format)
		{
			return GetInfo(enumType).AsString(value, format);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified
		///     <paramref name="format" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="format" /> is an invalid value.
		/// </exception>
		public static string AsString([NotNull] Type enumType, Enum value, EnumFormat format)
		{
			return GetInfo(enumType).AsString(value, format);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified formats.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="format0">The first output format to use.</param>
		/// <param name="format1">The second output format to use if using the first resolves to <c>null</c>.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="format0" /> or <paramref name="format1" /> is an invalid value.
		/// </exception>
		public static string AsString([NotNull] Type enumType, Enum value, EnumFormat format0, EnumFormat format1)
		{
			return GetInfo(enumType).AsString(value, format0, format1);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified formats.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="format0">The first output format to use.</param>
		/// <param name="format1">The second output format to use if using the first resolves to <c>null</c>.</param>
		/// <param name="format2">The third output format to use if using the first and second both resolve to <c>null</c>.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="format0" />, <paramref name="format1" />, or <paramref name="format2" /> is an invalid value.
		/// </exception>
		public static string AsString([NotNull] Type enumType, Enum value, EnumFormat format0, EnumFormat format1, EnumFormat format2)
		{
			return GetInfo(enumType).AsString(value, format0, format1, format2);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified
		///     <paramref name="formats" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static string AsString([NotNull] Type enumType, Enum value, params EnumFormat[] formats)
		{
			return GetInfo(enumType).AsString(value, formats);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified
		///     <paramref name="format" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="format">The output format to use.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="enumType" />, <paramref name="value" />, or
		///     <paramref name="format" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="format" /> is an invalid value.</exception>
		public static string Format([NotNull] Type enumType, Enum value, string format)
		{
			return GetInfo(enumType).Format(value, format);
		}

		/// <summary>
		///     Converts the specified <paramref name="value" /> to its string representation using the specified
		///     <paramref name="formats" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>A string representation of <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="enumType" />, <paramref name="value" />, or
		///     <paramref name="formats" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static string Format([NotNull] Type enumType, Enum value, params EnumFormat[] formats)
		{
			return GetInfo(enumType).Format(value, formats);
		}

		/// <summary>
		///     Returns <paramref name="value" />'s underlying integral value.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" />'s underlying integral value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static object GetUnderlyingValue([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetUnderlyingValue(value);
		}

		/// <summary>
		///     Converts <paramref name="value" /> to an <see cref="sbyte" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to an <see cref="sbyte" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="sbyte" />'s value range
		///     without overflowing.
		/// </exception>
		public static sbyte ToSByte([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToSByte(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to a <see cref="byte" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to a <see cref="byte" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="byte" />'s value range
		///     without overflowing.
		/// </exception>
		public static byte ToByte([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToByte(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to an <see cref="short" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to an <see cref="short" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="short" />'s value range
		///     without overflowing.
		/// </exception>
		public static short ToInt16([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToInt16(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to a <see cref="ushort" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to a <see cref="ushort" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="ushort" />'s value range
		///     without overflowing.
		/// </exception>
		public static ushort ToUInt16([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToUInt16(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to an <see cref="int" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to an <see cref="int" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="int" />'s value range
		///     without overflowing.
		/// </exception>
		public static int ToInt32([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToInt32(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to a <see cref="uint" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to a <see cref="uint" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="uint" />'s value range
		///     without overflowing.
		/// </exception>
		public static uint ToUInt32([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToUInt32(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to an <see cref="long" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to an <see cref="long" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="long" />'s value range
		///     without overflowing.
		/// </exception>
		public static long ToInt64([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToInt64(value); }

		/// <summary>
		///     Converts <paramref name="value" /> to a <see cref="ulong" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" /> converted to a <see cref="ulong" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> cannot fit within <see cref="ulong" />'s value range
		///     without overflowing.
		/// </exception>
		public static ulong ToUInt64([NotNull] Type enumType, Enum value) { return GetInfo(enumType).ToUInt64(value); }

		/// <summary>
		///     Indicates if <paramref name="value" /> equals <paramref name="other" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="other">The other enum value.</param>
		/// <returns>Indication if <paramref name="value" /> equals <paramref name="other" />.</returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="enumType" />, <paramref name="value" />, or
		///     <paramref name="other" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> or <paramref name="other" /> is of an invalid type.
		/// </exception>
		public static bool Equals([NotNull] Type enumType, Enum value, Enum other)
		{
			return GetInfo(enumType).Equals(value, other);
		}

		/// <summary>
		///     Compares <paramref name="value" /> to <paramref name="other" /> for ordering.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <param name="other">The other enum value.</param>
		/// <returns>
		///     1 if <paramref name="value" /> is greater than <paramref name="other" />, 0 if <paramref name="value" /> equals
		///     <paramref name="other" />,
		///     and -1 if <paramref name="value" /> is less than <paramref name="other" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="enumType" />, <paramref name="value" />, or
		///     <paramref name="other" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> or <paramref name="other" /> is of an invalid type.
		/// </exception>
		public static int CompareTo([NotNull] Type enumType, Enum value, Enum other) { return GetInfo(enumType).Compare(value, other); }

		/// <summary>
		///     Retrieves <paramref name="value" />'s enum member name if defined otherwise <c>null</c>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" />'s enum member name if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static string GetName([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetName(value);
		}

		/// <summary>
		///     Retrieves <paramref name="value" />'s enum member attributes if defined otherwise <c>null</c>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns><paramref name="value" />'s enum member attributes if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static AttributeCollection GetAttributes([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetAttributes(value);
		}

		/// <summary>
		///     Retrieves an enum member with the specified <paramref name="value" /> if defined otherwise <c>null</c>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum value.</param>
		/// <returns>Enum member with the specified <paramref name="value" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> is of an invalid type.
		/// </exception>
		public static EnumMember GetMember([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetMember(value);
		}

		/// <summary>
		///     Retrieves the enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.
		///     Is case-sensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="name">The enum member name.</param>
		/// <returns>Enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static EnumMember GetMember([NotNull] Type enumType, string name) { return GetInfo(enumType).GetMember(name); }

		/// <summary>
		///     Retrieves the enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.
		///     The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="name">The enum member name.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>Enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="name" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static EnumMember GetMember([NotNull] Type enumType, string name, bool ignoreCase) { return GetInfo(enumType).GetMember(name, ignoreCase); }

		/// <summary>
		///     Retrieves an enum member whose string representation using the specified <paramref name="formats" /> is
		///     <paramref name="value" /> if defined otherwise <c>null</c>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member's string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Enum member represented by <paramref name="value" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static EnumMember GetMember([NotNull] Type enumType, string value, params EnumFormat[] formats) { return GetInfo(enumType).GetMember(value, false, formats); }

		/// <summary>
		///     Retrieves an enum member whose string representation using the specified <paramref name="formats" /> is
		///     <paramref name="value" /> if defined otherwise <c>null</c>.
		///     The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member's string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Enum member represented by <paramref name="value" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static EnumMember GetMember([NotNull] Type enumType, string value, bool ignoreCase, params EnumFormat[] formats)
		{
			return GetInfo(enumType).GetMember(value, ignoreCase, formats);
		}

		/// <summary>
		///     Converts the string representation of one or more member names or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <returns>A <paramref name="enumType" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> doesn't represent a member name or value of <paramref name="enumType" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of <paramref name="enumType" />'s
		///     underlying type.
		/// </exception>
		public static Enum Parse([NotNull] Type enumType, string value) { return Parse(enumType, value, false, null); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />
		///     using the specified parsing enum formats.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <paramref name="enumType" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> doesn't represent a member or value of <paramref name="enumType" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <paramref name="enumType" />.
		/// </exception>
		public static Enum Parse([NotNull] Type enumType, string value, params EnumFormat[] formats) { return Parse(enumType, value, false, formats); }

		/// <summary>
		///     Converts the string representation of one or more member names or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />.
		///     The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>The <paramref name="enumType" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> doesn't represent a member name or value of <paramref name="enumType" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <paramref name="enumType" />.
		/// </exception>
		public static Enum Parse([NotNull] Type enumType, string value, bool ignoreCase) { return Parse(enumType, value, ignoreCase, null); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies if the operation
		///     is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <paramref name="enumType" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> or <paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="value" /> doesn't represent a member or value of <paramref name="enumType" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <paramref name="enumType" />.
		/// </exception>
		public static Enum Parse([NotNull] Type enumType, string value, bool ignoreCase, params EnumFormat[] formats)
		{
			return GetInfo(enumType).Parse(value, ignoreCase, formats);
		}

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <paramref name="enumType" /> to
		///     its respective value of type <paramref name="enumType" />.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <paramref name="enumType" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryParse([NotNull] Type enumType, string value, out Enum result) { return TryParse(enumType, value, false, out result, null); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />
		///     using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <paramref name="enumType" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static bool TryParse([NotNull] Type enumType, string value, out Enum result, params EnumFormat[] formats) { return TryParse(enumType, value, false, out result, formats); }

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <paramref name="enumType" /> to
		///     its respective value of type <paramref name="enumType" />.
		///     The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive. The return value
		///     indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <paramref name="enumType" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType" /> is not an enum type.</exception>
		public static bool TryParse([NotNull] Type enumType, string value, bool ignoreCase, out Enum result) { return TryParse(enumType, value, ignoreCase, out result, null); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <paramref name="enumType" /> to its
		///     respective value of type <paramref name="enumType" />
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies whether the
		///     operation is case-insensitive.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <paramref name="enumType" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="enumType" /> is not an enum type
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		public static bool TryParse([NotNull] Type enumType, string value, bool ignoreCase, out Enum result, params EnumFormat[] formats)
		{
			return GetInfo(enumType).TryParse(value, ignoreCase, out result, formats);
		}

		/// <summary>
		/// Indicates if <paramref name="enumType"/> is marked with the <see cref="FlagsAttribute"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns>Indication if <paramref name="enumType"/> is marked with the <see cref="FlagsAttribute"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static bool IsFlagEnum([NotNull] Type enumType) { return GetInfo(enumType).IsFlagEnum; }

		/// <summary>
		/// Retrieves all the flags defined by <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns>All the flags defined by <paramref name="enumType"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static Enum GetAllFlags([NotNull] Type enumType) { return GetInfo(enumType).AllFlags; }

		/// <summary>
		/// Indicates whether <paramref name="value"/> is a valid flag combination of <paramref name="enumType"/>'s defined flags.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>Indication of whether <paramref name="value"/> is a valid flag combination of <paramref name="enumType"/>'s defined flags.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type

		/// <paramref name="value"/> is of an invalid type.</exception>
		public static bool IsValidFlagCombination([NotNull] Type enumType, Enum value)
			{
			return GetInfo(enumType).IsValidFlagCombination(value);
			}

		/// <summary>
		/// Retrieves the names of <paramref name="value"/>'s flags delimited with commas or if empty returns the name of the zero flag if defined otherwise "0".
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>The names of <paramref name="value"/>'s flags delimited with commas or if empty returns the name of the zero flag if defined otherwise "0".
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static string FormatFlags([NotNull] Type enumType, Enum value) { return FormatFlags(enumType, value, null, null); }

		/// <summary>
		/// Retrieves <paramref name="value"/>'s flags formatted with <paramref name="formats"/> and delimited with commas
		/// or if empty returns the zero flag formatted with <paramref name="formats"/>.
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns><paramref name="value"/>'s flags formatted with <paramref name="formats"/> and delimited with commas
		/// or if empty returns the zero flag formatted with <paramref name="formats"/>.
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static string FormatFlags([NotNull] Type enumType, Enum value, params EnumFormat[] formats) { return FormatFlags(enumType, value, null, formats); }

		/// <summary>
		/// Retrieves the names of <paramref name="value"/>'s flags delimited with <paramref name="delimiter"/> or if empty returns the name of the zero flag if defined otherwise "0".
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="delimiter">The delimiter to use to separate individual flags.</param>
		/// <returns>The names of <paramref name="value"/>'s flags delimited with <paramref name="delimiter"/> or if empty returns the name of the zero flag if defined otherwise "0".
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static string FormatFlags([NotNull] Type enumType, Enum value, string delimiter) { return FormatFlags(enumType, value, delimiter, null); }

		/// <summary>
		/// Retrieves <paramref name="value"/>'s flags formatted with <paramref name="formats"/> and delimited with <paramref name="delimiter"/>
		/// or if empty returns the zero flag formatted with <paramref name="formats"/>.
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="delimiter">The delimiter to use to separate individual flags.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns><paramref name="value"/>'s flags formatted with <paramref name="formats"/> and delimited with <paramref name="delimiter"/>
		/// or if empty returns the zero flag formatted with <paramref name="formats"/>.
		/// If <paramref name="value"/> is not a valid flag combination <c>null</c> is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static string FormatFlags([NotNull] Type enumType, Enum value, string delimiter, params EnumFormat[] formats)
		{
			return GetInfo(enumType).FormatFlags(value, delimiter, formats);
		}

		/// <summary>
		/// Retrieves the flags that compose <paramref name="value"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>The flags that compose <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static IEnumerable<Enum> GetFlags([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetFlags(value);
		}

		/// <summary>
		/// Retrieves the <see cref="EnumMember"/>s of the flags that compose <paramref name="value"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>The <see cref="EnumMember"/>s of the flags that compose <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static IEnumerable<EnumMember> GetFlagMembers([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).GetFlagMembers(value);
		}

		/// <summary>
		/// Retrieves the flag count of <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <returns>The flag count of <paramref name="enumType"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static int GetFlagCount([NotNull] Type enumType) { return GetInfo(enumType).GetFlagCount(); }

		/// <summary>
		/// Retrieves the flag count of <paramref name="value"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>The flag count of <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static int GetFlagCount([NotNull] Type enumType, Enum value)
			{
			return GetInfo(enumType).GetFlagCount(value);
			}

		/// <summary>
		/// Retrieves the flag count of <paramref name="otherFlags"/> that <paramref name="value"/> has.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns>The flag count of <paramref name="otherFlags"/> that <paramref name="value"/> has.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static int GetFlagCount([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).GetFlagCount(value, otherFlags);
		}

		/// <summary>
		/// Indicates if <paramref name="value"/> has any flags.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>Indication if <paramref name="value"/> has any flags.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static bool HasAnyFlags([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).HasAnyFlags(value);
		}

		/// <summary>
		/// Indicates if <paramref name="value"/> has any flags that are in <paramref name="otherFlags"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns>Indication if <paramref name="value"/> has any flags that are in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static bool HasAnyFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).HasAnyFlags(value, otherFlags);
		}

		/// <summary>
		/// Indicates if <paramref name="value"/> has all of the flags that are defined in <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns>Indication if <paramref name="value"/> has all of the flags that are defined in <paramref name="enumType"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static bool HasAllFlags([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).HasAllFlags(value);
		}

		/// <summary>
		/// Indicates if <paramref name="value"/> has all of the flags that are in <paramref name="otherFlags"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns>Indication if <paramref name="value"/> has all of the flags that are in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static bool HasAllFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).HasAllFlags(value, otherFlags);
		}

		/// <summary>
		/// Returns <paramref name="value"/> with all of it's flags toggled. Equivalent to the bitwise "xor" operator with <see cref="GetAllFlags(Type)"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <returns><paramref name="value"/> with all of it's flags toggled.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> is of an invalid type.</exception>
		public static Enum ToggleFlags([NotNull] Type enumType, Enum value)
		{
			return GetInfo(enumType).ToggleFlags(value);
		}

		/// <summary>
		/// Returns <paramref name="value"/> while toggling the flags that are in <paramref name="otherFlags"/>. Equivalent to the bitwise "xor" operator.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns><paramref name="value"/> while toggling the flags that are in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static Enum ToggleFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).ToggleFlags(value, otherFlags);
		}

		/// <summary>
		/// Returns <paramref name="value"/> with only the flags that are also in <paramref name="otherFlags"/>. Equivalent to the bitwise "and" operation.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns><paramref name="value"/> with only the flags that are also in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static Enum CommonFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).CommonFlags(value, otherFlags);
		}

		/// <summary>
		/// Combines the flags of <paramref name="value"/> and <paramref name="otherFlags"/>. Equivalent to the bitwise "or" operation.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns>Combination of <paramref name="value"/> with the flags in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static Enum CombineFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).CombineFlags(value, otherFlags);
		}

		/// <summary>
		/// Combines all of the flags of <paramref name="flags"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="flags">The flags enum values.</param>
		/// <returns>Combination of all of the flags of <paramref name="flags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="flags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="flags"/> contains a value that is of an invalid type.</exception>
		public static Enum CombineFlags([NotNull] Type enumType, params Enum[] flags) { return CombineFlags(enumType, (IEnumerable<Enum>)flags); }

		/// <summary>
		/// Combines all of the flags of <paramref name="flags"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="flags">The flags enum values.</param>
		/// <returns>Combination of all of the flags of <paramref name="flags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="flags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="flags"/> contains a value that is of an invalid type.</exception>
		public static Enum CombineFlags([NotNull] Type enumType, IEnumerable<Enum> flags)
		{
			return GetInfo(enumType).CombineFlags(flags);
		}

		/// <summary>
		/// Returns <paramref name="value"/> without the flags specified in <paramref name="otherFlags"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns><paramref name="value"/> without the flags specified in <paramref name="otherFlags"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/>, <paramref name="value"/>, or <paramref name="otherFlags"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> or <paramref name="otherFlags"/> is of an invalid type.</exception>
		public static Enum RemoveFlags([NotNull] Type enumType, Enum value, Enum otherFlags)
		{
			return GetInfo(enumType).RemoveFlags(value, otherFlags);
		}

		/// <summary>
		/// Converts the string representation of one or more member names or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <returns>A <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member name or value of <paramref name="enumType"/>.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <paramref name="enumType"/>'s underlying type.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value) { return ParseFlags(enumType, value, false, null, null); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member or value of <paramref name="enumType"/>
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, params EnumFormat[] formats) { return ParseFlags(enumType, value, false, null, formats); }

		/// <summary>
		/// Converts the string representation of one or more member names or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>.
		/// The parameter <paramref name="ignoreCase"/> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>The <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member name or value of <paramref name="enumType"/>.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, bool ignoreCase) { return ParseFlags(enumType, value, ignoreCase, null, null); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase"/> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member or value of <paramref name="enumType"/>
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, bool ignoreCase, params EnumFormat[] formats)
		{
			return ParseFlags(enumType, value, ignoreCase, null, formats);
		}

		/// <summary>
		/// Converts the string representation of one or more member names or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <returns>A <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member name or value of <paramref name="enumType"/>.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <paramref name="enumType"/>'s underlying type.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, string delimiter) { return ParseFlags(enumType, value, false, delimiter, null); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member or value of <paramref name="enumType"/>
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, string delimiter, params EnumFormat[] formats)
		{
			return ParseFlags(enumType, value, false, delimiter, formats);
		}

		/// <summary>
		/// Converts the string representation of one or more member names or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>.
		/// The parameter <paramref name="ignoreCase"/> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <returns>The <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member name or value of <paramref name="enumType"/>.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, bool ignoreCase, string delimiter) { return ParseFlags(enumType, value, ignoreCase, delimiter, null); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase"/> specifies if the operation is case-insensitive.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <paramref name="enumType"/> value that is represented by <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> or <paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="value"/> doesn't represent a member or value of <paramref name="enumType"/>
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of the underlying type of <paramref name="enumType"/>.</exception>
		public static Enum ParseFlags([NotNull] Type enumType, string value, bool ignoreCase, string delimiter, params EnumFormat[] formats)
		{
			return GetInfo(enumType).ParseFlags(value, ignoreCase, delimiter, formats);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, out Enum result) { return TryParseFlags(enumType, value, false, null, out result, null); }

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, out Enum result, params EnumFormat[] formats)
		{
			return TryParseFlags(enumType, value, false, null, out result, formats);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>.
		/// The parameter <paramref name="ignoreCase"/> specifies whether the operation is case-insensitive. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, bool ignoreCase, out Enum result)
		{
			return TryParseFlags(enumType, value, ignoreCase, null, out result, null);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <paramref name="enumType"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase"/> specifies whether the operation is case-insensitive.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, bool ignoreCase, out Enum result, params EnumFormat[] formats)
		{
			return TryParseFlags(enumType, value, ignoreCase, null, out result, formats);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, string delimiter, out Enum result)
		{
			return TryParseFlags(enumType, value, false, delimiter, out result, null);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, string delimiter, out Enum result, params EnumFormat[] formats)
		{
			return TryParseFlags(enumType, value, false, delimiter, out result, formats);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>.
		/// The parameter <paramref name="ignoreCase"/> specifies whether the operation is case-insensitive. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, bool ignoreCase, string delimiter, out Enum result)
		{
			return TryParseFlags(enumType, value, ignoreCase, delimiter, out result, null);
		}

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <paramref name="enumType"/> delimited with <paramref name="delimiter"/> to its respective value of type <paramref name="enumType"/>
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase"/> specifies whether the operation is case-insensitive.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="enumType">The enum type.</param>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">If the conversion succeeds this contains a value of type <paramref name="enumType"/> that is represented by <paramref name="value"/>.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type
		/// -or-
		/// <paramref name="formats"/> contains an invalid value.</exception>
		public static bool TryParseFlags([NotNull] Type enumType, string value, bool ignoreCase, string delimiter, out Enum result, params EnumFormat[] formats)
		{
			return GetInfo(enumType).TryParseFlags(value, ignoreCase, delimiter, out result, formats);
		}

		/// <summary>
		/// Retrieves the flags that compose <paramref name="member"/>'s value.
		/// </summary>
		/// <param name="member">The enum member.</param>
		/// <returns>The flags that compose <paramref name="member"/>'s value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
		public static IEnumerable<Enum> GetFlags([NotNull] this EnumMember member) { return member.GetFlags(); }

		/// <summary>
		/// Retrieves the <see cref="EnumMember"/>s of the flags that compose <paramref name="member"/>'s value.
		/// </summary>
		/// <param name="member">The enum member.</param>
		/// <returns>The <see cref="EnumMember"/>s of the flags that compose <paramref name="member"/>'s value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
		public static IEnumerable<EnumMember> GetFlagMembers([NotNull] this EnumMember member) { return member.GetFlagMembers(); }

		internal static IEnumInfo GetInfo([NotNull] Type enumType)
		{
			return __enumInfo.GetOrAdd(enumType, e =>
												{
													if (e.IsEnum)
													{
														Type closedEnumsType = typeof(EnumHelper<>).MakeGenericType(e);
														return (IEnumInfo)closedEnumsType.GetProperty("Info", Constants.BF_PUBLIC_NON_PUBLIC_STATIC)?.GetValue(null);
													}

													Type underlyingType = Nullable.GetUnderlyingType(e);
													if (underlyingType == null) throw new NotEnumTypeException();
													if (!underlyingType.IsEnum) throw new NotEnumTypeException(underlyingType);
													
													Type numericProviderType = GetNumericProviderType(underlyingType);
													return typeof(EnumInfo<,>).MakeGenericType(underlyingType, numericProviderType)
																			.CreateInstance<IEnumInfo>(e);
												});
		}

		internal static int CompareToInternal(EnumInfo info, Enum value, Enum other)
		{
			IEnumInfo enumInfo = info.Value;
			if (info.IsNullable)
			{
				if (value == null)
				{
					if (other == null) return 0;
					enumInfo.ToObject(other);
					return -1;
				}

				if (other == null)
				{
					enumInfo.ToObject(value);
					return 1;
				}
			}

			return enumInfo.Compare(value, other);
		}

		/// <summary>
		/// Registers a custom <see cref="EnumFormat" /> with the specified <see cref="EnumMember" /> formatter.
		/// </summary>
		/// <param name="enumMemberFormatter">The <see cref="EnumMember" /> formatter.</param>
		/// <returns>A custom <see cref="EnumFormat" /> that is registered with the specified <see cref="EnumMember" /> formatter.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="enumMemberFormatter" /> is <c>null</c>.</exception>
		internal static EnumFormat RegisterCustomEnumFormat([NotNull] Func<EnumMember, string> enumMemberFormatter)
		{
			Func<EnumMember, string>[] customEnumMemberFormatters = __customEnumMemberFormatters;
			Func<EnumMember, string>[] oldCustomEnumMemberFormatters;

			do
			{
				oldCustomEnumMemberFormatters = customEnumMemberFormatters;

				if (oldCustomEnumMemberFormatters == null)
				{
					customEnumMemberFormatters = new[]
												{
													enumMemberFormatter
												};
				}
				else
				{
					customEnumMemberFormatters = new Func<EnumMember, string>[oldCustomEnumMemberFormatters.Length + 1];
					oldCustomEnumMemberFormatters.CopyTo(customEnumMemberFormatters, 0);
					customEnumMemberFormatters[oldCustomEnumMemberFormatters.Length] = enumMemberFormatter;
				}
			}
			while ((customEnumMemberFormatters = Interlocked.CompareExchange(ref __customEnumMemberFormatters, customEnumMemberFormatters, oldCustomEnumMemberFormatters)) !=
					oldCustomEnumMemberFormatters);

			return (EnumFormat)(oldCustomEnumMemberFormatters?.Length ?? 0 + S_STARTING_CUSTOM_ENUM_FORMAT_VALUE);
		}

		internal static bool EnumFormatIsValid(EnumFormat format)
		{
			return format >= EnumFormat.DecimalValue &&
					format <= (EnumFormat)(__customEnumMemberFormatters.Length - 1 + S_STARTING_CUSTOM_ENUM_FORMAT_VALUE);
		}

		internal static string CustomEnumMemberFormat(EnumMember member, EnumFormat format)
		{
			return __customEnumMemberFormatters[(int)format - S_STARTING_CUSTOM_ENUM_FORMAT_VALUE](member);
		}

		[NotNull]
		internal static Type GetNumericProviderType([NotNull] Type underlyingType)
		{
			switch (underlyingType.AsTypeCode())
			{
				case TypeCode.SByte:
					return typeof(SByteNumericProvider);
				case TypeCode.Byte:
					return typeof(ByteNumericProvider);
				case TypeCode.Int16:
					return typeof(Int16NumericProvider);
				case TypeCode.UInt16:
					return typeof(UInt16NumericProvider);
				case TypeCode.Int32:
					return typeof(Int32NumericProvider);
				case TypeCode.UInt32:
					return typeof(UInt32NumericProvider);
				case TypeCode.Int64:
					return typeof(Int64NumericProvider);
				case TypeCode.UInt64:
					return typeof(UInt64NumericProvider);
				case TypeCode.Boolean:
					return typeof(BooleanNumericProvider);
				case TypeCode.Char:
					return typeof(CharNumericProvider);
			}

			throw new NotSupportedException($"Enum underlying type of {underlyingType} is not supported");
		}
	}

	public static class EnumHelper<TEnum>
		where TEnum : struct, Enum, IComparable
	{
		public static IEnumInfo<TEnum> Info { get; } = GetInfo();

		/// <summary>
		/// Retrieves the underlying type of <typeparamref name="TEnum" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns>The underlying type of <typeparamref name="TEnum" />.</returns>
		public static Type GetUnderlyingType() { return Info.UnderlyingType; }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s underlying type's <see cref="TypeCode" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s underlying type's <see cref="TypeCode" />.</returns>
		public static TypeCode GetTypeCode() { return Info.TypeCode; }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s member count.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s member count.</returns>
		public static int GetCount() { return Info.GetCount(); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s member count.
		/// The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><typeparamref name="TEnum" />'s member count.</returns>
		/// <exception cref="ArgumentException"><paramref name="selection" /> is an invalid value.</exception>
		public static int GetCount(EnumMemberSelection selection) { return Info.GetCount(selection); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members in increasing value order.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s members in increasing value order.</returns>
		public static IEnumerable<EnumMember<TEnum>> GetMembers() { return Info.GetMembers(); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members in increasing value order.
		/// The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><typeparamref name="TEnum" />'s members in increasing value order.</returns>
		/// <exception cref="ArgumentException"><paramref name="selection" /> is an invalid value.</exception>
		public static IEnumerable<EnumMember<TEnum>> GetMembers(EnumMemberSelection selection) { return Info.GetMembers(selection); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' names in increasing value order.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s members' names in increasing value order.</returns>
		public static IEnumerable<string> GetNames() { return Info.GetNames(); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' names in increasing value order.
		/// The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><typeparamref name="TEnum" />'s members' names in increasing value order.</returns>
		/// <exception cref="ArgumentException"><paramref name="selection" /> is an invalid value.</exception>
		public static IEnumerable<string> GetNames(EnumMemberSelection selection) { return Info.GetNames(selection); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' display names in increasing value order.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s members' display names in increasing value order.</returns>
		public static IEnumerable<string> GetDisplayNames() { return Info.GetDisplayNames(); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' display names in increasing value order.
		/// The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><typeparamref name="TEnum" />'s members' display names in increasing value order.</returns>
		/// <exception cref="ArgumentException"><paramref name="selection" /> is an invalid value.</exception>
		public static IEnumerable<string> GetDisplayNames(EnumMemberSelection selection) { return Info.GetDisplayNames(selection); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' values in increasing order.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns><typeparamref name="TEnum" />'s members' values in increasing order.</returns>
		public static TEnum[] GetValues() { return Info.GetValues(); }

		/// <summary>
		/// Retrieves <typeparamref name="TEnum" />'s members' values in increasing order.
		/// The parameter <paramref name="selection" /> indicates what members to include.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="selection">Indicates what members to include.</param>
		/// <returns><typeparamref name="TEnum" />'s members' values in increasing order.</returns>
		/// <exception cref="ArgumentException"><paramref name="selection" /> is an invalid value.</exception>
		public static TEnum[] GetValues(EnumMemberSelection selection) { return Info.GetValues(selection); }

		public static (TEnum Minimum, TEnum Maximum) GetBoundaries() { return GetValues().FirstAndLast(); }

		[NotNull]
		public static EnumRange<TEnum> GetRange()
		{
			return new EnumRange<TEnum>();
		}

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">
		/// Value to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />, <see cref="ushort" />
		/// ,
		/// <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <typeparamref name="TEnum" />,
		/// <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="value" /> is not a valid type.</exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(object value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">
		/// Value to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />, <see cref="ushort" />
		/// ,
		/// <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <typeparamref name="TEnum" />,
		/// <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value" /> is not a valid type
		/// -or-
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(object value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(sbyte value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(sbyte value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(byte value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(byte value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(short value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(short value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(ushort value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(ushort value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(int value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(int value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(uint value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(uint value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(long value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(long value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(ulong value) { return Info.ToObject(value); }

		/// <summary>
		/// Converts the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it doesn't
		/// overflow the
		/// underlying type. The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <returns>The specified <paramref name="value" /> converted to a <typeparamref name="TEnum" />.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="validation" /> is an invalid value
		/// -or-
		/// the result is invalid with the specified <paramref name="validation" />.
		/// </exception>
		/// <exception cref="OverflowException"><paramref name="value" /> is outside the underlying type's value range.</exception>
		public static TEnum ToObject(ulong value, EnumValidation validation) { return Info.ToObject(value, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">
		/// Value to try to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />,
		/// <see cref="ushort" />,
		/// <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <typeparamref name="TEnum" />,
		/// <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(object value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">
		/// Value to try to convert. Must be an <see cref="sbyte" />, <see cref="byte" />, <see cref="short" />,
		/// <see cref="ushort" />,
		/// <see cref="int" />, <see cref="uint" />, <see cref="long" />, <see cref="ulong" />, <typeparamref name="TEnum" />,
		/// <see cref="string" />, or Nullable of one of these.
		/// </param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(object value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(sbyte value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(sbyte value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(byte value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(byte value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(short value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(short value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(ushort value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(ushort value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(int value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(int value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(uint value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(uint value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(long value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(long value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryToObject(ulong value, out TEnum result) { return Info.TryToObject(value, out result); }

		/// <summary>
		/// Tries to convert the specified <paramref name="value" /> to a <typeparamref name="TEnum" /> while checking that it
		/// doesn't overflow the
		/// underlying type.  The parameter <paramref name="validation" /> specifies the validation to perform on the result.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">Value to try to convert.</param>
		/// <param name="validation">The validation to perform on the result.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> whose value is
		/// <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="validation" /> is an invalid value.</exception>
		public static bool TryToObject(ulong value, EnumValidation validation, out TEnum result) { return Info.TryToObject(value, out result, validation); }

		/// <summary>
		/// Retrieves the enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.
		/// Is case-sensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="name">The enum member name.</param>
		/// <returns>Enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		public static EnumMember<TEnum> GetMember(string name) { return Info.GetMember(name); }

		/// <summary>
		/// Retrieves the enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.
		/// The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="name">The enum member name.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>Enum member with the specified <paramref name="name" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
		public static EnumMember<TEnum> GetMember(string name, bool ignoreCase) { return Info.GetMember(name, ignoreCase); }

		/// <summary>
		/// Retrieves an enum member whose string representation using the specified <paramref name="formats" /> is
		/// <paramref name="value" /> if defined otherwise <c>null</c>.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member's string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Enum member represented by <paramref name="value" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static EnumMember<TEnum> GetMember(string value, params EnumFormat[] formats) { return Info.GetMember(value, false, formats); }

		/// <summary>
		/// Retrieves an enum member whose string representation using the specified <paramref name="formats" /> is
		/// <paramref name="value" /> if defined otherwise <c>null</c>.
		/// The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member's string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Enum member represented by <paramref name="value" /> if defined otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static EnumMember<TEnum> GetMember(string value, bool ignoreCase, params EnumFormat[] formats) { return Info.GetMember(value, ignoreCase, formats); }

		/// <summary>
		/// Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value" /> doesn't represent a member name or value of
		/// <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		/// <paramref name="value" /> is outside the range of <typeparamref name="TEnum" />'s
		/// underlying type.
		/// </exception>
		public static TEnum Parse(string value) { return Info.Parse(value); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value
		/// using the specified parsing enum formats.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		/// -or-
		/// <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		/// <paramref name="value" /> is outside the range of the underlying type of
		/// <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum Parse(string value, params EnumFormat[] formats) { return Info.Parse(value, false, formats); }

		/// <summary>
		/// Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value.
		/// The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value" /> doesn't represent a member name or value of
		/// <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		/// <paramref name="value" /> is outside the range of the underlying type of
		/// <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum Parse(string value, bool ignoreCase) { return Info.Parse(value, ignoreCase); }

		/// <summary>
		/// Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies if the operation
		/// is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		/// -or-
		/// <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		/// <paramref name="value" /> is outside the range of the underlying type of
		/// <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum Parse(string value, bool ignoreCase, params EnumFormat[] formats) { return Info.Parse(value, ignoreCase, formats); }

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		/// to its respective <typeparamref name="TEnum" /> value.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		/// by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParse(string value, out TEnum result) { return Info.TryParse(value, false, out result); }

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value
		/// using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		/// by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParse(string value, out TEnum result, params EnumFormat[] formats) { return Info.TryParse(value, false, out result, formats); }

		/// <summary>
		/// Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		/// to its respective <typeparamref name="TEnum" /> value.
		/// The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive. The return value
		/// indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		/// by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParse(string value, bool ignoreCase, out TEnum result) { return Info.TryParse(value, ignoreCase, out result); }

		/// <summary>
		/// Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		/// respective <typeparamref name="TEnum" /> value
		/// using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies whether the
		/// operation is case-insensitive.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		/// If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		/// by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParse(string value, bool ignoreCase, out TEnum result, params EnumFormat[] formats) { return Info.TryParse(value, ignoreCase, out result, formats); }

		public static bool IsValid(TEnum value) { return Info.IsValid(value); }

		public static bool IsValid(TEnum value, EnumValidation validation) { return Info.IsValid(value, validation); }

		public static bool IsDefined(TEnum value) { return Info.IsDefined(value); }

		public static TEnum Validate(TEnum value, string paramName) { return Info.Validate(value, paramName); }

		public static TEnum Validate(TEnum value, string paramName, EnumValidation validation) { return Info.Validate(value, paramName, validation); }

		public static string AsString(TEnum value) { return Info.AsString(value); }

		public static string AsString(TEnum value, string format) { return Info.AsString(value, format); }

		public static string AsString(TEnum value, EnumFormat format) { return Info.AsString(value, format); }

		public static string AsString(TEnum value, EnumFormat format0, EnumFormat format1) { return Info.AsString(value, format0, format1); }

		public static string AsString(TEnum value, EnumFormat format0, EnumFormat format1, EnumFormat format2) { return Info.AsString(value, format0, format1, format2); }

		public static string AsString(TEnum value, params EnumFormat[] formats) { return Info.AsString(value, formats); }

		public static string Format(TEnum value, string format) { return Info.Format(value, format); }

		public static string Format(TEnum value, params EnumFormat[] formats) { return Info.Format(value, formats); }

		public static object GetUnderlyingValue(TEnum value) { return Info.GetUnderlyingValue(value); }

		public static sbyte ToSByte(TEnum value) { return Info.ToSByte(value); }

		public static byte ToByte(TEnum value) { return Info.ToByte(value); }

		public static short ToInt16(TEnum value) { return Info.ToInt16(value); }

		public static ushort ToUInt16(TEnum value) { return Info.ToUInt16(value); }

		public static int ToInt32(TEnum value) { return Info.ToInt32(value); }

		public static uint ToUInt32(TEnum value) { return Info.ToUInt32(value); }

		public static long ToInt64(TEnum value) { return Info.ToInt64(value); }

		public static ulong ToUInt64(TEnum value) { return Info.ToUInt64(value); }

		public static int GetHashCode(TEnum value) { return Info.GetHashCode(value); }

		public static bool Equals(TEnum value, TEnum other) { return Info.Equals(value, other); }

		public static int CompareTo(TEnum value, TEnum other) { return Info.Compare(value, other); }

		public static string GetName(TEnum value) { return Info.GetName(value); }

		public static AttributeCollection GetAttributes(TEnum value) { return Info.GetAttributes(value); }

		public static EnumMember<TEnum> GetMember(TEnum value) { return Info.GetMember(value); }

		public static IEnumerable<TEnum> GetFlags(TEnum value) { return Info.GetFlags(value); }

		public static int GetFlagCount(TEnum value) { return Info.GetFlagCount(value); }

		public static int GetFlagCount(TEnum value, TEnum otherFlags) { return Info.GetFlagCount(value, otherFlags); }

		public static bool HasAnyFlags(TEnum value) { return Info.HasAnyFlags(value); }

		public static bool HasAnyFlags(TEnum value, TEnum otherFlags) { return Info.HasAnyFlags(value, otherFlags); }

		public static bool HasAllFlags(TEnum value) { return Info.HasAllFlags(value); }

		public static bool HasAllFlags(TEnum value, TEnum otherFlags) { return Info.HasAllFlags(value, otherFlags); }

		public static TEnum CommonFlags(TEnum value, TEnum otherFlags) { return Info.CommonFlags(value, otherFlags); }

		public static TEnum CombineFlags(TEnum value, TEnum otherFlags) { return Info.CombineFlags(value, otherFlags); }

		public static TEnum RemoveFlags(TEnum value, TEnum otherFlags) { return Info.RemoveFlags(value, otherFlags); }

		public static bool IsEnum => typeof(TEnum).IsEnum;

		/// <summary>
		///     Indicates if <typeparamref name="TEnum" /> is marked with the <see cref="FlagsAttribute" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns>Indication if <typeparamref name="TEnum" /> is marked with the <see cref="FlagsAttribute" />.</returns>
		public static bool IsFlagEnum() { return Info.IsFlagEnum; }

		/// <summary>
		///     Retrieves all the flags defined by <typeparamref name="TEnum" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns>All the flags defined by <typeparamref name="TEnum" />.</returns>
		public static TEnum GetAllFlags() { return Info.AllFlags; }

		/// <summary>
		///     Indicates whether <paramref name="value" /> is a valid flag combination of <typeparamref name="TEnum" />'s defined
		///     flags.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <returns>
		///     Indication of whether <paramref name="value" /> is a valid flag combination of <typeparamref name="TEnum" />'s
		///     defined flags.
		/// </returns>
		public static bool IsValidFlagCombination(TEnum value) { return Info.IsValidFlagCombination(value); }

		/// <summary>
		///     Retrieves the names of <paramref name="value" />'s flags delimited with commas or if empty returns the name of the
		///     zero flag if defined otherwise "0".
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <returns>
		///     The names of <paramref name="value" />'s flags delimited with commas or if empty returns the name of the zero flag
		///     if defined otherwise "0".
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </returns>
		public static string FormatFlags(TEnum value) { return Info.FormatFlags(value); }

		/// <summary>
		///     Retrieves <paramref name="value" />'s flags formatted with <paramref name="formats" /> and delimited with commas
		///     or if empty returns the zero flag formatted with <paramref name="formats" />.
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>
		///     <paramref name="value" />'s flags formatted with <paramref name="formats" /> and delimited with commas
		///     or if empty returns the zero flag formatted with <paramref name="formats" />.
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static string FormatFlags(TEnum value, params EnumFormat[] formats) { return Info.FormatFlags(value, null, formats); }

		/// <summary>
		///     Retrieves the names of <paramref name="value" />'s flags delimited with <paramref name="delimiter" /> or if empty
		///     returns the name of the zero flag if defined otherwise "0".
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <param name="delimiter">The delimiter to use to separate individual flags.</param>
		/// <returns>
		///     The names of <paramref name="value" />'s flags delimited with <paramref name="delimiter" /> or if empty returns the
		///     name of the zero flag if defined otherwise "0".
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </returns>
		public static string FormatFlags(TEnum value, string delimiter) { return Info.FormatFlags(value, delimiter); }

		/// <summary>
		///     Retrieves <paramref name="value" />'s flags formatted with <paramref name="formats" /> and delimited with
		///     <paramref name="delimiter" />
		///     or if empty returns the zero flag formatted with <paramref name="formats" />.
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <param name="delimiter">The delimiter to use to separate individual flags.</param>
		/// <param name="formats">The output formats to use.</param>
		/// <returns>
		///     <paramref name="value" />'s flags formatted with <paramref name="formats" /> and delimited with
		///     <paramref name="delimiter" />
		///     or if empty returns the zero flag formatted with <paramref name="formats" />.
		///     If <paramref name="value" /> is not a valid flag combination <c>null</c> is returned.
		/// </returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static string FormatFlags(TEnum value, string delimiter, params EnumFormat[] formats) { return Info.FormatFlags(value, delimiter, formats); }

		/// <summary>
		///     Retrieves the <see cref="EnumMember{TEnum}" />s of the flags that compose <paramref name="value" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <returns>The <see cref="EnumMember{TEnum}" />s of the flags that compose <paramref name="value" />.</returns>
		public static IEnumerable<EnumMember<TEnum>> GetFlagMembers(TEnum value) { return Info.GetFlagMembers(value); }

		/// <summary>
		///     Retrieves the flag count of <typeparamref name="TEnum" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <returns>The flag count of <typeparamref name="TEnum" />.</returns>
		public static int GetFlagCount() { return Info.GetFlagCount(); }

		/// <summary>
		///     Returns <paramref name="value" /> with all of it's flags toggled. Equivalent to the bitwise "xor" operator with
		///     <see cref="GetAllFlags" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <returns><paramref name="value" /> with all of it's flags toggled.</returns>
		public static TEnum ToggleFlags(TEnum value) { return Info.ToggleFlags(value); }

		/// <summary>
		///     Returns <paramref name="value" /> while toggling the flags that are in <paramref name="otherFlags" />. Equivalent
		///     to the bitwise "xor" operator.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The flags enum value.</param>
		/// <param name="otherFlags">The other flags enum value.</param>
		/// <returns><paramref name="value" /> while toggling the flags that are in <paramref name="otherFlags" />.</returns>
		public static TEnum ToggleFlags(TEnum value, TEnum otherFlags) { return Info.ToggleFlags(value, otherFlags); }

		/// <summary>
		///     Combines the flags of <paramref name="flag0" />, <paramref name="flag1" />, and <paramref name="flag2" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="flag0">The first flags enum value.</param>
		/// <param name="flag1">The second flags enum value.</param>
		/// <param name="flag2">The third flags enum value.</param>
		/// <returns>
		///     Combination of the flags of <paramref name="flag0" />, <paramref name="flag1" />, and
		///     <paramref name="flag2" />.
		/// </returns>
		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2) { return Info.CombineFlags(flag0, flag1, flag2); }

		/// <summary>
		///     Combines the flags of <paramref name="flag0" />, <paramref name="flag1" />, <paramref name="flag2" />, and
		///     <paramref name="flag3" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="flag0">The first flags enum value.</param>
		/// <param name="flag1">The second flags enum value.</param>
		/// <param name="flag2">The third flags enum value.</param>
		/// <param name="flag3">The fourth flags enum value.</param>
		/// <returns>
		///     Combination of the flags of <paramref name="flag0" />, <paramref name="flag1" />, <paramref name="flag2" />,
		///     and <paramref name="flag3" />.
		/// </returns>
		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3) { return Info.CombineFlags(flag0, flag1, flag2, flag3); }

		/// <summary>
		///     Combines the flags of <paramref name="flag0" />, <paramref name="flag1" />, <paramref name="flag2" />,
		///     <paramref name="flag3" />, and <paramref name="flag4" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="flag0">The first flags enum value.</param>
		/// <param name="flag1">The second flags enum value.</param>
		/// <param name="flag2">The third flags enum value.</param>
		/// <param name="flag3">The fourth flags enum value.</param>
		/// <param name="flag4">The fifth flags enum value.</param>
		/// <returns>
		///     Combination of the flags of <paramref name="flag0" />, <paramref name="flag1" />, <paramref name="flag2" />,
		///     <paramref name="flag3" />, and <paramref name="flag4" />.
		/// </returns>
		public static TEnum CombineFlags(TEnum flag0, TEnum flag1, TEnum flag2, TEnum flag3, TEnum flag4) { return Info.CombineFlags(flag0, flag1, flag2, flag3, flag4); }

		/// <summary>
		///     Combines all of the flags of <paramref name="flags" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="flags">The flags enum values.</param>
		/// <returns>Combination of all of the flags of <paramref name="flags" />.</returns>
		public static TEnum CombineFlags(params TEnum[] flags) { return Info.CombineFlags(flags); }

		/// <summary>
		///     Combines all of the flags of <paramref name="flags" />.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="flags">The flags enum values.</param>
		/// <returns>Combination of all of the flags of <paramref name="flags" />.</returns>
		public static TEnum CombineFlags(IEnumerable<TEnum> flags) { return Info.CombineFlags(flags); }

		/// <summary>
		///     Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member name or value of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of <typeparamref name="TEnum" />'s
		///     underlying type.
		/// </exception>
		public static TEnum ParseFlags(string value) { return Info.ParseFlags(value); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, params EnumFormat[] formats) { return Info.ParseFlags(value, false, null, formats); }

		/// <summary>
		///     Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value.
		///     The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member name or value of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, bool ignoreCase) { return Info.ParseFlags(value, ignoreCase); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies if the operation
		///     is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, bool ignoreCase, params EnumFormat[] formats) { return Info.ParseFlags(value, ignoreCase, null, formats); }

		/// <summary>
		///     Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> delimited
		///     with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member name or value of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of <typeparamref name="TEnum" />'s
		///     underlying type.
		/// </exception>
		public static TEnum ParseFlags(string value, string delimiter) { return Info.ParseFlags(value, false, delimiter); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> delimited with
		///     <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>A <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, string delimiter, params EnumFormat[] formats) { return Info.ParseFlags(value, false, delimiter, formats); }

		/// <summary>
		///     Converts the string representation of one or more member names or values of <typeparamref name="TEnum" /> delimited
		///     with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value.
		///     The parameter <paramref name="ignoreCase" /> specifies if the operation is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member name or value of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, bool ignoreCase, string delimiter) { return Info.ParseFlags(value, ignoreCase, delimiter); }

		/// <summary>
		///     Converts the string representation of one or more members or values of <typeparamref name="TEnum" /> delimited with
		///     <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies if the operation
		///     is case-insensitive.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>The <typeparamref name="TEnum" /> value that is represented by <paramref name="value" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="value" /> doesn't represent a member or value of <typeparamref name="TEnum" />
		///     -or-
		///     <paramref name="formats" /> contains an invalid value.
		/// </exception>
		/// <exception cref="OverflowException">
		///     <paramref name="value" /> is outside the range of the underlying type of
		///     <typeparamref name="TEnum" />.
		/// </exception>
		public static TEnum ParseFlags(string value, bool ignoreCase, string delimiter, params EnumFormat[] formats)
		{
			return Info.ParseFlags(value, ignoreCase, delimiter, formats);
		}

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		///     to its respective <typeparamref name="TEnum" /> value.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParseFlags(string value, out TEnum result) { return Info.TryParseFlags(value, false, null, out result); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParseFlags(string value, out TEnum result, params EnumFormat[] formats) { return Info.TryParseFlags(value, false, null, out result, formats); }

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		///     to its respective <typeparamref name="TEnum" /> value.
		///     The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive. The return value
		///     indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParseFlags(string value, bool ignoreCase, out TEnum result) { return Info.TryParseFlags(value, ignoreCase, null, out result); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" /> to its
		///     respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies whether the
		///     operation is case-insensitive.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParseFlags(string value, bool ignoreCase, out TEnum result, params EnumFormat[] formats)
		{
			return Info.TryParseFlags(value, ignoreCase, null, out result, formats);
		}

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		///     delimited with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParseFlags(string value, string delimiter, out TEnum result) { return Info.TryParseFlags(value, false, delimiter, out result); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" />
		///     delimited with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParseFlags(string value, string delimiter, out TEnum result, params EnumFormat[] formats)
		{
			return Info.TryParseFlags(value, false, delimiter, out result, formats);
		}

		/// <summary>
		///     Tries to convert the string representation of one or more member names or values of <typeparamref name="TEnum" />
		///     delimited with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value.
		///     The parameter <paramref name="ignoreCase" /> specifies whether the operation is case-insensitive. The return value
		///     indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum member names or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		public static bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result) { return Info.TryParseFlags(value, ignoreCase, delimiter, out result); }

		/// <summary>
		///     Tries to convert the string representation of one or more members or values of <typeparamref name="TEnum" />
		///     delimited with <paramref name="delimiter" /> to its respective <typeparamref name="TEnum" /> value
		///     using the specified parsing enum formats. The parameter <paramref name="ignoreCase" /> specifies whether the
		///     operation is case-insensitive.
		///     The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="value">The enum members or values' string representation.</param>
		/// <param name="ignoreCase">Indicates if the operation is case-insensitive.</param>
		/// <param name="delimiter">The delimiter used to separate individual flags.</param>
		/// <param name="result">
		///     If the conversion succeeds this contains a <typeparamref name="TEnum" /> value that is represented
		///     by <paramref name="value" />.
		/// </param>
		/// <param name="formats">The parsing enum formats.</param>
		/// <returns>Indication whether the conversion succeeded.</returns>
		/// <exception cref="ArgumentException"><paramref name="formats" /> contains an invalid value.</exception>
		public static bool TryParseFlags(string value, bool ignoreCase, string delimiter, out TEnum result, params EnumFormat[] formats)
		{
			return Info.TryParseFlags(value, ignoreCase, delimiter, out result, formats);
		}

		private static IEnumInfo<TEnum> GetInfo()
		{
			Type type = typeof(TEnum);
			if (!type.IsEnum) throw new NotEnumTypeException(type);
			Type underlyingType = Enum.GetUnderlyingType(type);
			Type numericProviderType = EnumHelper.GetNumericProviderType(underlyingType);
			return typeof(EnumInfo<,,>).MakeGenericType(type, underlyingType, numericProviderType)
														.CreateInstance<IEnumInfo<TEnum>>();
		}
	}
}