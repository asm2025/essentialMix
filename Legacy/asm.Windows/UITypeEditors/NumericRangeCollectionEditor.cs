using System;
using System.ComponentModel;
using System.Drawing.Design;
using asm.Collections;
using JetBrains.Annotations;

namespace asm.Windows.UITypeEditors
{
	public class NumericRangeCollectionEditor<T> : CollectionEditor
		where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
	{
		public NumericRangeCollectionEditor() 
			: base(typeof(T))
		{
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.Modal; }

		protected override bool CanSelectMultipleInstances() { return false; }

		[NotNull]
		protected override Type CreateCollectionItemType() { return typeof(NumericRange<T>); }
	}
}