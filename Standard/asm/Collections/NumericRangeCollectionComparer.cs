using System;
using System.Collections.Generic;
using System.Linq;
using asm.Comparers;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class NumericRangeCollectionComparer<T> : GenericComparer<ICollection<NumericRange<T>>>
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
	{
		public new static NumericRangeCollectionComparer<T> Default { get; } = new NumericRangeCollectionComparer<T>();

		public NumericRangeCollectionComparer()
		{
		}

		public override int Compare(ICollection<NumericRange<T>> x, ICollection<NumericRange<T>> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			if (x.Count != y.Count) return y.Count - x.Count;
			return x.Select((t, i) => NumericRangeComparer<T>.Default.Compare(t, y.ElementAt(i))).Sum();
		}

		public override bool Equals(ICollection<NumericRange<T>> x, ICollection<NumericRange<T>> y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			if (x.Count != y.Count) return false;
			return !x.Where((t, i) => !NumericRangeComparer<T>.Default.Equals(t, y.ElementAt(i))).Any();
		}

		public TResult Simplify<TResult>([NotNull] ICollection<NumericRange<T>> obj, NumericRangeComparer<T> comparer = null, TResult defaultValue = default(TResult))
			where TResult : List<NumericRange<T>>, new()
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (obj.Count == 0) return defaultValue;

			TResult result = new TResult();
			result.AddRange(obj);
			if (result.Count <= 1) return result;

			comparer ??= NumericRangeComparer<T>.Default;
			result.Sort(comparer);

			for (int i = result.Count - 1; i >= 0 && result.Count > 1; --i)
			{
				int n = 0;
				NumericRange<T> x = result[i];

				for (int j = result.Count - 1; j >= 0 && result.Count > 1; --j)
				{
					NumericRange<T> y = result[j];
					if (!x.Merge(y)) continue;
					result[i] = x;
					result.RemoveAt(j);
					--j;
					++n;
				}

				i -= n;
			}

			return result;
		}
	}
}