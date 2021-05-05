using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class ObservableCollectionTypeDescriptor<TSource, T> : TypeDescriptorBase<TSource>, ICollection<T>, ICollection, INotifyCollectionChanged, INotifyPropertyChanged
		where TSource : ObservableCollection<T>, ICollection<T>, ICollection
	{
		public ObservableCollectionTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
			source.CollectionChanged += (_, args) => OnCollectionChanged(args);
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
				pds.Add(new ObservableCollectionTypePropertyDescriptor<ObservableCollectionTypeDescriptor<TSource, T>, TSource, T>(this, i));

			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach((o, i) =>
			{
				if (o is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) return;
				}

				pds.Add(new ObservableCollectionTypePropertyDescriptor<ObservableCollectionTypeDescriptor<TSource, T>, TSource, T>(this, i));
			});

			return pds;
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		public IEnumerator<T> GetEnumerator() { return Source.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Add(T item) { Source.Add(item); }

		public void Clear() { Source.Clear(); }

		public bool Contains(T item) { return Source.Contains(item!); }

		public void CopyTo(T[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		public bool Remove(T item) { return Source.Remove(item); }

		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		public int Count => Source.Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		public bool IsReadOnly => false;

		public void ForEach([NotNull] Action<T> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, bool> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Action<T, int> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, int, bool> action) { Source.ForEach(action); }
	}
}