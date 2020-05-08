using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Comparers;

namespace asm.Collections
{
	[Serializable]
	[DebuggerDisplay("{Text} = {Value}")]
	public class SimpleItem : ISimpleItem
	{
		private string _text;
		private object _value;
		private object _defaultValue;
		private bool _selected;
		private bool _enabled;
		private object _tag;

		public SimpleItem()
			: this(null, null, null)
		{
		}

		public SimpleItem(object value)
			: this(null, value, null)
		{
		}

		public SimpleItem(string text, object value)
			: this(text, value, null)
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

		protected SimpleItem([NotNull] SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));
			ValueType = (Type)info.GetValue("ValueType", typeof(Type)) ?? throw new SerializationException("Invalid value type.");
			_value = info.GetValue("Value", ValueType);
			_defaultValue = info.GetValue("DefaultValue", ValueType);
			_text = info.GetString("Text").IfNullOrEmpty(() => Convert.ToString(_value ?? _defaultValue));
			_selected = info.GetBoolean("Selected");
			_enabled = info.GetBoolean("Enabled");
			_tag = info.GetValue("Tag", typeof(object));
		}

		[SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ValueType", ValueType, typeof(Type));
			info.AddValue("Value", _value, ValueType);
			info.AddValue("DefaultValue", _defaultValue, ValueType);
			info.AddValue("Text", _text);
			info.AddValue("Enabled", _enabled);
			info.AddValue("Selected", _selected);
			info.AddValue("Tag", _tag, typeof(object));
		}

		public override string ToString() { return _text; }

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

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
					}
				}

				try
				{
					object unused = value.ToEnum(ValueType);
					return true;
				}
				catch
				{
				}
			}

			return value.Is(ValueType);
		}

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs args) { PropertyChanged?.Invoke(this, args); }

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { OnPropertyChanged(new PropertyChangedEventArgs(propertyName)); }
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

		/// <inheritdoc />
		protected SimpleItem([NotNull] SerializationInfo info, StreamingContext context)
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