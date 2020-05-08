using System.ComponentModel;

namespace asm.ComponentModel
{
	public abstract class DropDownListTypeConverter<T> : DropDownTypeConverter<T>
	{
		protected DropDownListTypeConverter()
		{
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
	}
}