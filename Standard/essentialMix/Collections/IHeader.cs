using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IHeader : IFixable, IComparable<IHeader>, IComparable, IEquatable<IHeader>, INotifyPropertyChanged, ICloneable
	{
		[NotNull]
		string Name { get; set; }

		[Localizable(true)]
		string Text { get; set; }

		bool Enabled { get; set; }

		[Browsable(false)]
		object Tag { get; set; }

		[NotNull]
		[Browsable(false)]
		Type ValueType { get; }

		bool IsCompatibleObject(object value);
	}
}