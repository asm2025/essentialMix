using System;
using System.ComponentModel;
using asm.Helpers;
using DevExpress.Data.Mask;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using JetBrains.Annotations;

namespace asm.DevExpress.XtraEditors
{
	public class TimeSpanEdit : TimeEdit
	{
		/*
		 * based on https://www.devexpress.com/Support/Center/Question/Details/Q509471/editor-for-timespan
		 * Thanks to Asier Cayón Francisco
		*/

		static TimeSpanEdit() { RepositoryItemTimeSpanEdit.RegisterTimeSpanEdit(); }

		public TimeSpanEdit() { fOldEditValue = fEditValue = System.TimeSpan.Zero; }

		[NotNull]
		public override string EditorTypeName => RepositoryItemTimeSpanEdit.TIME_SPAN_EDIT_NAME;

		public override object EditValue
		{
			get
			{
				if (Properties.ExportMode == ExportMode.DisplayText) return Properties.GetDisplayText(null, base.EditValue);
				return TimeSpan;
			}
			set
			{
				TimeSpan? timeSpan = GetTimeSpan(value);

				if (null != timeSpan)
				{
					TimeSpan timeSpanValue = (TimeSpan)timeSpan;
					timeSpanValue = TimeSpanHelper.ToInternalOperationValue(timeSpanValue);
					TimeSpanHelper.AssertInInternalOperationRange(timeSpanValue);
					timeSpan = timeSpanValue;
				}
				base.EditValue = timeSpan;
			}
		}

		[NotNull]
		protected override MaskManager CreateMaskManager([NotNull] MaskProperties mask)
		{
			CustomTimeEditMaskProperties patchedMask = new CustomTimeEditMaskProperties(this);
			patchedMask.Assign(mask);
			patchedMask.EditMask = Properties.GetFormatMaskAccessFunction(mask.EditMask, mask.Culture);
			return patchedMask.CreatePatchedMaskManager();
		}

		public virtual TimeSpan? TimeSpan
		{
			get
			{
				TimeSpan? timeSpan = null;

				switch (base.EditValue)
				{
					case DateTime _:
						DateTime time = (DateTime)base.EditValue;
						timeSpan = new TimeSpan(time.Ticks);
						break;
					case TimeSpan _:
						timeSpan = (TimeSpan)base.EditValue;
						break;
					case string _:
						timeSpan = TimeSpanHelper.Parse((string)base.EditValue);
						break;
				}

				if (null == timeSpan) return timeSpan;

				TimeSpan timeSpanValue = (TimeSpan)timeSpan;
				timeSpanValue = TimeSpanHelper.ToUserOperationValue(timeSpanValue, out bool _);
				TimeSpanHelper.AssertInUserOperationRange(timeSpanValue, Properties.AllowNegativeValues);
				timeSpan = timeSpanValue;
				return timeSpan;
			}
			set => EditValue = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new RepositoryItemTimeSpanEdit Properties => base.Properties as RepositoryItemTimeSpanEdit;

		protected internal virtual TimeSpan? GetTimeSpan(object value)
		{
			TimeSpan? timeSpan = null;

			switch (value)
			{
				case DateTime time:
					timeSpan = new TimeSpan(time.Ticks);
					break;
				case TimeSpan span:
					timeSpan = span;
					break;
				case string str:
					timeSpan = TimeSpanHelper.Parse(str);
					break;
			}

			return timeSpan;
		}
	}
}