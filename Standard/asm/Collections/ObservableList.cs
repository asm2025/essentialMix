using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ObservableList<T> : List<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		private const string PROPERTY_NAME_ITEMS = "Item[]";

		[NonSerialized]
		private readonly SimpleMonitor _monitor = new SimpleMonitor();

		[NonSerialized]
		private uint _updateRefCount;

		public ObservableList() { }

		public ObservableList(int capacity)
			: base(capacity) { }

		public ObservableList([NotNull] IEnumerable<T> collection)
			: base(collection)
		{
		}

		public new T this[int index]
		{
			get => base[index];
			set
			{
				if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
				CheckReentrancy();

				T obj = this[index];
				base[index] = value;
				OnPropertyChanged();
				OnCollectionChanged(NotifyCollectionChangedAction.Replace, obj, value, index);
			}
		}

		public new void Add(T item)
		{
			CheckReentrancy();
			base.Add(item);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
		}

		public new void Insert(int index, T item)
		{
			CheckReentrancy();
			base.Insert(index, item);
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}

		public new void RemoveAt(int index)
		{
			CheckReentrancy();
			T item = base[index];
			base.RemoveAt(index);
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
		}

		public new bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		public new void Clear()
		{
			CheckReentrancy();
			base.Clear();
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionReset();
		}

		public void BeginUpdate() { _updateRefCount++; }

		public void EndUpdate()
		{
			_updateRefCount--;
			if (_updateRefCount > 0) return;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged();
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public virtual void Move(int oldIndex, int newIndex)
		{
			if (!oldIndex.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(oldIndex));
			if (!newIndex.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(newIndex));
			CheckReentrancy();

			T obj = base[oldIndex];
			base.RemoveAt(oldIndex);
			base.Insert(newIndex, obj);
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Move, obj, newIndex, oldIndex);
		}

		public new void InsertRange(int index, [NotNull] IEnumerable<T> collection)
		{
			BeginUpdate();

			try
			{
				base.InsertRange(index, collection);
			}
			finally
			{
				EndUpdate();
			}
		}

		public new void RemoveRange(int index, int count)
		{
			BeginUpdate();

			try
			{
				base.RemoveRange(index, count);
			}
			finally
			{
				EndUpdate();
			}
		}

		public new int RemoveAll([NotNull] Predicate<T> match)
		{
			BeginUpdate();

			try
			{
				return base.RemoveAll(match);
			}
			finally
			{
				EndUpdate();
			}
		}

		public new void Reverse(int index, int count)
		{
			BeginUpdate();

			try
			{
				base.Reverse(index, count);
			}
			finally
			{
				EndUpdate();
			}
		}

		public new void Sort(int index, int count, IComparer<T> comparer)
		{
			BeginUpdate();

			try
			{
				base.Sort(index, count, comparer);
			}
			finally
			{
				EndUpdate();
			}
		}

		public new void Sort([NotNull] Comparison<T> comparison)
		{
			BeginUpdate();

			try
			{
				base.Sort(comparison);
			}
			finally
			{
				EndUpdate();
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (_updateRefCount > 0) return;
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
		{
			if (_updateRefCount > 0) return;
			PropertyChanged?.Invoke(this, e);
		}

		protected virtual void OnCollectionChanged() { OnCollectionReset(); }

		protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
		{
			if (_updateRefCount > 0) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
		{
			if (_updateRefCount > 0) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
		{
			if (_updateRefCount > 0) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
		{
			if (_updateRefCount > 0) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		protected virtual void OnCollectionReset()
		{
			if (_updateRefCount > 0) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
		{
			if (_updateRefCount > 0 || CollectionChanged == null) return;

			using (BlockReentrancy())
			{
				CollectionChanged?.Invoke(this, e);
			}
		}

		protected void CheckReentrancy()
		{
			if (_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1) throw new InvalidOperationException("Observable collection reentrancy not allowed.");
		}

		[NotNull]
		protected IDisposable BlockReentrancy()
		{
			_monitor.Enter();
			return _monitor;
		}
	}
}