using JetBrains.Annotations;
using asm.Patterns.Nullable;

namespace asm.Extensions
{
	public static class NonNullableExtension
	{
		public static NonNullable<T> AsNonNullable<T>([NotNull] this T thisValue)
			where T : class
		{
			return (NonNullable<T>)thisValue;
		}
	}
}