using System.Collections.Specialized;
using System.ComponentModel;

namespace asm.Collections
{
	public interface IObservableKeyedCollection<TKey, TValue> : IKeyedCollection<TKey, TValue>, INotifyPropertyChanged, INotifyCollectionChanged
	{
	}
}