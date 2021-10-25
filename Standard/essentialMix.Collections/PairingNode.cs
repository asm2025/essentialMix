using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("{Key} = {Value}")]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class PairingNode<TKey, TValue> : PairingNodeBase<PairingNode<TKey, TValue>, TValue>, ISiblingNode<PairingNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public PairingNode([NotNull] TKey key, TValue value)
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
			return $"{Key} = {Value} :L{level}";
		}

		/// <inheritdoc />
		public override void Swap(PairingNode<TKey, TValue> other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class PairingNode<T> : PairingNodeBase<PairingNode<T>, T>
	{
		/// <inheritdoc />
		public PairingNode(T value)
			: base(value)
		{
		}

		public override string ToString(int level)
		{
			return $"{Value} :L{level}";
		}

		/// <inheritdoc />
		public override void Swap(PairingNode<T> other)
		{
			(other.Value, Value) = (Value, other.Value);
		}
	}
}