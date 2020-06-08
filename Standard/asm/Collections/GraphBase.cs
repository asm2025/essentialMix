using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	// https://www.researchgate.net/publication/2349751_Design_And_Implementation_Of_A_Generic_Graph_Container_In_Java
	// https://www.lri.fr/~filliatr/ftp/publis/ocamlgraph-tfp-8.pdf
	// https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.1944&rep=rep1&type=pdf
	// https://gist.github.com/kevinmorio/f7102c5094aa748503f9
	public abstract class GraphBase<TNode, T> : KeyedDictionaryBase<T, TNode>
		where TNode : GraphNodeBase<TNode, T>
	{
		/// <inheritdoc />
		protected GraphBase()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected GraphBase(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected GraphBase([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected GraphBase([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphBase(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public string Label { get; set; }

		/// <inheritdoc />
		protected override void Insert(T key, TNode value, bool add) { throw new NotSupportedException(); }

		/// <inheritdoc />
		public override bool RemoveByKey(T key) { throw new NotSupportedException(); }

		/// <inheritdoc />
		public override void Clear() { throw new NotSupportedException(); }

		/// <inheritdoc />
		protected override T GetKeyForItem(TNode item) { return item.Value; }
	}
}