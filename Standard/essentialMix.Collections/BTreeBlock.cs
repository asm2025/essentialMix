using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	//[DebuggerDisplay("{Degree}, Count = {Count}")]
	//[Serializable]
	//[StructLayout(LayoutKind.Sequential)]
	//public abstract class BTreeBlock<TBlock, TEntry, TKey, TValue>
	//	where TBlock : BTreeBlock<TBlock, TEntry, TKey, TValue>
	//{
	//	protected BTreeBlock(int degree)
	//		: this(degree, default(T))
	//	{
	//	}

	//	protected BTreeBlock(int degree, T value)
	//	{
	//		Degree = degree;
	//		Value = value;
	//	}

	//	public T Value { get; set; }

	//	public int Degree { get; }
	
	//	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	//	public void Swap([NotNull] TNode other)
	//	{
	//		(other.Value, Value) = (Value, other.Value);
	//	}

	//	public static implicit operator TValue([NotNull] KeyedBinaryNode<TNode, TKey, TValue> node) { return node.Value; }
	//}
}