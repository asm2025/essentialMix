using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Patterns.DateTime;

namespace asm.Collections
{
	public class DateRangeLister : RangeLister<DateTime>
	{
		private readonly Func<DateTime?> _getNextValue;

		private int _count;

		public DateRangeLister([NotNull] DateRange range)
			: this(range.Minimum, range.Maximum, range.Unit)
		{
		}

		public DateRangeLister([NotNull] IReadOnlyRange<DateTime> range, DateTimeUnit unit)
			: this(range.Minimum, range.Maximum, unit)
		{
		}

		public DateRangeLister([NotNull] LambdaRange<DateTime> range, DateTimeUnit unit)
			: this(range.Minimum, range.Maximum, unit)
		{
		}

		public DateRangeLister([NotNull] IEnumerable<DateTime> enumerable)
			: base(enumerable)
		{
			_count = List.Count;

			if (_count <= 1)
			{
				_getNextValue = () =>
				{
					if (Index < 0) return Minimum;
					return null;
				};
			}
			else
			{
				_getNextValue = () =>
				{
					if (Index < 0) return Minimum;
					if (Index >= _count) return null;
					return List[Index + 1];
				};
			}
		}

		public DateRangeLister(DateTime minimum, DateTime maximum, DateTimeUnit unit)
			: base(minimum, maximum)
		{
			switch (unit)
			{
				case DateTimeUnit.Year:
					_count = (int)Maximum.Years(Minimum);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddYears(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				case DateTimeUnit.Month:
					_count = (int)Maximum.Months(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddMonths(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				case DateTimeUnit.Hour:
					_count = (int)Maximum.Hours(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddHours(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				case DateTimeUnit.Minute:
					_count = (int)Maximum.Minutes(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddMinutes(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				case DateTimeUnit.Second:
					_count = (int)Maximum.Seconds(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddSeconds(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				case DateTimeUnit.Millisecond:
					_count = (int)Maximum.Milliseconds(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddMilliseconds(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
				default: // default to add day
					_count = (int)Maximum.Days(Minimum).NotAbove(int.MaxValue);
					_getNextValue = () =>
					{
						if (Index < 0) return Minimum;
						if (Index >= _count - 1) return null;
						if (Minimum.TryAddDays(Index + 1, out DateTime result)) return result;
						return null;
					};
					break;
			}
		}

		public override int Count
		{
			get
			{
				if (_count < 0)
				{
					_count = IsIterable ? Convert.ToInt32(Maximum) - Convert.ToInt32(Maximum) + 1 : 0;
				}

				return _count;
			}
		}

		protected override int MoveTo(int index)
		{
			if (!IsIterable || Done) return 0;

			int n = 0;

			while (!Done && List.Count <= index)
			{
				DateTime? next = _getNextValue();
				
				if (next == null)
				{
					Done = true;
					return n;
				}

				Value = next.Value;
				List.Add(Value);

				if (Index < 0) Index = 0;
				else Index++;

				n++;
			}

			return n;
		}
	}
}