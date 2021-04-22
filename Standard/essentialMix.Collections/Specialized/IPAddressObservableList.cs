using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections.Specialized
{
	[Serializable]
	[TypeConverter(typeof(EmptyDisplayNameExpandableObjectConverter))]
	public class IPAddressObservableList : ObservableList<IPAddressEntry>, IComparable<IPAddressObservableList>, IComparable, IEquatable<IPAddressObservableList>
	{
		public IPAddressObservableList()
		{
		}

		public IPAddressObservableList(List<IPAddressEntry> enumerable) 
			: base(enumerable)
		{
		}

		public IPAddressObservableList([NotNull] IEnumerable<IPAddressEntry> enumerable) 
			: base(enumerable)
		{
		}

		public virtual int CompareTo(IPAddressObservableList other) { return IPAddressCollectionComparer.Default.Compare(this, other); }

		public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

		public virtual bool Equals(IPAddressObservableList other) { return IPAddressCollectionComparer.Default.Equals(this, other); }

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
			OnPropertyChanged($"Item[{index}]");
		}
	}
}