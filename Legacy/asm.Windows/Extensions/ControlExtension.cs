using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Windows.Extensions
{
	public static class ControlExtension
	{
		public static void EnableControls([NotNull] this Control thisValue, bool value, params Control[] ignore)
		{
			thisValue.InvokeIf(() =>
			{
				if (thisValue.Controls.Count == 0) return;
				ISet<Control> controls = new HashSet<Control>(ignore ?? Array.Empty<Control>());
				EnableControlsLocal(thisValue, value, controls);
			});

			static void EnableControlsLocal(Control control, bool v, ISet<Control> controls)
			{
				control.InvokeIf(() =>
				{
					foreach (Control ctl in control.Controls)
					{
						if (!controls.Contains(ctl)) ctl.Enabled = v;

						foreach (Control ctl2 in ctl.Controls)
							EnableControlsLocal(ctl2, v, controls);
					}
				});
			}
		}

		public static Size ActualSize([NotNull] this Control thisValue)
		{
			Size sz = new Size(thisValue.Width, thisValue.Height);
			sz.Width += thisValue.Margin.Horizontal;
			sz.Height += thisValue.Margin.Vertical;
			if (thisValue.Parent == null) return sz;

			Control parent = thisValue.Parent;

			if (parent.Controls.Count == 1)
			{
				sz.Width += parent.Padding.Horizontal;
				sz.Height += parent.Padding.Vertical;
			}

			return sz;
		}

		public static void BindTo([NotNull] this Control thisValue, object data, string dataMember = null)
		{
			PropertyInfo property = thisValue.GetProperty("DataSource", Constants.BF_PUBLIC_INSTANCE | BindingFlags.SetProperty, typeof(object));
			if (property == null) throw new NotSupportedException();
			property.SetValue(thisValue, null);
			if (data is null) return;

			BindingSource source = new BindingSource(data, dataMember);
			property.SetValue(thisValue, source.DataSource);
		}
	}
}