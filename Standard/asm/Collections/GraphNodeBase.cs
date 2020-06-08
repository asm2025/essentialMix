using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphNodeBase<TNode, T>
		where TNode : GraphNodeBase<TNode, T>
	{
		protected GraphNodeBase([NotNull] T value)
		{
			Value = value;
		}

		[NotNull]
		public T Value { get; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }
	}
}