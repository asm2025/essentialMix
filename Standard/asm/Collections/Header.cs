using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Comparers;

namespace asm.Collections
{
	[Serializable]
	public abstract class Header : IHeader
	{
		private string _name;
		private string _text;
		private object _tag;
		private bool _enabled;

		protected Header([NotNull] string name, [NotNull] Type type)
			: this(name, type, null, false) { }

		protected Header([NotNull] string name, [NotNull] Type type, string text)
			: this(name, type, text, false) { }

		protected Header([NotNull] string name, [NotNull] Type type, bool isFixed)
			: this(name, type, null, isFixed) { }

		protected Header([NotNull] string name, [NotNull] Type type, string text, bool isFixed)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
			ValueType = type ?? throw new ArgumentNullException(nameof(type));
			_name = name;
			_text = text.IfNullOrEmpty(() => _name);
			_enabled = true;
			IsFixed = isFixed;
		}

		public override string ToString() { return _text.IfNullOrEmpty(() => _name); }

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual string Name
		{
			get => _name;
			set
			{
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
				if (_name == value) return;
				_name = value;
				OnNameChanged();
				OnPropertyChanged();
			}
		}

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

		public virtual bool IsFixed { get; }

		public Type ValueType { get; }

		public abstract object Clone();

		public virtual int CompareTo(object obj) { return ReferenceComparer<IHeader>.Default.Compare(this, obj); }

		public virtual int CompareTo(IHeader other) { return HeaderComparer.Default.Compare(this, other); }

		public virtual bool Equals(IHeader other) { return HeaderComparer.Default.Equals(this, other); }

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

		protected virtual void OnNameChanged() { }

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs args) { PropertyChanged?.Invoke(this, args); }

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { OnPropertyChanged(new PropertyChangedEventArgs(propertyName)); }
	}
}