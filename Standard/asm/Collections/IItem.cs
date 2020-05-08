using System;
using System.ComponentModel;

namespace asm.Collections
{
	public interface IItem : IProperty, IComparable<IItem>, IEquatable<IItem>
	{
		[Browsable(false)]
		bool IsContainer { get; }

		[Browsable(false)]
		bool Selected { get; set; }

		[Browsable(false)]
		IBrowsable Parent { get; }

		[Browsable(false)]
		Item.SubItemsCollection SubItems { get; }
	}

	public interface IItem<T> : IItem
	{
		[Browsable(false)]
		new T Value { get; set; }

		[Browsable(false)]
		new T DefaultValue { get; set; }
	}
}