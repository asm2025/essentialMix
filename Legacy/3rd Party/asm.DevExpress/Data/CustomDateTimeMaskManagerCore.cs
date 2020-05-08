using System;
using System.Globalization;
using asm.DevExpress.XtraEditors;
using asm.Helpers;
using DevExpress.Data.Mask;
using JetBrains.Annotations;

namespace asm.DevExpress.Data
{
	public class CustomDateTimeMaskManagerCore : DateTimeMaskManagerCore
	{
		private readonly TimeSpanEdit _timeSpanEdit;

		public CustomDateTimeMaskManagerCore(string mask, bool isOperatorMask, CultureInfo culture, bool allowNull, TimeSpanEdit timeSpanEdit)
			: base(mask, isOperatorMask, culture, allowNull)
		{
			_timeSpanEdit = timeSpanEdit;
			fFormatInfo = new CustomDateTimeMaskFormatInfo(mask, fInitialDateTimeFormatInfo, timeSpanEdit);
		}

		public override object GetCurrentEditValue()
		{
			object currentEditValue = base.GetCurrentEditValue();
			if (!(currentEditValue is DateTime)) return currentEditValue;
			TimeSpan operationValue = new TimeSpan(((DateTime)currentEditValue).Ticks);
			currentEditValue = new DateTime(TimeSpanHelper.ToInternalOperationValue(operationValue).Ticks);
			return currentEditValue;
		}

		public override void SetInitialEditText(string initialEditText)
		{
			KillCurrentElementEditor();
			DateTime? initialEditValue = GetClearValue();
			if (!string.IsNullOrEmpty(initialEditText))
			{
				try
				{
					initialEditValue = new DateTime(TimeSpanHelper.Parse(initialEditText).Ticks);
				}
				catch
				{
				}
			}
			SetInitialEditValue(initialEditValue);
		}

		public override void SetInitialEditValue(object initialEditValue)
		{
			TimeSpan? timeSpan = _timeSpanEdit.GetTimeSpan(initialEditValue);
			if (timeSpan != null) initialEditValue = TimeSpanHelper.ToInternalOperationValue((TimeSpan)timeSpan);
			base.SetInitialEditValue(initialEditValue);
		}

		protected override DateTime GetClearValue() { return new DateTime(TimeSpanHelper.ToInternalOperationValue(TimeSpanHelper.NeutralUserOperationValue).Ticks); }
	}
}