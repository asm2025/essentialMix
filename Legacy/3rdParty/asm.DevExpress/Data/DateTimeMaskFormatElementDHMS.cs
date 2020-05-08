using System;
using System.Globalization;
using asm.DevExpress.XtraEditors;
using asm.Helpers;
using asm.Patterns.DateTime;
using DevExpress.Data.Mask;
using JetBrains.Annotations;

namespace asm.DevExpress.Data
{
	internal class DateTimeMaskFormatElementDHMS : DateTimeNumericRangeFormatElementEditable
	{
		private readonly TimeSpanEdit _timeSpanEdit;
		private readonly TimeUnit _part;

		internal DateTimeMaskFormatElementDHMS([NotNull] TimeSpanEdit timeSpanEdit, TimeUnit part, DateTimeFormatInfo dateTimeFormatInfo)
			: base(TimeSpanHelper.GetMask(GetFormat(timeSpanEdit), part), dateTimeFormatInfo, DateTimePart.Time)
		{
			_timeSpanEdit = timeSpanEdit;
			_part = part;
		}

		[NotNull]
		public override DateTimeElementEditor CreateElementEditor(DateTime editedDateTime)
		{
			TimeSpan userValue = TimeSpanHelper.ToUserOperationValue(new TimeSpan(editedDateTime.Ticks), out bool isNegativeZero);
			int initialValue = TimeSpanHelper.GetPart(userValue, GetFormat(_timeSpanEdit), _part);
			bool isInitialNegativeZeroValuePart = isNegativeZero || initialValue == 0 && userValue.Ticks < 0;
			bool containsNextPart = TimeSpanHelper.ContainsNextPart(GetFormat(_timeSpanEdit), _part);
			bool allowNegativeZero = !containsNextPart && AllowNegativeValues;

			return new NeutralDateTimeNumericRangeElementEditor(
				allowNegativeZero,
				isInitialNegativeZeroValuePart,
				containsNextPart ? Math.Abs(initialValue) : initialValue, // 5 | -5
				TimeSpanHelper.GetNeutralPartValue(GetFormat(_timeSpanEdit), _part), //  0 | 0,
				TimeSpanHelper.GetMinPartValue(GetFormat(_timeSpanEdit), _part, AllowNegativeValues), //  0 | -1727999999, 
				TimeSpanHelper.GetMaxPartValue(GetFormat(_timeSpanEdit), _part), // 59 |  1727999999,
				TimeSpanHelper.GetMinNumDigits(GetFormat(_timeSpanEdit), _part, AllowNegativeValues), //  2 | 1
				TimeSpanHelper.GetMaxNumDigits(GetFormat(_timeSpanEdit), _part, AllowNegativeValues)); //  2 | 10);
		}

		public override DateTime ApplyElement(int result, DateTime editedDateTime)
		{
			TimeSpan newInternalTimeSpan = TimeSpanHelper.ChangePart(new TimeSpan(editedDateTime.Ticks), GetFormat(_timeSpanEdit), _part, result);
			return new DateTime(newInternalTimeSpan.Ticks);
		}

		[NotNull]
		public override string Format(DateTime formattedDateTime)
		{
			TimeSpan userOperationValue = TimeSpanHelper.ToUserOperationValue(new TimeSpan(formattedDateTime.Ticks), out bool isNegativeZero);
			int part = TimeSpanHelper.GetPart(userOperationValue, GetFormat(_timeSpanEdit), _part);
			bool isNegativeZeroPart = isNegativeZero || part == 0 && userOperationValue.Ticks < 0;
			return TimeSpanHelper.ContainsNextPart(GetFormat(_timeSpanEdit), _part)
				? Math.Abs(part).ToString("00")
				: $"{(isNegativeZeroPart ? "-" : string.Empty)}{part:0}";
		}

		private bool AllowNegativeValues => _timeSpanEdit.Properties.AllowNegativeValues;

		private static TimeUnit GetFormat([NotNull] TimeSpanEdit timeSpanEdit) { return timeSpanEdit.Properties.AllowedUnit; }
	}
}