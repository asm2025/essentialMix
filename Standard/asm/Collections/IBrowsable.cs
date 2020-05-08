using System;
using System.ComponentModel;

namespace asm.Collections
{
	public interface IBrowsable : IItem, IComparable<IBrowsable>, IEquatable<IBrowsable>, IObservableKeyedCollection<string, IItem>
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