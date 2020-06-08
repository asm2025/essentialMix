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
	public abstract class GraphNode<TNode, T> : GraphNodeBase<TNode, T>, ICollection<TNode>
		where TNode : GraphNode<TNode, T>
	{
		private ISet<TNode> _adjacencyList;

		protected GraphNode([NotNull] T value)
			: base(value)
		{
		}

		[NotNull]
		protected ISet<TNode> AdjacencyList => _adjacencyList ??= new HashSet<TNode>();

		/// <inheritdoc cref="ICollection{TNode}" />
		public int Count => AdjacencyList.Count;

		/// <inheritdoc />
		bool ICollection<TNode>.IsReadOnly => AdjacencyList.IsReadOnly;

		/// <inheritdoc />
		public IEnumerator<TNode> GetEnumerator() { return AdjacencyList.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public abstract void Add(TNode to);

		/// <inheritdoc />
		public abstract bool Remove(TNode to);

		/// <inheritdoc />
		public abstract void Clear();

		/// <inheritdoc />
		public bool Contains(TNode item) { return item != null && AdjacencyList.Contains(item); }

		/// <inheritdoc />
		public void CopyTo(TNode[] array, int arrayIndex) { AdjacencyList.CopyTo(array, arrayIndex); }
	}
}