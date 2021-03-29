using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace essentialMix.Collections
{
	[Serializable]
	public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback, INotifyPropertyChanged, INotifyCollectionChanged
	{

	}
}