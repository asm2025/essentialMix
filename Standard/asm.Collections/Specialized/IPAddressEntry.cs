using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Specialized
{
	[Serializable]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class IPAddressEntry : IComparable<IPAddressEntry>, IComparable, IEquatable<IPAddressEntry>, INotifyPropertyChanged
	{
		private string _value;

		[NonSerialized]
		private bool _isEmpty;

		public IPAddressEntry()
			: this(null)
		{
		}

		public IPAddressEntry(string entry)
		{
			Value = entry;
		}

		[NotNull]
		public override string ToString() { return _value ?? string.Empty; }

		[Category("Entry")]
		public string Value
		{
			get => _value;
			set
			{
				if (_value == value) return;
				if (!string.IsNullOrEmpty(value) && !Test(value)) throw new ArgumentException("Value is in a bad format.");
				_value = value;
				_isEmpty = string.IsNullOrEmpty(_value);
				OnPropertyChanged();
			}
		}

		[Browsable(false)]
		public bool IsEmpty => _isEmpty;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		public virtual int CompareTo(IPAddressEntry other) { return IPAddressEntryComparer.Default.Compare(this, other); }

		public virtual bool Equals(IPAddressEntry other) { return IPAddressEntryComparer.Default.Equals(this, other); }

		public virtual int CompareTo(object obj) { return IPAddressEntryComparer.Default.Compare(this, obj); }

		public static bool Test(string value) { return !string.IsNullOrEmpty(value) && IPAddressHelper.IsPossibleIPv4(value); }

		public static IPAddressEntry Parse([NotNull] string value)
		{
			return !Test(value)
						? null
						: new IPAddressEntry(value);
		}
	}
}