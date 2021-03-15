using System;
using System.ComponentModel;
using System.Diagnostics;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Comparers;
using essentialMix.Patterns.NotifyChange;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("{Text} = {Value}")]
	public class SimpleItem : NotifyPropertyChangedBase, ISimpleItem
	{
		private string _text;
		private object _value;
		private object _defaultValue;
		private bool _selected;
		private bool _enabled;
		private object _tag;

		public SimpleItem()
			: this(null, null, typeof(object))
		{
		}

		public SimpleItem(object value)
			: this(null, value, typeof(object))
		{
		}

		public SimpleItem(string text, object value)
			: this(text, value, typeof(object))
		{
		}

		public SimpleItem(string text, object value, [NotNull] Type type)
		{
			ValueType = type ?? throw new ArgumentNullException(nameof(type));
			_value = value;
			_defaultValue = ValueType.Default();
			_text = text.IfNullOrEmpty(() => Convert.ToString(_value ?? _defaultValue));
			_enabled = true;
		}

		public override string ToString() { return _text; }

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public virtual object Clone() { return this.CloneMemberwise(); }

		[Localizable(true)]
		public virtual string Text
		{
			get => _text;
			set
			{
				if (_text == value) return;
				_text = value;
				OnPropertyChanged();
			}
		}

		public virtual object Value
		{
			get => _value;
			set
			{
				if (_value.Equals(value)) return;
				_value = value;
				OnPropertyChanged();
			}
		}

		public virtual object DefaultValue
		{
			get => _defaultValue;
			set
			{
				if (_defaultValue.Equals(value)) return;
				_defaultValue = value;
				OnPropertyChanged();
			}
		}

		public bool Selected
		{
			get => _selected;
			set
			{
				if (_selected == value) return;
				_selected = value;
				OnPropertyChanged();
			}
		}

		public virtual object Tag
		{
			get => _tag;
			set
			{
				if (_tag == value) return;
				_tag = value;
				OnPropertyChanged();
			}
		}

		public bool Enabled
		{
			get => _enabled;
			set
			{
				if (_enabled == value) return;
				_enabled = value;
				OnPropertyChanged();
			}
		}

		public Type ValueType { get; }

		public bool IsPrimitive => ValueType.IsPrimitive();

		public virtual void Reset() { Value = DefaultValue; }

		public virtual int CompareTo(object obj) { return ReferenceComparer<ISimpleItem>.Default.Compare(this, obj); }

		public virtual int CompareTo(ISimpleItem other) { return SimpleItemComparer.Default.Compare(this, other); }

		public virtual bool Equals(ISimpleItem other) { return SimpleItemComparer.Default.Equals(this, other); }

		public virtual bool IsCompatibleObject(object value)
		{
			if (value == null) return ValueType.IsClass;

			if (ValueType.IsEnum)
			{
				if (value is string s)
				{
					try
					{
						object unused = Enum.Parse(ValueType, s, true);
						return true;
					}
					catch
					{
						// ignored
					}
				}

				try
				{
					object unused = value.ToEnum(ValueType);
					return true;
				}
				catch
				{
					// ignored
				}
			}

			return value.Is(ValueType);
		}
	}

	[Serializable]
	public class SimpleItem<T> : SimpleItem, ISimpleItem<T>
	{
		/// <inheritdoc />
		public SimpleItem(T value)
			: this(null, value)
		{
		}

		/// <inheritdoc />
		public SimpleItem(string text, T value)
			: base(text, value, typeof(T))
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