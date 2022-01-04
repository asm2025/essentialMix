using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections.Specialized;

[Serializable]
[TypeConverter(typeof(EmptyDisplayNameExpandableObjectConverter))]
public class IPAddressObservableCollection : ObservableCollection<IPAddressEntry>, IComparable<IPAddressObservableCollection>, IComparable, IEquatable<IPAddressObservableCollection>
{
	public IPAddressObservableCollection()
	{
	}

	public IPAddressObservableCollection([NotNull] List<IPAddressEntry> list) 
		: base(list)
	{
	}

	public IPAddressObservableCollection([NotNull] IEnumerable<IPAddressEntry> collection) 
		: base(collection)
	{
	}

	public virtual int CompareTo(IPAddressObservableCollection other) { return IPAddressCollectionComparer.Default.Compare(this, other); }

	public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

	public virtual bool Equals(IPAddressObservableCollection other) { return IPAddressCollectionComparer.Default.Equals(this, other); }

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (IPAddressEntry newItem in e.NewItems)
				newItem.PropertyChanged += OnItemPropertyChanged;
		}

		if (e.OldItems != null)
		{
			foreach (IPAddressEntry oldItem in e.OldItems)
				oldItem.PropertyChanged -= OnItemPropertyChanged;
		}

		base.OnCollectionChanged(e);
	}

	private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (sender is not IPAddressEntry item) return;

		int index = IndexOf(item);
		OnPropertyChanged(new PropertyChangedEventArgs($"Item[{index}]"));
	}
}