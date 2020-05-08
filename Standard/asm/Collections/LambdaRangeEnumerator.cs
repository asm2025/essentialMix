using System;
using System.Collections;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public sealed class LambdaRangeEnumerator<T> : IEnumerator<T>, IEnumerator
		where T : struct, IComparable
	{
		[NotNull]
		private readonly Func<T, T> _step;

		private readonly T _start;
		private readonly T _end;
		private readonly IComparer<T> _comparer;
		private readonly bool _includesStart;
		private readonly bool _includesEnd;

		private bool _started;

		/// <inheritdoc />
		/// <summary>
		/// Creates an ascending iterator over the given range with the given step function
		/// </summary>
		public LambdaRangeEnumerator([NotNull] LambdaRange<T> range, [NotNull] Func<T, T> step)
			: this(range, step, true)
		{
		}

		/// <summary>
		/// Creates an iterator over the given range with the given step function,
		/// with the specified direction.
		/// </summary>
		public LambdaRangeEnumerator([NotNull] LambdaRange<T> range, [NotNull] Func<T, T> step, bool ascending)
		{
			if (ascending && range.Comparer.Compare(range.Minimum, step(range.Minimum)) >= 0 ||
				!ascending && range.Comparer.Compare(range.Maximum, step(range.Maximum)) <= 0)
			{
				throw new ArgumentException("step does nothing, or progresses the wrong way.");
			}

			_step = step;

			if (ascending)
			{
				_includesStart = range.IncludesStart;
				_includesEnd = range.IncludesEnd;
				_start = range.Minimum;
				_end = range.Maximum;
				_comparer = range.Comparer;
			}
			else
			{
				_includesStart = range.IncludesEnd;
				_includesEnd = range.IncludesStart;
				_start = range.Maximum;
				_end = range.Minimum;
				_comparer = range.Comparer.Reverse();
			}
		}

		/// <inheritdoc />
		public T Current { get; private set; }

		/// <inheritdoc />
		[NotNull]
		object IEnumerator.Current => Current;

		/// <inheritdoc />
		public void Dispose() { }

		/// <inheritdoc />
		public bool MoveNext()
		{
			if (!_started)
			{
				_started = true;
				Current = _includesStart
							? _start
							: _step(_start);
			}
			else if (_comparer.IsLessThan(Current, _end))
			{
				Current = _step(Current);
			}

			int cmp = _comparer.Compare(Current, _end);
			return cmp < 0 || _includesEnd && cmp == 0;
		}

		/// <inheritdoc />
		void IEnumerator.Reset()
		{
			_started = false;
			Current = _start;
		}
	}
}