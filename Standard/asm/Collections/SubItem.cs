using System;
using System.ComponentModel;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class SubItem : Property, IFixable, IComparable<SubItem>, IComparable, IEquatable<SubItem>, INotifyPropertyChanged, ICloneable
	{
		public SubItem([NotNull] string name)
			: this(name, null, null, false, false)
		{
		}

		public SubItem([NotNull] string name, object value)
			: this(name, null, value, false, false)
		{
		}

		public SubItem([NotNull] string name, object value, bool isFixed)
			: this(name, null, value, isFixed, false)
		{
		}

		public SubItem([NotNull] string name, object value, bool isFixed, bool isReadOnly)
			: this(name, null, value, isFixed, isReadOnly)
		{
		}

		public SubItem([NotNull] string name, string text, object value, bool isFixed, bool isReadOnly) 
			: base(name, text, value, isFixed, isReadOnly)
		{
		}

		public override object Clone() { return this.CloneMemberwise(); }

		public override string ToString() { return base.Text; }

		public override string Text
		{
			get => base.Text;
			set { base.Text = value.IfNullOrEmpty(() => Convert.ToString(Value)); }
		}

		public Column Column { get; internal set; }

		public IItem Owner { get; internal set; }

		public virtual int CompareTo(SubItem other) { return SubItemComparer.Default.Compare(this, other); }

		public virtual bool Equals(SubItem other) { return SubItemComparer.Default.Equals(this, other); }
	}
}