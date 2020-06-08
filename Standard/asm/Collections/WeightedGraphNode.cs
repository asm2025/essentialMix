using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}, Count = {Count}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class WeightedGraphNode<TNode, T> : GraphNodeBase<TNode, T>, IDictionary<TNode, int>, ICollection<TNode>
		where TNode : WeightedGraphNode<TNode, T>
	{
		private IDictionary<TNode, int> _adjacencyList;

		protected WeightedGraphNode([NotNull] T value)
			: base(value)
		{
		}

		[NotNull]
		protected IDictionary<TNode, int> AdjacencyList => _adjacencyList ??= new Dictionary<TNode, int>();

		/// <inheritdoc />
		public int this[TNode node]
		{
			get => AdjacencyList[node]; 
			set => AdjacencyList[node] = value;
		}

		/// <inheritdoc cref="ICollection{TNode}" />
		public int Count => AdjacencyList.Count;

		/// <inheritdoc />
		bool ICollection<KeyValuePair<TNode, int>>.IsReadOnly => AdjacencyList.IsReadOnly;

		/// <inheritdoc />
		bool ICollection<TNode>.IsReadOnly => AdjacencyList.IsReadOnly;

		/// <inheritdoc />
		public ICollection<TNode> Keys => AdjacencyList.Keys;

		/// <inheritdoc />
		public ICollection<int> Values => AdjacencyList.Values;

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<TNode, int>> GetEnumerator() { return AdjacencyList.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator() { return AdjacencyList.Keys.GetEnumerator(); }

		/// <inheritdoc />
		public void Add(TNode to)
		{
			if (ReferenceEquals(to, null)) throw new ArgumentNullException(nameof(to));
			Add(to, 0);
		}

		/// <inheritdoc />
		public void Add(KeyValuePair<TNode, int> to)
		{
			if (ReferenceEquals(to.Key, null)) throw new ArgumentNullException(nameof(to.Key));
			Add(to.Key, to.Value);
		}

		public abstract void Add(TNode to, int weight);

		/// <inheritdoc />
		public bool Remove(KeyValuePair<TNode, int> to) { return AdjacencyList.Remove(to); }

		/// <inheritdoc cref="IDictionary{TKey,TValue}" />
		public abstract bool Remove(TNode to);

		/// <inheritdoc cref="IDictionary{TKey,TValue}" />
		public abstract void Clear();

		/// <inheritdoc />
		public bool Contains(TNode node) { return node != null && AdjacencyList.ContainsKey(node); }

		/// <inheritdoc />
		public bool Contains(KeyValuePair<TNode, int> node) { return AdjacencyList.Contains(node); }

		/// <inheritdoc />
		bool IDictionary<TNode, int>.ContainsKey(TNode node) { return AdjacencyList.ContainsKey(node); }

		/// <inheritdoc />
		public bool TryGetValue(TNode node, out int value) { return AdjacencyList.TryGetValue(node, out value); }

		public int GetOrAdd([NotNull] TNode node)
		{
			if (AdjacencyList.TryGetValue(node, out int value)) return value;
			value = 0;
			AdjacencyList.Add(node, value);
			return value;
		}

		/// <inheritdoc />
		public void CopyTo(TNode[] array, int arrayIndex) { AdjacencyList.Keys.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<TNode, int>[] array, int arrayIndex) { AdjacencyList.CopyTo(array, arrayIndex); }
	}
}