using System;
using System.ComponentModel;
using System.Drawing.Design;
using essentialMix.Collections;
using JetBrains.Annotations;

namespace essentialMix.Windows.UITypeEditors
{
	public class NumericRangeCollectionEditor<T> : CollectionEditor
		where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible, IFormattable
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