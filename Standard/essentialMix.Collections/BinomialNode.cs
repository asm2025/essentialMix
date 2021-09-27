using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<TKey, TValue> : BinomialNodeBase<BinomialNode<TKey, TValue>, TValue>, ISiblingNode<BinomialNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public BinomialNode([NotNull] TKey key, TValue value)
			: base(value)
		{
			_key = key;
		}

		/// <inheritdoc />
		public TKey Key
		{
			get => _key;
			set => _key = value;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<T> : BinomialNodeBase<BinomialNode<T>, T>
	{
		/// <inheritdoc />
		public BinomialNode(T value)
			: base(value)
		{
		}

		public override string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}
	}
}