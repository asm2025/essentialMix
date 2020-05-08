using System;
using System.Globalization;
using asm.DevExpress.XtraEditors;
using DevExpress.Data.Mask;

namespace asm.DevExpress.Data
{
	public class CustomDateTimeMaskManager : MaskManagerSelectAllEnhancer<CustomDateTimeMaskManagerCore>
	{
		public CustomDateTimeMaskManager(string mask, bool isOperatorMask, CultureInfo culture, bool allowNull, TimeSpanEdit timeSpanEdit)
			: base(new CustomDateTimeMaskManagerCore(mask, isOperatorMask, culture, allowNull, timeSpanEdit))
		{
		}

		protected override bool IsNestedCanSelectAll => false;

		public override bool Backspace()
		{
			if (!IsSelectAllEnforced) return base.Backspace();
			ClearSelectAllFlag();
			Nested.ClearFromSelectAll();
			return true;
		}

		public override bool Delete()
		{
			if (!IsSelectAllEnforced) return base.Delete();
			ClearSelectAllFlag();
			Nested.ClearFromSelectAll();
			return true;
		}

		protected override bool MakeChange(Func<bool> changeWithTrueWhenSuccessful)
		{
			if (!IsSelectAllEnforced) return base.MakeChange(changeWithTrueWhenSuccessful);
			ClearSelectAllFlag();
			if (!DoNotClearValueOnInsertAfterSelectAll) Nested.ClearFromSelectAll();
			base.MakeChange(changeWithTrueWhenSuccessful);
			return true;
		}

		protected override bool MakeCursorOp(Func<bool> cursorOpWithTrueWhenSuccessful)
		{
			if (!IsSelectAllEnforced) return base.MakeCursorOp(cursorOpWithTrueWhenSuccessful);
			ClearSelectAllFlag();
			base.MakeCursorOp(cursorOpWithTrueWhenSuccessful);
			return true;
		}

		protected static bool DoNotClearValueOnInsertAfterSelectAll { get; set; }
	}
}