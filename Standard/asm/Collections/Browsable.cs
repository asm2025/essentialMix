using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class Browsable : Item, IBrowsable, IReadOnlyKeyedCollection<string, IItem>, IReadOnlyList<IItem>, IReadOnlyCollection<IItem>, IList<IItem>, IList, ISerializable, IDeserializationCallback
	{
		[Serializable]
		public class ColumnsCollection : ObservableKeyedCollectionBase<string, Column>
		{
			[NonSerialized] 
			private SerializationInfo _siInfo;

			protected ColumnsCollection(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				_siInfo = info;
			}

			internal ColumnsCollection([NotNull] IBrowsable owner)
				: base(StringComparer.OrdinalIgnoreCase)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue("Owner", Owner, typeof(IBrowsable));
			}

			public override void OnDeserialization(object sender)
			{
				base.OnDeserialization(sender);
				if (_siInfo == null) return;
				Owner = (IBrowsable)_siInfo.GetValue("Owner", typeof(IBrowsable)) ?? throw new SerializationException("Invalid owner type.");
				_siInfo = null;
			}

			/// <inheritdoc />
			protected override void RemoveItem(int index)
			{
				Column item = base[index];
				if (item.IsFixed) return;
				base.RemoveItem(index);
			}

			protected override void ClearItems()
			{
				if (Count == 0) return;
				this.RemoveAll(item => !item.IsFixed);
			}

			[NotNull]
			protected override string GetKeyForItem(Column item) { return item.Name; }

			public IBrowsable Owner { get; private set; }
		}

		[Serializable]
		public class ItemsCollection : ObservableKeyedCollectionBase<string, IItem>
		{
			[NonSerialized] 
			private SerializationInfo _siInfo;

			protected ItemsCollection(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				_siInfo = info;
			}

			internal ItemsCollection([NotNull] Browsable owner)
				: base(StringComparer.OrdinalIgnoreCase)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue("Owner", Owner, typeof(Browsable));
			}

			public override void OnDeserialization(object sender)
			{
				base.OnDeserialization(sender);
				if (_siInfo == null) return;
				Owner = (Browsable)_siInfo.GetValue("Owner", typeof(Browsable)) ?? throw new SerializationException("Invalid owner type.");
				_siInfo = null;
			}

			/// <inheritdoc />
			protected override void InsertItem(int index, IItem item)
			{
				base.InsertItem(index, item);
				if (item is Item t) t.Parent = Owner;
			}

			/// <inheritdoc />
			protected override void RemoveItem(int index)
			{
				IItem item = base[index];
				if (item.IsFixed) return;
				base.RemoveItem(index);
			}

			protected override void ClearItems()
			{
				if (Count == 0) return;
				this.RemoveAll(item => !item.IsFixed);
			}

			[NotNull]
			protected override string GetKeyForItem(IItem item) { return item.Name; }

			public Browsable Owner { get; private set; }
		}

		[NonSerialized] 
		private SerializationInfo _siInfo;

		private ColumnsCollection _columns;
		private ItemsCollection _items;

		public Browsable([NotNull] string name)
			: this(name, null, null, null, false, false)
		{
		}

		public Browsable([NotNull] string name, object value)
			: this(name, null, value, null, false, false)
		{
		}

		public Browsable([NotNull] string name, string text, object value)
			: this(name, text, value, null, false, false)
		{
		}

		public Browsable([NotNull] string name, object value, bool isFixed, bool isReadOnly)
			: this(name, null, value, null, isFixed, isReadOnly)
		{
		}

		public Browsable([NotNull] string name, string text, object value, bool isFixed, bool isReadOnly)
			: this(name, text, value, null, isFixed, isReadOnly)
		{
		}

		public Browsable([NotNull] string name, string text, object value, Type type, bool isFixed, bool isReadOnly)
			: base(name, text, value, type, isFixed, isReadOnly)
		{
		}

		protected Browsable([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_siInfo = info;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Columns", _columns, typeof(ColumnsCollection));
			info.AddValue("Items", _items, typeof(ItemsCollection));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IItem this[int index] { get => Items[index]; set => Items[index] = value; }

		public IItem this[[NotNull] string key] => Items[key];

		object IList.this[int index] { get => this[index]; set => this[index] = (IItem)value; }

		IItem IReadOnlyList<IItem>.this[int index] => this[index];

		public int Count => Items.Count;

		public object SyncRoot => ((ICollection)Items).SyncRoot;

		public bool IsSynchronized => ((ICollection)Items).IsSynchronized;

		/// <inheritdoc />
		public IEqualityComparer<string> Comparer => Items.Comparer;

		[NotNull]
		public ColumnsCollection Columns => _columns ??= new ColumnsCollection(this);

		public bool IsFixedSize => Items.IsFixedSize;

		[NotNull]
		public virtual ItemsCollection Items
		{
			get
			{
				if (_items != null) return _items;
				_items = new ItemsCollection(this);
				_items.CollectionChanged += (sender, args) =>
				{
					if (CollectionChanged == null) return;
					CollectionChanged(this, args);
				};
				return _items;
			}
		}

		int ICollection.Count => Count;

		int ICollection<IItem>.Count => Count;

		int IReadOnlyCollection<KeyValuePair<string, IItem>>.Count => Count;

		int IReadOnlyCollection<IItem>.Count => Count;

		IEnumerable<string> IReadOnlyDictionary<string, IItem>.Keys => Items.Keys;

		IEnumerable<IItem> IReadOnlyDictionary<string, IItem>.Values => Items.Values;

		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo == null) return;
			_columns = (ColumnsCollection)_siInfo.GetValue("Columns", typeof(ColumnsCollection));
			_items = (ItemsCollection)_siInfo.GetValue("Items", typeof(ItemsCollection));
			_siInfo = null;
		}

		public virtual int CompareTo(IBrowsable other) { return BrowsableComparer.Default.Compare(this, other); }

		public virtual int CompareItems(IBrowsable other)
		{
			return CompareTo(other?.Items);
		}

		public virtual bool Equals(IBrowsable other) { return BrowsableComparer.Default.Equals(this, other); }

		public virtual bool AreItemsEqual(IBrowsable other) { return Equals(Items, other?.Items); }

		public virtual bool AreItemsEqual(ItemsCollection other) { return ItemsCollectionComparer.Default.Equals(Items, other); }

		public virtual void Add(IItem item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			Items.Add(item.Name, item);
		}

		public virtual bool Remove(IItem item) { return Items.Remove(item); }

		public virtual void Insert(int index, IItem item) { Items.Insert(index, item); }

		public virtual void Clear() { Items.Clear(); }

		public virtual void RemoveAt(int index) { Items.RemoveAt(index); }

		public bool Contains(IItem item) { return Items.Contains(item); }

		public bool Contains([NotNull] string key) { return Items.Contains(key); }

		public bool Remove([NotNull] string key) { return Items.Remove(key); }

		public int IndexOf(IItem item) { return Items.IndexOf(item); }

		public int IndexOfKey(string key) { return Items.IndexOfKey(key); }

		public void CopyTo(IItem[] array, int arrayIndex) { Items.CopyTo(array, arrayIndex); }

		public bool TryGetValue([NotNull] string key, out IItem value) { return Items.TryGetValue(key, out value); }

		public IEnumerator<IItem> GetEnumerator() { return Items.GetEnumerator(); }

		int IList.Add(object value)
		{
			Add((IItem)value);
			return Count;
		}

		bool IList.Contains(object value) { return Contains((IItem)value); }

		bool IReadOnlyDictionary<string, IItem>.ContainsKey([NotNull] string key) { return Contains(key); }

		void IList.Remove(object value) { Remove((IItem)value); }
		int IList.IndexOf(object value) { return IndexOf((IItem)value); }

		void IList.Insert(int index, object value) { Insert(index, (IItem)value); }

		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

			if (array is IItem[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			 * Catch the obvious case assignment will fail.
			 * We can find all possible problems by doing the check though.
			 * For example, if the element type of the Array is derived from T,
			 * we can't figure out if we can successfully copy the element beforehand.
			 */
			array.Length.ValidateRange(index, Count);
			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(IItem);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;

			try
			{
				foreach (IItem item in this)
				{
					objects[index++] = item;
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}

		bool IReadOnlyDictionary<string, IItem>.TryGetValue(string key, out IItem value) { return TryGetValue(key, out value); }
		void IList.Clear() { Clear(); }

		IEnumerator<KeyValuePair<string, IItem>> IEnumerable<KeyValuePair<string, IItem>>.GetEnumerator() { return ((IEnumerable<KeyValuePair<string, IItem>>)Items).GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		void IList.RemoveAt(int index) { RemoveAt(index); }
	}

	[Serializable]
	public class Browsable<T> : Browsable, IBrowsable<T>
	{
		/// <inheritdoc />
		public Browsable([NotNull] string name)
			: this(name, null, default(T), false, false)
		{
		}

		/// <inheritdoc />
		public Browsable([NotNull] string name, T value)
			: this(name, null, value, false, false)
		{
		}

		/// <inheritdoc />
		public Browsable([NotNull] string name, T value, bool isFixed)
			: this(name, null, value, isFixed, false)
		{
		}

		/// <inheritdoc />
		public Browsable([NotNull] string name, T value, bool isFixed, bool isReadOnly)
			: this(name, null, value, isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		public Browsable([NotNull] string name, string text, T value, bool isFixed, bool isReadOnly)
			: base(name, text, value, typeof(T), isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		protected Browsable([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public new virtual T Value
		{
			get => (T)base.Value;
			set => base.Value = value;
		}

		public new virtual T DefaultValue
		{
			get => (T)base.DefaultValue;
			set => base.DefaultValue = value;
		}
	}
}