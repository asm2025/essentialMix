using Other.MarcGravell.Nullable;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class NonNullableExtension
{
	public static NonNullable<T> AsNonNullable<T>([NotNull] this T thisValue)
		where T : class
	{
		return (NonNullable<T>)thisValue;
	}
}