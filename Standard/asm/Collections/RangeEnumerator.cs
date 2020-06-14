using System;
using System.Collections.Generic;
using asm.Extensions;
using asm.Other.MarcGravell;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class RangeEnumerator<T> : Enumerator<T>
		where T : struct, IComparable
	{
		public RangeEnumerator()
			: this(default(T), default(T))
		{
			IsIterable = false;
		}

		public RangeEnumerator([NotNull] IReadOnlyRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public RangeEnumerator([NotNull] LambdaRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public RangeEnumerator([NotNull] IEnumerable<T> enumerable)
		{
			bool assigned = false;

			if (enumerable is IList<T> sourceList)
			{
				if (sourceList.Count > 0)
				{
					Minimum = sourceList[0];
					Maximum = sourceList[sourceList.Count - 1];
					assigned = true;
				}
			}
			else
			{
				using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Minimum = enumerator.Current;

						do
						{
							Maximum = enumerator.Current;
						}
						while (enumerator.MoveNext());

						assigned = true;
					}
				}
			}

			if (!assigned)
			{
				Minimum = Maximum = default(T);
				IsIterable = false;
			}
			else
			{
				IsIterable = Minimum.IsLessThanOrEqual(Maximum);
			}
		}

		public RangeEnumerator(T minimum, T maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
			IsIterable = Minimum.IsLessThanOrEqual(Maximum);
		}

		public override T Current
		{
			get
			{
				if (!IsValid) throw new InvalidOperationException("No value available.");
				return Value;
			}
		}

		public override bool MoveNext()
		{
			IsValid = false;
			if (!IsIterable || Done) return false;
			Value = Index < 0 ? Minimum : Operator<T>.Increment(Value);
			Done = Value.IsGreaterThan(Maximum);
			if (Done) return false;
			Index++;
			return true;
		}

		public override void Reset()
		{
			base.Reset();
			IsValid = false;
		}

		public T Minimum { get; }
		public T Maximum { get; }
		protected T Value { get; set; }
		protected bool IsValid { get; set; }
	}
}