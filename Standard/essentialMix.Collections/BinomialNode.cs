using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("{Key} = {Value}")]
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

		/// <inheritdoc />
		public override string ToString(int level)
		{
			return $"{Key} = {Value} :D{Degree}L{level}";
		}

		/// <inheritdoc />
		public override void Swap(BinomialNode<TKey, TValue> other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
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

		/// <inheritdoc />
		public override void Swap(BinomialNode<T> other)
		{
			(other.Value, Value) = (Value, other.Value);
		}
	}
}