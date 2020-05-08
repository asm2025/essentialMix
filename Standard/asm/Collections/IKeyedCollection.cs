using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace asm.Collections
{
	public interface IKeyedCollection<TKey, TValue> : IList<TValue>, IList, ICollection<TValue>, ICollection, IDictionary<TKey, TValue>, IDictionary, IEnumerable<TValue>, IEnumerable, IReadOnlyKeyedCollection<TKey, TValue>, ISerializable, IDeserializationCallback
	{
	}
}