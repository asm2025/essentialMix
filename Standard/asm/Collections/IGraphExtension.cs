using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public static class IGraphExtension
	{
		public static void AddEdge<T>([NotNull] this IGraph<T> thisValue, [NotNull] Tuple<T, T> tuple)
			where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
		{
			thisValue.AddEdge(tuple.Item1, tuple.Item2);
		}

		public static void AddEdge<T>([NotNull] this IGraph<T> thisValue, (T, T) tuple)
			where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
		{
			thisValue.AddEdge(tuple.Item1, tuple.Item2);
		}

		public static void AddEdge<T>([NotNull] this IGraph<T> thisValue, KeyValuePair<T, T> pair)
			where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
		{
			thisValue.AddEdge(pair.Key, pair.Value);
		}
	}
}