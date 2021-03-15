using System;
using System.Collections.Generic;
using essentialMix.Extensions;
using Other.MarcGravell;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class RangeLister<T> : Lister<T>
		where T : struct, IComparable
	{
		private int _count = -1;

		public RangeLister([NotNull] IReadOnlyRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public RangeLister([NotNull] LambdaRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public RangeLister([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
			if (List.Count > 0)
			{
				Minimum = List[0];
				Maximum = List[List.Count - 1];
			}
			else
			{
				Minimum = Maximum = default(T);
			}

			_count = List.Count;
			IsIterable = List.Count > 0;
		}

		public RangeLister(T minimum, T maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
			IsIterable = Minimum.IsLessThanOrEqual(Maximum);
		}

		public override int Count
		{
			get
			{
				if (_count < 0) _count = IsIterable ? Convert.ToInt32(Maximum) - Convert.ToInt32(Maximum) + 1 : 0;
				return _count;
			}
		}

		public T Minimum { get; }
		public T Maximum { get; }
		protected T Value { get; set; }
		protected bool IsValid { get; set; }

		protected override int MoveTo(int index)
		{
			if (!IsIterable || Done) return 0;

			int n = 0;

			while (!Done && List.Count <= index)
			{
				Value = Index < 0 ? Minimum : Operator<T>.Increment(Value);
				Done = Value.IsGreaterThan(Maximum);
				if (Done) return n;
				List.Add(Value);

				if (Index < 0) Index = 0;
				else Index++;

				n++;
			}

			return n;
		}
	}
}