using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("{To} :{Weight}")]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class GraphEdge<T, TWeight>([NotNull] T to, TWeight weight)
	where TWeight : struct, IComparable, IComparable<TWeight>, IEquatable<TWeight>, IConvertible, IFormattable
{
	public GraphEdge([NotNull] T to)
		: this(to, default(TWeight))
	{
	}

	[NotNull]
	public T To { get; } = to;

	public TWeight Weight { get; set; } = weight;

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