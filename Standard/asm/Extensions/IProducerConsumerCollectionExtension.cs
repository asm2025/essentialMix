using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class IProducerConsumerCollectionExtension
	{
		public static void Clear<T>([NotNull] this IProducerConsumerCollection<T> thisValue)
		{
			lock (thisValue)
			{
				if (thisValue.Count == 0) return;

				while (thisValue.TryTake(out _))
				{
					
				}
			}
		}
	}
}