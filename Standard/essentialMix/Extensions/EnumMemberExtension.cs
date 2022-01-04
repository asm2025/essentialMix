using System;
using System.Collections.Generic;
using Other.TylerBrinkley.Enumeration;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class EnumMemberExtension
{
	/// <summary>
	/// Indicates whether <paramref name="member"/>'s value is a valid flag combination of its enum's defined values.
	/// </summary>
	/// <param name="member">The enum member.</param>
	/// <returns>Indication of whether <paramref name="member"/>'s value is a valid flag combination of its enum's defined values.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
	public static bool IsValidFlagCombination([NotNull] this EnumMember member) { return member.IsValidFlagCombination(); }

	/// <summary>
	/// Retrieves the flags that compose <paramref name="member"/>'s value.
	/// </summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="member">The enum member.</param>
	/// <returns>The flags that compose <paramref name="member"/>'s value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
	public static IEnumerable<TEnum> GetFlags<TEnum>([NotNull] this EnumMember<TEnum> member)
		where TEnum : struct, Enum, IComparable
	{
		return member.GetGenericFlags();
	}

	/// <summary>
	/// Retrieves the <see cref="EnumMember{TEnum}"/>s of the flags that compose <paramref name="member"/>'s value.
	/// </summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="member">The enum member.</param>
	/// <returns>The <see cref="EnumMember{TEnum}"/>s of the flags that compose <paramref name="member"/>'s value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
	public static IEnumerable<EnumMember<TEnum>> GetFlagMembers<TEnum>([NotNull] this EnumMember<TEnum> member)
		where TEnum : struct, Enum, IComparable
	{
		return member.GetGenericFlagMembers();
	}

	/// <summary>
	/// Indicates if <paramref name="member"/>'s value has any flags.
	/// </summary>
	/// <param name="member">The enum member.</param>
	/// <returns>Indication if <paramref name="member"/>'s has any flags.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
	public static bool HasAnyFlags([NotNull] this EnumMember member) { return member.HasAnyFlags(); }

	/// <summary>
	/// Indicates if <paramref name="member"/>'s value has all of the flags that are defined in its enum type.
	/// </summary>
	/// <param name="member">The enum member.</param>
	/// <returns>Indication if <paramref name="member"/> has all of the flags that are defined in its enum type.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
	public static bool HasAllFlags([NotNull] this EnumMember member) { return member.HasAllFlags(); }

	/// <summary>
	/// Retrieves the flag count of <paramref name="member"/>.
	/// </summary>
	/// <param name="member">The flags enum value.</param>
	/// <returns>The flag count of <paramref name="member"/>.</returns>
	public static int GetFlagCount([NotNull] this EnumMember member) { return member.GetFlagCount(); }
}