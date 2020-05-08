using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}, Count = {Count}")]
	[Serializable]
	public class Tree<T> : Microsoft.Collections.List<Tree<T>>
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

		public Tree<T> Parent { get; private set; }
		
		public T Value { get; set; }

		public bool IsRoot => Parent == null;
		
		public bool IsLeaf => Count == 0;

		/// <inheritdoc />
		protected override void OnInserted(int index, [NotNull] Tree<T> item)
		{
			base.OnInserted(index, item);
			item.Parent = this;
		}

		/// <inheritdoc />
		protected override void OnRemoved(int index, [NotNull] Tree<T> item)
		{
			base.OnRemoved(index, item);
			if (ReferenceEquals(this, item.Parent)) item.Parent = null;
		}

		/// <inheritdoc />
		protected override void OnRemoving(int index, int count)
		{
			for (int i = index; i < count; i++)
			{
				Tree<T> item = Items[i];
				if (ReferenceEquals(this, item.Parent)) item.Parent = null;
			}

			base.OnRemoving(index, count);
		}
	}

	[DebuggerDisplay("{Key} [{Value}], Count = {Count}")]
	[Serializable]
	public class Tree<TKey, TValue> : KeyedCollection<TKey, Tree<TKey, TValue>>
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

		public Tree<TKey, TValue> Parent { get; private set; }

		[NotNull]
		public KeyedCollection<TKey, Tree<TKey, TValue>> Children => this;

		public TKey Key { get; set; }
		
		public TValue Value { get; set; }

		public bool IsRoot => Parent == null;
		
		public bool IsLeaf => Count == 0;

		/// <inheritdoc />
		protected override TKey GetKeyForItem(Tree<TKey, TValue> item) { return item.Key; }

		/// <inheritdoc />
		protected override void OnInserted(int index, [NotNull] Tree<TKey, TValue> item)
		{
			item.Parent = this;
			base.OnInserted(index, item);
		}

		/// <inheritdoc />
		protected override void OnRemoved(int index, [NotNull] Tree<TKey, TValue> item)
		{
			base.OnRemoved(index, item);
			if (ReferenceEquals(this, item.Parent)) item.Parent = null;
		}

		/// <inheritdoc />
		protected override void OnClearing()
		{
			foreach (Tree<TKey, TValue> item in Items)
			{
				if (ReferenceEquals(this, item.Parent)) item.Parent = null;
			}
			base.OnClearing();
		}
	}
}
