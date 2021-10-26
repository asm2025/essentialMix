﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_HeapDebugView<,>))]
	public abstract class Heap<TNode, T> : IBinaryHeap<TNode, T>, ICollection<T>, IReadOnlyCollection<T>, ICollection
		where TNode : class, ITreeNode<TNode, T>
	{
		internal int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		protected Heap()
			: this((IComparer<T>)null)
		{
		}

		protected Heap(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		protected Heap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected Heap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		public IComparer<T> Comparer { get; }

		protected internal TNode Head { get; set; }

		public int Count { get; protected internal set; }

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

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
		public IEnumerator<T> GetEnumerator() { return Enumerate(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public abstract IEnumerableEnumerator<T> Enumerate(TNode root, BreadthDepthTraversal method);

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate() { return Enumerate(BreadthDepthTraversal.BreadthFirst); }
		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(BreadthDepthTraversal method) { return Enumerate(Head, method); }
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TNode root) { return Enumerate(root, BreadthDepthTraversal.BreadthFirst); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public abstract void Iterate(TNode root, BreadthDepthTraversal method, [NotNull] Action<TNode> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Action<TNode> visitCallback) { Iterate(Head, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback) { Iterate(root, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(BreadthDepthTraversal method, [NotNull] Action<TNode> visitCallback) { Iterate(Head, method, visitCallback); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public abstract void Iterate(TNode root, BreadthDepthTraversal method, [NotNull] Func<TNode, bool> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Func<TNode, bool> visitCallback) { Iterate(Head, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(BreadthDepthTraversal method, [NotNull] Func<TNode, bool> visitCallback) { Iterate(Head, method, visitCallback); }
		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback) { Iterate(root, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		#endregion

		/// <inheritdoc />
		public abstract TNode MakeNode(T value);

		/// <inheritdoc />
		public void Add(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			Add(MakeNode(value));
		}

		/// <inheritdoc />
		public abstract TNode Add(TNode node);

		/// <inheritdoc />
		public void Add(IEnumerable<T> enumerable)
		{
			foreach (T item in enumerable)
				Add(item);
		}

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			TNode node = Find(value);
			return node != null && Remove(node);
		}

		/// <inheritdoc />
		public abstract bool Remove(TNode node);

		/// <inheritdoc cref="ICollection{T}" />
		public abstract void Clear();

		/// <inheritdoc />
		public abstract void DecreaseKey(TNode node, T newValue);

		/// <inheritdoc />
		public abstract T Value();

		/// <inheritdoc />
		public T ExtractValue() { return ExtractNode().Value; }

		/// <inheritdoc />
		public abstract TNode ExtractNode();

		/// <inheritdoc />
		public T ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++)
				ExtractNode();

			return Value();
		}

		/// <inheritdoc />
		public TNode Find(T value)
		{
			TNode node = null;
			Iterate(e =>
			{
				if (Comparer.Compare(e.Value, value) == 0) node = e;
				return node == null;
			});
			return node;
		}

		public virtual bool Equals(Heap<TNode, T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<T> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<T> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (Comparer.Compare(thisEnumerator.Current, otherEnumerator.Current) != 0) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
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

			if (array is T[] tArray)
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
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			Iterate(e => objects[index++] = e.Value);
		}

		/// <inheritdoc />
		public bool Contains(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return Find(value) != null;
		}

		protected abstract int Compare([NotNull] T x, [NotNull] T y);
	}

	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_HeapDebugView<,,>))]
	public abstract class Heap<TNode, TKey, TValue> : IBinaryHeap<TNode, TKey, TValue>, ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : class, ITreeNode<TNode, TKey, TValue>
	{
		internal int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		protected Heap()
			: this((IComparer<TKey>)null)
		{
		}

		protected Heap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		protected Heap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected Heap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		protected internal TNode Head { get; set; }

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
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public abstract IEnumerableEnumerator<TValue> Enumerate(TNode root, BreadthDepthTraversal method);

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate() { return Enumerate(BreadthDepthTraversal.BreadthFirst); }
		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(BreadthDepthTraversal method) { return Enumerate(Head, method); }
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode root) { return Enumerate(root, BreadthDepthTraversal.BreadthFirst); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public abstract void Iterate(TNode root, BreadthDepthTraversal method, [NotNull] Action<TNode> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Action<TNode> visitCallback) { Iterate(Head, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback) { Iterate(root, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(BreadthDepthTraversal method, [NotNull] Action<TNode> visitCallback) { Iterate(Head, method, visitCallback); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public abstract void Iterate(TNode root, BreadthDepthTraversal method, [NotNull] Func<TNode, bool> visitCallback);

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Func<TNode, bool> visitCallback) { Iterate(Head, BreadthDepthTraversal.BreadthFirst, visitCallback); }
		public void Iterate(BreadthDepthTraversal method, [NotNull] Func<TNode, bool> visitCallback) { Iterate(Head, method, visitCallback); }
		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback) { Iterate(root, BreadthDepthTraversal.BreadthFirst, visitCallback); }
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

		/// <inheritdoc cref="ICollection{T}" />
		public abstract void Clear();

		/// <inheritdoc />
		public abstract void DecreaseKey(TNode node, TKey newKey);

		/// <inheritdoc />
		public abstract TValue Value();

		/// <inheritdoc />
		public TValue ExtractValue() { return ExtractNode().Value; }

		/// <inheritdoc />
		public abstract TNode ExtractNode();

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

		public virtual bool Equals(Heap<TNode, TKey, TValue> other)
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
			if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));
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