using System.Collections.Generic;

namespace essentialMix.Collections;

public interface IReadOnlySet<T> : IReadOnlyCollection<T>
{
	bool Contains(T item);
	void CopyTo(T[] array);
	void CopyTo(T[] array, int arrayIndex);
	bool IsSubsetOf(IEnumerable<T> other);
	bool IsSupersetOf(IEnumerable<T> other);
	bool IsProperSupersetOf(IEnumerable<T> other);
	bool IsProperSubsetOf(IEnumerable<T> other);
	bool Overlaps(IEnumerable<T> other);
	bool SetEquals(IEnumerable<T> other);
}