using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ObservableSortedSet<T> : SortedSet<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		private const string PROPERTY_NAME_ITEMS = "Item[]";

		[NonSerialized]
		private readonly SimpleMonitor _monitor = new SimpleMonitor();

		[NonSerialized]
		private uint _updateRefCount;

		public ObservableSortedSet()
		{
		}

		public ObservableSortedSet(IComparer<T> comparer) 
			: base(comparer)
		{
		}

		public ObservableSortedSet([NotNull] IEnumerable<T> enumerable) 
			: base(enumerable)
		{
		}

		public ObservableSortedSet([NotNull] IEnumerable<T> enumerable, [NotNull] IComparer<T> comparer) 
			: base(enumerable, comparer)
		{
		}

		protected ObservableSortedSet(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		public new bool Add([NotNull] T item)
		{
			CheckReentrancy();
			if (!base.Add(item)) return false;
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
			return true;
		}

		public new bool Remove(T item)
		{
			CheckReentrancy();
			if (!base.Remove(item)) return false;
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
			return true;
		}

		public override void Clear()
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