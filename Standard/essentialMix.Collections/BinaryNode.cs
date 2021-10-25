using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
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

		public override string ToString(int level)
		{
			return $"{Value} :L{level}";
		}
	}
}