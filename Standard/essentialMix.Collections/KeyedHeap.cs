using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public abstract class KeyedHeap<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : class, IKeyedNode<TKey, TValue>
	{
		internal int _version;

		private object _syncRoot;

		/// <inheritdoc />
		protected KeyedHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected KeyedHeap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		protected KeyedHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected KeyedHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		public int Count { get; protected internal set; }

		/// <inheritdoc />
		bool ICollection<TValue>.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <inheritdoc />
		public IEnumerator<TValue> GetEnumerator() { return Enumerate(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public abstract IEnumerableEnumerator<TValue> Enumerate(BreadthDepthTraversal method);

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate() { return Enumerate(BreadthDepthTraversal.BreadthFirst); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public abstract void Iterate(BreadthDepthTraversal method, [NotNull] Action<TNode> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Action<TNode> visitCallback)
		{
			Iterate(BreadthDepthTraversal.BreadthFirst, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public abstract void Iterate(BreadthDepthTraversal method, [NotNull] Func<TNode, bool> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(BreadthDepthTraversal.BreadthFirst, visitCallback);
		}
		#endregion

		/// <inheritdoc />
		public abstract TNode MakeNode(TValue value);

		/// <inheritdoc />
		public void Add(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			Add(MakeNode(value));
		}

		/// <inheritdoc />
		public abstract TNode Add(TNode node);

		/// <inheritdoc />
		public void Add(IEnumerable<TValue> enumerable)
		{
			foreach (TValue item in enumerable)
				Add(item);
		}

		/// <inheritdoc />
		public bool Remove(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			TNode node = Find(value);
			return node != null && Remove(node);
		}

		/// <inheritdoc />
		public abstract bool Remove(TNode node);

		/// <inheritdoc />
		public abstract void Clear();

		/// <inheritdoc />
		public abstract void DecreaseKey(TNode node, TKey newKey);

		/// <inheritdoc />
		public abstract TValue Value();

		/// <inheritdoc />
		TValue IHeap<TValue>.ExtractValue() { return ExtractValue().Value; }

		/// <inheritdoc />
		public abstract TNode ExtractValue();

		/// <inheritdoc />
		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++)
				ExtractValue();

			return Value();
		}

		/// <inheritdoc />
		public TNode Find(TValue value)
		{
			TNode node = null;
			Iterate(e =>
			{
				if (ValueComparer.Equals(e.Value, value)) node = e;
				return node == null;
			});
			return node;
		}

		/// <inheritdoc />
		public TNode FindByKey(TKey key)
		{
			TNode node = null;
			Iterate(e =>
			{
				if (Comparer.IsEqual(e.Key, key)) node = e;
				return node == null;
			});
			return node;
		}

		public virtual bool Equals(KeyedHeap<TNode, TKey, TValue> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !ValueComparer.Equals(other.ValueComparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<TValue> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<TValue> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (!ValueComparer.Equals(thisEnumerator.Current, otherEnumerator.Current)) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
		}

		/// <inheritdoc />
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = e.Value);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is TValue[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			* Catch the obvious case assignment will fail.
			* We can find all possible problems by doing the check though.
			* For example, if the element type of the Array is derived from T,
			* we can't figure out if we can successfully copy the element beforehand.
			*/
			array.Length.ValidateRange(index, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(TValue);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			Iterate(e => objects[index++] = e.Value);
		}

		/// <inheritdoc />
		public bool Contains(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return Find(value) != null;
		}

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);
	}
}
