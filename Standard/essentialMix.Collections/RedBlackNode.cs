using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Value} :C{ColorC}")]
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

		private char ColorC =>
			Color
				? 'R'
				: 'B';

		/// <inheritdoc />
		protected internal override string ToString(int level)
		{
			return $"{Value} {ColorC}";
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void SwapColor([NotNull] RedBlackNode<T> other)
		{
			(other.Color, Color) = (Color, other.Color);
		}
	}
}