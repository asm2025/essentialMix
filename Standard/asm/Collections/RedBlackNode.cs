using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class RedBlackNode<T> : LinkedBinaryNode<RedBlackNode<T>, T>
	{
		internal RedBlackNode(T value)
			: base(value)
		{
		}

		/// <summary>
		/// True means Red and False = no color or Black
		/// </summary>
		public bool Color { get; internal set; } = true;

		/// <inheritdoc />
		protected internal override string ToString(int depth)
		{
			return $"{Value} {(Color ? 'R' : 'B')}";
		}

		public void SwapColor([NotNull] RedBlackNode<T> other)
		{
			bool tmp = other.Color;
			other.Color = Color;
			Color = tmp;
		}
	}
}