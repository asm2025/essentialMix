using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphVertex<TVertex, T>
		where TVertex : GraphVertex<TVertex, T>
	{
		protected GraphVertex([NotNull] T value)
		{
			Value = value;
		}

		[NotNull]
		public T Value { get; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public static implicit operator T([NotNull] GraphVertex<TVertex, T> vertex) { return vertex.Value; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphVertex<T> : GraphVertex<GraphVertex<T>, T>
	{
		public GraphVertex([NotNull] T value)
			: base(value)
		{
		}
	}
}