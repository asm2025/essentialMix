using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.Collections
{
	[Serializable]
	internal class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
	{
		private readonly IEqualityComparer<T> _comparer;

		public HashSetEqualityComparer()
			: this(EqualityComparer<T>.Default)
		{
		}

		public HashSetEqualityComparer(IEqualityComparer<T> comparer)
		{
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}

		// using _comparer to keep equals properties in tact; don't want to choose one of the comparers
		public bool Equals(HashSet<T> x, HashSet<T> y)
		{
			return HashSet<T>.HashSetEquals(x, y, _comparer);
		}

		public int GetHashCode(HashSet<T> obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}

		// Equals method for the comparer itself. 
		public override bool Equals(object obj)
		{
			return obj is HashSetEqualityComparer<T> comparer && _comparer == comparer._comparer;
		}

		public override int GetHashCode()
		{
			return _comparer.GetHashCode();
		}
	}
}