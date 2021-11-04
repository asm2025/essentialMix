using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("{Value}")]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class BinaryNodeBase<TNode, T> : ITreeNode<TNode, T>
		where TNode : BinaryNodeBase<TNode, T>
	{
		protected BinaryNodeBase(T value)
		{
			Value = value;
		}

		/// <inheritdoc />
		public T Value { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public virtual string ToString(int level)
		{
			return $"{Value} :L{level}";
		}

		public abstract void Swap(TNode other);

		public static implicit operator T([NotNull] BinaryNodeBase<TNode, T> node) { return node.Value; }
	}

	[Serializable]
	[DebuggerDisplay("{Key} = {Value}")]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinaryNode<TKey, TValue> : BinaryNodeBase<BinaryNode<TKey, TValue>, TValue>, ITreeNode<BinaryNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public BinaryNode([NotNull] TKey key, TValue value)
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

		public override string ToString(int level)
		{
			return $"{Key} = {Value} :L{level}";
		}

		/// <inheritdoc />
		public override void Swap(BinaryNode<TKey, TValue> other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinaryNode<T> : BinaryNodeBase<BinaryNode<T>, T>
	{
		/// <inheritdoc />
		public BinaryNode(T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		public override void Swap(BinaryNode<T> other)
		{
			(other.Value, Value) = (Value, other.Value);
		}
	}
}