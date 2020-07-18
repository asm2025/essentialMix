using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{To} :{Weight}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphEdge<T, TWeight>
		where TWeight : struct, IComparable, IComparable<TWeight>, IEquatable<TWeight>, IConvertible, IFormattable
	{
		public GraphEdge([NotNull] T to)
			: this(to, default(TWeight))
		{
		}

		public GraphEdge([NotNull] T to, TWeight weight)
		{
			To = to;
			Weight = weight;
		}

		[NotNull]
		public T To { get; }

		public TWeight Weight { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return $"{To} :{Weight}"; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphEdge<T> : GraphEdge<T, int>
	{
		/// <inheritdoc />
		public GraphEdge([NotNull] T to)
			: base(to)
		{
		}

		/// <inheritdoc />
		public GraphEdge([NotNull] T to, int weight)
			: base(to, weight)
		{
		}
	}
}