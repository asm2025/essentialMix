using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class IProducerConsumerCollectionExtension
	{
		public static void Clear<T>([NotNull] this IProducerConsumerCollection<T> thisValue)
		{
			while (thisValue.Count > 0 && thisValue.TryTake(out _))
			{
			}
		}
	}
}