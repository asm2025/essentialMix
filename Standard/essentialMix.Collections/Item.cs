using System;
using System.ComponentModel;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class Item : Property, IItem
	{
		[Serializable]
		public class SubItemsCollection : ObservableKeyedCollectionBase<string, SubItem>
		{
			internal SubItemsCollection([NotNull] Item owner)
				: base(StringComparer.OrdinalIgnoreCase)
			{
				Owner = owner ?? throw new ArgumentNullException(nameof(owner));
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

			/// <inheritdoc />
			protected override void RemoveItem(int index)
			{
				SubItem item = base[index];
				if (item.IsFixed) return;
				base.RemoveItem(index);
				item.Owner = null;
			}

			protected override void ClearItems()
			{
				if (Count == 0) return;

				for (int i = Items.Count - 1; i >= 0; i--)
				{
					if (Items[i].IsFixed) continue;
					RemoveAt(i);
				}
			}

			[NotNull]
			protected override string GetKeyForItem(SubItem item) { return item.Column.Name; }

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

		public Browsable Parent { get; internal set; }

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