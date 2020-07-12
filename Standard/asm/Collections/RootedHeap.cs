using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerTypeProxy(typeof(asm_RootedHeapDebugView<,,>))]
	[Serializable]
	public abstract class RootedHeap<TNode, TKey, TValue> : KeyedHeap<TNode, TKey, TValue>
		where TNode : class, IKeyedNode<TKey, TValue>
	{
		/// <inheritdoc />
		protected RootedHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected RootedHeap(IComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected RootedHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected RootedHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(enumerable, comparer)
		{
		}

		protected internal TNode Head { get; set; }

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public abstract IEnumerableEnumerator<TValue> Enumerate(TNode root, BreadthDepthTraverse method);

		#region Enumerate overloads
		/// <inheritdoc />
		public sealed override IEnumerableEnumerator<TValue> Enumerate(BreadthDepthTraverse method) { return Enumerate(Head, method); }
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode root) { return Enumerate(root, BreadthDepthTraverse.BreadthFirst); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public abstract void Iterate(TNode root, BreadthDepthTraverse method, [NotNull] Action<TNode> visitCallback);

		#region Iterate overloads - visitCallback action
		/// <inheritdoc />
		public sealed override void Iterate(BreadthDepthTraverse method, Action<TNode> visitCallback)
		{
			Iterate(Head, method, visitCallback);
		}

		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, BreadthDepthTraverse.BreadthFirst, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public abstract void Iterate(TNode root, BreadthDepthTraverse method, [NotNull] Func<TNode, bool> visitCallback);

		#region Iterate overloads - visitCallback action
		/// <inheritdoc />
		public sealed override void Iterate(BreadthDepthTraverse method, Func<TNode, bool> visitCallback)
		{
			Iterate(Head, method, visitCallback);
		}

		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, BreadthDepthTraverse.BreadthFirst, visitCallback);
		}
		#endregion
	}
}