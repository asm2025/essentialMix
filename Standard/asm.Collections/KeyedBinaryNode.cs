using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Key} = {Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class KeyedBinaryNode<TNode, TKey, TValue> : IKeyedNode<TKey, TValue>
		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
	{
		protected KeyedBinaryNode([NotNull] TValue value)
		{
			Value = value;
		}

		[NotNull]
		public abstract TKey Key { get; set; }

		[NotNull]
		public TValue Value { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal virtual string ToString(int level)
		{
			return $"{Key} = {Value} :L{level}";
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class KeyedBinaryNode<TKey, TValue> : KeyedBinaryNode<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public KeyedBinaryNode([NotNull] TKey key, [NotNull] TValue value)
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
		public KeyedBinaryNode([NotNull] T value)
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

		internal override string ToString(int level)
		{
			return $"{Value} :L{level}";
		}
	}
}