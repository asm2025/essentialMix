using System.Globalization;
using asm.DevExpress.Data;
using DevExpress.Data.Mask;
using DevExpress.XtraEditors.Mask;
using JetBrains.Annotations;

namespace asm.DevExpress.XtraEditors
{
	public class CustomTimeEditMaskProperties : TimeEditMaskProperties
	{
		private readonly TimeSpanEdit _timeSpanEdit;

		public CustomTimeEditMaskProperties(TimeSpanEdit timeSpanEdit)
		{
			_timeSpanEdit = timeSpanEdit;
		}

		[NotNull]
		public virtual MaskManager CreatePatchedMaskManager()
		{
			CultureInfo managerCultureInfo = Culture ?? CultureInfo.CurrentCulture;
			string editMask = EditMask ?? string.Empty;
			return new CustomDateTimeMaskManager(editMask, false, managerCultureInfo, true, _timeSpanEdit);
		}
	}
}