using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IRange<T> : IReadOnlyRange<T>
		where T : struct, IComparable
	{
		[Category("Range")]
		new T Minimum { get; set; }

		[Category("Range")]
		new T Maximum { get; set; }

		[Category("Entry")]
		new T Value { get; set; }

		bool Merge([NotNull] IReadOnlyRange<T> other);
		bool Exclude([NotNull] IReadOnlyRange<T> other);
		void ShiftBy(T steps);
		void InflateBy(T steps);
	}
}