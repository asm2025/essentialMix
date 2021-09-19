﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Key} = {Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class KeyedBinaryNode<TNode, TKey, TValue> : IKeyedNode<TKey, TValue>
		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
	{
		protected KeyedBinaryNode(TValue value)
		{
			Value = value;
		}

		/// <inheritdoc />
		[NotNull]
		public abstract TKey Key { get; set; }

		/// <inheritdoc />
		public TValue Value { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public virtual string ToString(int level)
		{
			return $"{Key} = {Value} :L{level}";
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap([NotNull] TNode other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
		}

		public static implicit operator TValue([NotNull] KeyedBinaryNode<TNode, TKey, TValue> node) { return node.Value; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class KeyedBinaryNode<TKey, TValue> : KeyedBinaryNode<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public KeyedBinaryNode([NotNull] TKey key, TValue value)
			: base(value)
		{
			_key = key;
		}

		/// <inheritdoc />
		public override TKey Key
		{
			get => _key;
			set => _key = value;
		}
	}

	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class KeyedBinaryNode<T> : KeyedBinaryNode<KeyedBinaryNode<T>, T, T>
	{
		/// <inheritdoc />
		public KeyedBinaryNode(T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public override T Key
		{
			get => Value;
			set => Value = value;
		}

		public override string ToString(int level)
		{
			return $"{Value} :L{level}";
		}
	}
}