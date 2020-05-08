using System;
using System.Collections.Generic;
using asm.Patterns.Collections;

namespace asm.Collections
{
	public interface IGraph<T> : IDictionary<T, ISet<T>>
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
	{
		void AddEdge(T x, T y);

		ICollection<T> Search(T start, TraverseMethod algorithm, Action<T> preVisit = null);
		GraphSnapshot<T> GetSnapshot(T start, TraverseMethod algorithm);
	}
}