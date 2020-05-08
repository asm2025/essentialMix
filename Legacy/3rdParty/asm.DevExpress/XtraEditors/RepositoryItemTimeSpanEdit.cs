using System;
using System.ComponentModel;
using System.Globalization;
using asm.Helpers;
using asm.Patterns.DateTime;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using JetBrains.Annotations;

namespace asm.DevExpress.XtraEditors
{
	[UserRepositoryItem("RegisterTimeSpanEdit")]
	public class RepositoryItemTimeSpanEdit : RepositoryItemTimeEdit
	{
		/*
		 * based on https://www.devexpress.com/Support/Center/Question/Details/Q509471/editor-for-timespan
		 * Thanks to Asier Cayón Francisco
		*/
		public const string TIME_SPAN_EDIT_NAME = "TimeSpanEdit";

		private TimeUnit _allowedUnit;

		static RepositoryItemTimeSpanEdit() { RegisterTimeSpanEdit(); }

		public RepositoryItemTimeSpanEdit() { Initialize(); }

		[NotNull]
		public override string EditorTypeName => TIME_SPAN_EDIT_NAME;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override FormatInfo EditFormat => base.EditFormat;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override FormatInfo DisplayFormat => base.DisplayFormat;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override MaskProperties Mask => base.Mask;

		public override void Assign([NotNull] RepositoryItem item)
		{
			BeginUpdate();

			try
			{
				base.Assign(item);
				if (!(item is RepositoryItemTimeSpanEdit source)) return;
				AllowedUnit = source.AllowedUnit;
				AllowNegativeValues = source.AllowNegativeValues;
			}
			finally
			{
				EndUpdate();
			}
		}

		public override string GetDisplayText(FormatInfo format, object editValue)
		{
			if (editValue is TimeSpan span) return TimeSpanHelper.TimeSpanToString(span, _allowedUnit);
			string displayText = editValue as string;
			return displayText ?? base.GetDisplayText(null, string.Empty);
		}

		[Category(CategoryName.Behavior)]
		[DefaultValue(TimeUnit.Hour | TimeUnit.Minute)]
		public virtual TimeUnit AllowedUnit
		{
			get => _allowedUnit;
			set
			{
				if (_allowedUnit == value) return;
				_allowedUnit = value;
				UpdateFormats();
			}
		}

		[Category(CategoryName.Behavior)]
		[DefaultValue(false)]
		public virtual bool AllowNegativeValues { get; set; }

		[Browsable(false)]
		[NotNull]
		protected new virtual string EditMask
		{
			get
			{
				string mask = string.Empty;

				if (_allowedUnit.HasFlag(TimeUnit.Second))
				{
					// 00-59 : 0-1727999999
					string secondsMask = _allowedUnit.HasFlag(TimeUnit.Minute) ? "ss" : "s";

					mask = secondsMask + mask;
				}

				if (_allowedUnit.HasFlag(TimeUnit.Minute))
				{
					// 00-59 : 0-28799999
					string minutesMask = _allowedUnit.HasFlag(TimeUnit.Hour) ? "mm" : "m";

					mask = mask.Length > 0 ? $@"\{TimeSeparator}{mask}" : mask;
					mask = minutesMask + mask;
				}

				if (_allowedUnit.HasFlag(TimeUnit.Hour))
				{
					//00-59 : 0-479999
					string hoursMask = _allowedUnit.HasFlag(TimeUnit.Day) ? "HH" : "H";

					mask = mask.Length > 0 ? $@"\{TimeSeparator}{mask}" : mask;
					mask = hoursMask + mask;
				}

				if (_allowedUnit.HasFlag(TimeUnit.Day))
				{
					//0-19999
					string daysMask = "d";

					mask = mask.Length > 0 ? $@"\{DaySeparator}{mask}" : mask;
					mask = daysMask + mask;
				}

				if (mask == "d" || mask == "H" || mask == "m" || mask == "s") mask = "%" + mask; //Custom en vez de Predefined
				return mask;
			}
		}

		protected internal virtual char TimeSeparator => TimeSpanHelper.TimeSeparator;

		protected internal virtual char DaySeparator => TimeSpanHelper.DaySeparator;

		protected virtual void UpdateFormats()
		{
			Mask.MaskType = MaskType.DateTime;
			EditFormat.FormatType = FormatType.DateTime;
			DisplayFormat.FormatType = FormatType.DateTime;
			Mask.EditMask = EditMask;
			EditFormat.FormatString = EditMask;
			DisplayFormat.FormatString = EditMask;
		}

		protected internal virtual string GetFormatMaskAccessFunction(string editMask, CultureInfo managerCultureInfo) { return GetFormatMask(editMask, managerCultureInfo); }

		private void Initialize()
		{
			AllowNullInput = DefaultBoolean.True;
			AllowedUnit = TimeUnit.Hour | TimeUnit.Minute;
			AllowNegativeValues = false;
			Mask.UseMaskAsDisplayFormat = true;
			UpdateFormats();
		}

		public static void RegisterTimeSpanEdit()
		{
			EditorRegistrationInfo.Default.Editors.Add(
				new EditorClassInfo(TIME_SPAN_EDIT_NAME,
					typeof(TimeSpanEdit),
					typeof(RepositoryItemTimeSpanEdit),
					typeof(BaseSpinEditViewInfo),
					new ButtonEditPainter(),
					true)
			);
		}
	}
}