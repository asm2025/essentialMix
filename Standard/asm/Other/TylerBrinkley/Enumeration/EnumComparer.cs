using System;
using asm.Comparers;
using asm.Helpers;

namespace asm.Other.TylerBrinkley.Enumeration
{
	/// <summary>
	/// An efficient type-safe enum comparer which doesn't box the values.
	/// </summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	public sealed class EnumComparer<TEnum> : GenericComparer<TEnum>
		where TEnum : struct, Enum, IComparable
	{
		public new static EnumComparer<TEnum> Default { get; } = new EnumComparer<TEnum>();

		private static readonly IEnumInfo<TEnum> INFO = EnumHelper<TEnum>.Info;

		public EnumComparer()
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Compares <paramref name="x" /> to <paramref name="y" /> without boxing the values.
		/// </summary>
		/// <param name="x">The first enum value.</param>
		/// <param name="y">The second enum value.</param>
		/// <returns>
		/// 1 if <paramref name="x" /> is greater than <paramref name="y" />, 0 if <paramref name="x" /> equals
		/// <paramref name="y" />,
		/// and -1 if <paramref name="x" /> is less than <paramref name="y" />.
		/// </returns>
		public override int Compare(TEnum x, TEnum y) { return INFO.Compare(x, y); }

		/// <inheritdoc />
		/// <summary>
		/// Indicates if <paramref name="x" /> equals <paramref name="y" /> without boxing the values.
		/// </summary>
		/// <param name="x">The first enum value.</param>
		/// <param name="y">The second enum value.</param>
		/// <returns>Indication if <paramref name="x" /> equals <paramref name="y" /> without boxing the values.</returns>
		public override bool Equals(TEnum x, TEnum y) { return INFO.Equals(x, y); }

		/// <inheritdoc />
		/// <summary>
		/// Retrieves a hash code for <paramref name="obj" /> without boxing the value.
		/// </summary>
		/// <param name="obj">The enum value.</param>
		/// <returns>Hash code for <paramref name="obj" /> without boxing the value.</returns>
		public override int GetHashCode(TEnum obj) { return INFO.GetHashCode(obj); }
	}
}