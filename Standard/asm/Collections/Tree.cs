using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Microsoft.Collections;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}, Count = {Count}")]
	[Serializable]
	public class Tree<T> : ListBase<Tree<T>>
	{
		/// <inheritdoc />
		public Tree() 
		{
		}

		/// <inheritdoc />
		public Tree(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public Tree([NotNull] IEnumerable<Tree<T>> collection)
			: base(collection)
		{
		}
		
		public T Value { get; set; }
		
		public bool IsLeaf => Count == 0;
	}

	[DebuggerDisplay("{Key} [{Value}], Count = {Count}")]
	[Serializable]
	public class Tree<TKey, TValue> : KeyedDictionaryBase<TKey, Tree<TKey, TValue>>
	{
		/// <inheritdoc />
		public Tree() 
		{
		}

		/// <inheritdoc />
		public Tree(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public Tree([NotNull] IEnumerable<Tree<TKey, TValue>> collection)
			: base(collection)
		{
		}

		/// <inheritdoc />
		public Tree([NotNull] IEnumerable<Tree<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected Tree(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		[NotNull]
		public KeyedDictionaryBase<TKey, Tree<TKey, TValue>> Children => this;

		public TKey Key { get; set; }
		
		public TValue Value { get; set; }
		
		public bool IsLeaf => Count == 0;

		/// <inheritdoc />
		protected override TKey GetKeyForItem(Tree<TKey, TValue> item) { return item.Key; }
	}
}
