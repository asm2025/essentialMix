using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace asm.Collections
{
	public interface IReadOnlyRange<T> : IReadOnlyList<T>
		where T : struct, IComparable
	{
		[Category("Range")]
		T Minimum { get; }

		[Category("Range")]
		T Maximum { get; }

		[Category("Entry")]
		T Value { get; }

		[Browsable(false)]
		bool IsRange { get; }

		[Browsable(false)]
		bool IsEmpty { get; }

		[Browsable(false)]
		bool IsIterable { get; }

		[Browsable(false)]
		bool HasMany { get; }

		[Browsable(false)]
		bool HasOne { get; }

		bool InRange(T value);
		bool InRangeEx(T value);
		bool InRangeLx(T value);
		bool InRangeRx(T value);
		T Within(T value);
		T WithinEx(T value);
		T WithinLx(T value);
		T WithinRx(T value);
		bool Contains(T item);
		bool Contains(IReadOnlyRange<T> other);
		bool Overlaps(IReadOnlyRange<T> other);
		bool Overlaps(T minimum, T maximum);
		bool IsPreviousTo(IReadOnlyRange<T> other);
		bool IsNextTo(IReadOnlyRange<T> other);
		void CopyTo(T[] array, int arrayIndex);
	}
}