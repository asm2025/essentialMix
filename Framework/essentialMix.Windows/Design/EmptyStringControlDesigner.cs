using System.Collections;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace essentialMix.Windows.Design
{
	public class EmptyStringControlDesigner : ControlDesigner
	{
		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);
			PropertyDescriptor descriptor = TypeDescriptor.GetProperties(Component)["Text"];
			if (descriptor != null && descriptor.PropertyType == typeof(string) && !descriptor.IsReadOnly && descriptor.IsBrowsable)
				descriptor.SetValue(Component, string.Empty);
		}
	}
}
