using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class ObservableListTypeDescriptor<TSource, T> : TypeDescriptorBase<TSource>, IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
		where TSource : ObservableList<T>, IList<T>, IList
	{
		public ObservableListTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
			source.PropertyChanged += (sender, args) => OnPropertyChanged(args);
			source.CollectionChanged += (sender, args) => OnCollectionChanged(args);
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
				pds.Add(new ObservableListTypePropertyDescriptor<ObservableListTypeDescriptor<TSource, T>, TSource, T>(this, i));

			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
			{
				if (this[i] is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) continue;
				}

				pds.Add(new ObservableListTypePropertyDescriptor<ObservableListTypeDescriptor<TSource, T>, TSource, T>(this, i));
			}

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

		public int Add(object value) { return Source.Add(value); }

		public bool Contains(object value) { return Source.Contains(value); }

		public void Clear() { Source.Clear(); }
		public int IndexOf(object value) { return Source.IndexOf(value); }

		public void Insert(int index, object value) { Source.Insert(index, value); }

		public void Remove(object value) { Source.Remove(value); }

		public bool Contains(T item) { return Source.Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		public bool Remove(T item) { return Source.Remove(item); }

		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		public int Count => Source.Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		public bool IsReadOnly => false;

		public bool IsFixedSize => Source.IsFixedSize;

		public int IndexOf(T item) { return Source.IndexOf(item); }

		public void Insert(int index, T item) { Source.Insert(index, item); }

		public void RemoveAt(int index) { Source.RemoveAt(index); }

		object IList.this[int index]
		{
			get => Source[index];
			set => ((IList)Source)[index] = value;
		}

		public T this[int index]
		{
			get => Source[index];
			set => Source[index] = value;
		}

		public void ForEach([NotNull] Action<T> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, bool> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Action<T, int> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, int, bool> action) { Source.ForEach(action); }
	}
}