using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace essentialMix.Collections
{
	public interface IBrowsable : IItem, IComparable<IBrowsable>, IEquatable<IBrowsable>, INotifyCollectionChanged
	{
		Browsable.ColumnsCollection Columns { get; }

		[Browsable(false)]
		Browsable.ItemsCollection Items { get; }
	}

	public interface IBrowsable<T> : IBrowsable
	{
		[Browsable(false)]
		new T Value { get; set; }

		[Browsable(false)]
		new T DefaultValue { get; set; }
	}
}