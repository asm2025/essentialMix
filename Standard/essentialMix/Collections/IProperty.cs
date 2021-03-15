using System;
using System.ComponentModel;

namespace essentialMix.Collections
{
	public interface IProperty : IHeader, IComparable<IProperty>, IEquatable<IProperty>
	{
		[Browsable(false)]
		object Value { get; set; }

		[Browsable(false)]
		object DefaultValue { get; set; }

		PropertyScope Scope { get; set; }

		[Browsable(false)]
		bool IsPrimitive { get; }

		[Browsable(false)]
		bool IsReadOnly { get; }

		void Reset();
	}

	public interface IProperty<T> : IProperty
	{
		[Browsable(false)]
		new T Value { get; set; }

		[Browsable(false)]
		new T DefaultValue { get; set; }
	}
}