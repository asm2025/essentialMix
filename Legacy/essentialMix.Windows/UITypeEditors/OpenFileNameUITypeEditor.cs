using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace essentialMix.Windows.UITypeEditors
{
	public class OpenFileNameUITypeEditor : UITypeEditor
	{
		public OpenFileNameUITypeEditor()
		{
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.Modal; }

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context?.Instance == null || provider == null) return base.EditValue(context, provider, value);

			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (value != null) openFileDialog.FileName = Convert.ToString(value);
				if (!string.IsNullOrEmpty(context.PropertyDescriptor?.DisplayName)) openFileDialog.Title = context.PropertyDescriptor.DisplayName;
				openFileDialog.Filter = "All files (*.*)|*.*";
				if (openFileDialog.ShowDialog() == DialogResult.OK) value = openFileDialog.FileName;
			}

			return value;
		}
	}
}