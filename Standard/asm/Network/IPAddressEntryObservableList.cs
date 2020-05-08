using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using JetBrains.Annotations;
using asm.Collections;
using asm.Comparers;

namespace asm.Network
{
	[Serializable]
	[TypeConverter(typeof(EmptyDisplayNameExpandableObjectConverter))]
	public class IPAddressEntryObservableList : ObservableList<IPAddressEntry>, IComparable<IPAddressEntryObservableList>, IComparable, IEquatable<IPAddressEntryObservableList>
	{
		public IPAddressEntryObservableList()
		{
		}

		public IPAddressEntryObservableList(List<IPAddressEntry> collection) 
			: base(collection)
		{
		}

		public IPAddressEntryObservableList([NotNull] IEnumerable<IPAddressEntry> collection) 
			: base(collection)
		{
		}

		public virtual int CompareTo(IPAddressEntryObservableList other) { return IPAddressEntryCollectionComparer.Default.Compare(this, other); }

		public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

		public virtual bool Equals(IPAddressEntryObservableList other) { return IPAddressEntryCollectionComparer.Default.Equals(this, other); }

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
			if (!(sender is IPAddressEntry item)) return;

			int index = IndexOf(item);
			OnPropertyChanged($"Item[{index}]");
		}
	}
}