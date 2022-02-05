using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface IBoundList<T> : IList<T>
{
	int Capacity { get; set; }
	int Limit { get; }

	void AddRange([NotNull] IEnumerable<T> enumerable);
	void InsertRange(int index, [NotNull] IEnumerable<T> enumerable);
	void RemoveRange(int index, int count);
	int RemoveAll([NotNull] Predicate<T> match);
	int IndexOf(T item, int index);
	int IndexOf(T item, int index, int count);
	int LastIndexOf(T item);
	int LastIndexOf(T item, int index);
	int LastIndexOf(T item, int index, int count);
	T Find([NotNull] Predicate<T> match);
	T FindLast([NotNull] Predicate<T> match);
	IEnumerable<T> FindAll([NotNull] Predicate<T> match);
	int FindIndex([NotNull] Predicate<T> match);
	int FindIndex(int startIndex, [NotNull] Predicate<T> match);
	int FindLastIndex([NotNull] Predicate<T> match);
	int FindLastIndex(int startIndex, [NotNull] Predicate<T> match);
	int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match);
	IEnumerable<T> GetRange(int index, int count);
}