using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace essentialMix.Windows.UITypeEditors;

public class SaveFileNameUITypeEditor : UITypeEditor
{
	public SaveFileNameUITypeEditor()
	{
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.Modal; }

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context?.Instance == null || provider == null) return base.EditValue(context, provider, value);

		using (SaveFileDialog saveFileDialog = new SaveFileDialog())
		{
			if (value != null) saveFileDialog.FileName = Convert.ToString(value);
			if (!string.IsNullOrEmpty(context.PropertyDescriptor?.DisplayName)) saveFileDialog.Title = context.PropertyDescriptor.DisplayName;
			saveFileDialog.Filter = "All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK) value = saveFileDialog.FileName;
		}

		return value;
	}
}