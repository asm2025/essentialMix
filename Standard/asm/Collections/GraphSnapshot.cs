using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Patterns.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class GraphSnapshot<T> : Dictionary<T, T>
		where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		/// <inheritdoc />
		public GraphSnapshot() 
		{
		}

		/// <inheritdoc />
		public GraphSnapshot([NotNull] IDictionary<T, T> dictionary)
			: base(dictionary)
		{
		}

		/// <inheritdoc />
		public GraphSnapshot([NotNull] IDictionary<T, T> dictionary, IEqualityComparer<T> comparer)
			: base(dictionary, comparer)
		{
		}

		/// <inheritdoc />
		public GraphSnapshot(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public GraphSnapshot(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public GraphSnapshot(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphSnapshot(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public TraverseMethod Algorithm { get; internal set; }
	}
}