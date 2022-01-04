using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface ISimpleItem : IComparable<ISimpleItem>, IComparable, IEquatable<ISimpleItem>, INotifyPropertyChanged, ICloneable
{
	[Localizable(true)]
	string Text { get; set; }

	[Browsable(false)]
	object Value { get; set; }

	[Browsable(false)]
	object DefaultValue { get; set; }

	[Browsable(false)]
	bool Selected { get; set; }

	bool Enabled { get; set; }

	[Browsable(false)]
	object Tag { get; set; }

	[NotNull]
	[Browsable(false)]
	Type ValueType { get; }

	[Browsable(false)]
	bool IsPrimitive { get; }

	bool IsCompatibleObject(object value);

	void Reset();
}

public interface ISimpleItem<T> : ISimpleItem
{
	[Browsable(false)]
	new T Value { get; set; }

	[Browsable(false)]
	new T DefaultValue { get; set; }
}