using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public virtual void Swap(TNode other)
		{
			(other.Value, Value) = (Value, other.Value);
		}

		public static implicit operator T([NotNull] BinaryNodeBase<TNode, T> node) { return node.Value; }
	}
}