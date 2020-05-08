using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class Item : Property, IItem
	{
		[Serializable]
		public class SubItemsCollection : ObservableKeyedCollection<string, SubItem>
		{
			[NonSerialized] 
			private SerializationInfo _siInfo;

			protected SubItemsCollection(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				_siInfo = info;
			}

			internal SubItemsCollection([NotNull] Item owner)
				: base(StringComparer.OrdinalIgnoreCase)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue("Owner", Owner, typeof(Item));
			}

			public override void OnDeserialization(object sender)
			{
				base.OnDeserialization(sender);
				if (_siInfo == null) return;
				Owner = (Item)_siInfo.GetValue("Owner", typeof(Item)) ?? throw new SerializationException("Invalid owner type.");
				_siInfo = null;
			}

			/// <inheritdoc />
			protected override void InsertItem(int index, SubItem item)
			{
				if (item == null) throw new ArgumentNullException(nameof(item));
				item.Column = Owner.Parent.Columns[index];
				base.InsertItem(index, item);
			}

			/// <inheritdoc />
			protected override void SetItem(int index, SubItem item)
			{
				if (item == null) throw new ArgumentNullException(nameof(item));
				item.Column = Owner.Parent.Columns[index];
				base.SetItem(index, item);
			}

			public override bool Remove(SubItem item)
			{
				if (item == null) throw new ArgumentNullException(nameof(item));
				if (item.IsFixed || !base.Remove(item)) return false;
				item.Owner = null;
				return true;
			}

			public override bool Remove(string key)
			{
				SubItem item = base[key];
				if (item == null || item.IsFixed || !base.Remove(key)) return false;
				item.Owner = null;
				return true;
			}

			public override void RemoveAt(int index)
			{
				SubItem item = base[index];
				if (item.IsFixed) return;
				base.RemoveAt(index);
			}

			protected override void ClearItems()
			{
				if (Count == 0) return;
				this.RemoveAll(item => !item.IsFixed);
			}

			[NotNull] protected override string GetKeyForItem(SubItem item) { return item.Column.Name; }

			public Item Owner { get; private set; }
		}

		private bool _selected;
		private SubItemsCollection _subItems;

		public Item([NotNull] string name)
			: this(name, null, null, null, false, false)
		{
		}

		public Item([NotNull] string name, object value)
			: this(name, null, value, null, false, false)
		{
		}

		public Item([NotNull] string name, string text, object value)
			: this(name, text, value, null, false, false)
		{
		}

		public Item([NotNull] string name, object value, bool isFixed, bool isReadOnly)
			: this(name, null, value, null, isFixed, isReadOnly)
		{
		}

		public Item([NotNull] string name, string text, object value, bool isFixed, bool isReadOnly)
			: this(name, text, value, null, isFixed, isReadOnly)
		{
		}

		public Item([NotNull] string name, string text, object value, Type type, bool isFixed, bool isReadOnly)
			: base(name, text, value, type, isFixed, isReadOnly)
		{
			IsContainer = GetType().Is(typeof(IBrowsable));
		}

		protected Item([NotNull] SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
			IsContainer = GetType().Is(typeof(IBrowsable));
			_selected = info.GetBoolean("Selected");
			_subItems = (SubItemsCollection)info.GetValue("SubItems", typeof(SubItemsCollection));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Selected", _selected);
			info.AddValue("SubItems", _subItems, typeof(SubItemsCollection));
		}

		public override object Clone() { return this.CloneMemberwise(); }

		public virtual bool Selected
		{
			get => _selected;
			set
			{
				if (_selected == value) return;
				_selected = value;
				OnPropertyChanged();
			}
		}

		public bool IsContainer { get; }

		public IBrowsable Parent { get; internal set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotNull]
		public virtual SubItemsCollection SubItems => _subItems ??= new SubItemsCollection(this);

		public virtual int CompareTo(IItem other) { return ItemComparer.Default.Compare(this, other); }

		public virtual bool Equals(IItem other) { return ItemComparer.Default.Equals(this, other); }
	}

	[Serializable]
	public class Item<T> : Item, IItem<T>
	{
		/// <inheritdoc />
		public Item([NotNull] string name)
			: this(name, null, default(T), false, false)
		{
		}

		/// <inheritdoc />
		public Item([NotNull] string name, T value)
			: this(name, null, value, false, false)
		{
		}

		/// <inheritdoc />
		public Item([NotNull] string name, T value, bool isFixed)
			: this(name, null, value, isFixed, false)
		{
		}

		/// <inheritdoc />
		public Item([NotNull] string name, T value, bool isFixed, bool isReadOnly)
			: this(name, null, value, isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		public Item([NotNull] string name, string text, T value, bool isFixed, bool isReadOnly)
			: base(name, text, value, typeof(T), isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		protected Item([NotNull] SerializationInfo info, StreamingContext context)
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