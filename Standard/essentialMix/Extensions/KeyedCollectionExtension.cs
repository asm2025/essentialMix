using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class KeyedCollectionExtension
{
	public static TValue GetByIndex<TKey, TValue>([NotNull] this KeyedCollection<TKey, TValue> thisValue, int index) { return thisValue[index]; }
}