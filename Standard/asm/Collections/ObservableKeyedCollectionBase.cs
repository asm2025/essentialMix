using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using asm.Threading;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class ObservableKeyedCollectionBase<TKey, TValue> : KeyedCollectionBase<TKey, TValue>, INotifyPropertyChanged, INotifyCollectionChanged
	{
		private const string PROPERTY_NAME_ITEMS = "Item[]";

		private readonly SimpleMonitor _monitor = new SimpleMonitor();

		[NonSerialized]
		private uint _updateRefCount;

		protected ObservableKeyedCollectionBase() { }

		protected ObservableKeyedCollectionBase(IEqualityComparer<TKey> comparer) : base(comparer) { }

		protected ObservableKeyedCollectionBase([NotNull] IEnumerable<TValue> collection) : base(collection) { }

		protected ObservableKeyedCollectionBase([NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { }

		protected override void InsertItem(int index, TValue item)
		{
			CheckReentrancy();
			base.InsertItem(index, item);
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}

		protected override void SetItem(int index, TValue item)
		{
			TValue value = Items[index];
			if (value.Equals(item)) return;
			CheckReentrancy();
			base.SetItem(index, item);
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, item, index);
		}

		public override void MoveItem(int index, int newIndex)
		{
			TValue value = Items[index];
			CheckReentrancy();
			base.MoveItem(index, newIndex);
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Move, value, newIndex, index);
		}

		protected override void RemoveItem(int index)
		{
			TValue value = Items[index];
			CheckReentrancy();
			base.RemoveItem(index);
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, index);
		}

		protected override void ClearItems()
		{
			CheckReentrancy();
			base.ClearItems();
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionReset();
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void BeginUpdate() { _updateRefCount++; }

		public void EndUpdate()
		{
			_updateRefCount--;
			if (_updateRefCount > 0) return;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(PROPERTY_NAME_ITEMS);
			OnCollectionChanged();
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

		protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs args)
		{
			if (CollectionChanged == null) return;

			using (BlockReentrancy())
			{
				CollectionChanged?.Invoke(this, args);
			}
		}

		protected void CheckReentrancy()
		{
			if (_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1) throw new InvalidOperationException("Reentrancy is not allowed");
		}

		[NotNull]
		protected IDisposable BlockReentrancy()
		{
			_monitor.Enter();
			return _monitor;
		}
	}
}