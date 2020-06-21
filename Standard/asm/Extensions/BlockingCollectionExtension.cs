using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class BlockingCollectionExtension
	{
		public static void Clear<T>([NotNull] this BlockingCollection<T> thisValue)
		{
			if (thisValue.Count == 0) return;

			foreach (T _ in thisValue.GetConsumingEnumerable())
			{
			}
		}
	}
}