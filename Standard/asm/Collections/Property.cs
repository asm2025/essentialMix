using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using asm.Extensions;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[DebuggerDisplay("{Name} = {Value}")]
	public class Property : Header, IProperty
	{
		private object _value;
		private object _defaultValue;
		private Scope _scope;

		public Property([NotNull] string name)
			: this(name, null, null, null, false, false)
		{
		}

		public Property([NotNull] string name, object value)
			: this(name, null, value, null, false, false)
		{
		}

		public Property([NotNull] string name, object value, bool isFixed)
			: this(name, null, value, null, isFixed, false)
		{
		}

		public Property([NotNull] string name, object value, bool isFixed, bool isReadOnly)
			: this(name, null, value, null, isFixed, isReadOnly)
		{
		}

		public Property([NotNull] string name, string text, object value, bool isFixed, bool isReadOnly)
			: this(name, text, value, null, isFixed, isReadOnly)
		{
		}

		public Property([NotNull] string name, string text, object value, Type type, bool isFixed, bool isReadOnly)
			: base(name, type ?? value?.GetType() ?? typeof(object), text, isFixed)
		{
			_value = value;
			_defaultValue = ValueType.Default();
			IsReadOnly = isReadOnly;
		}

		protected Property([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_value = info.GetValue("Value", ValueType);
			_defaultValue = info.GetValue("DefaultValue", ValueType);
			_scope = (Scope)info.GetValue("Scope", typeof(Scope));
			IsReadOnly = info.GetBoolean("IsReadOnly");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Value", _value, ValueType);
			info.AddValue("DefaultValue", _defaultValue, ValueType);
			info.AddValue("Scope", _scope, typeof(Scope));
			info.AddValue("IsReadOnly", IsReadOnly);
		}

		public override object Clone() { return this.CloneMemberwise(); }

		public virtual object Value
		{
			get => _value;
			set
			{
				if (IsReadOnly) throw new ReadOnlyException();
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

		public virtual Scope Scope
		{
			get => _scope;
			set
			{
				if (_scope == value) return;
				_scope = value;
				OnPropertyChanged();
			}
		}

		public virtual bool IsReadOnly { get; }

		public bool IsPrimitive => ValueType.IsPrimitive();

		public virtual void Reset() { Value = DefaultValue; }

		public virtual int CompareTo(IProperty other) { return PropertyComparer.Default.Compare(this, other); }

		public virtual bool Equals(IProperty other) { return PropertyComparer.Default.Equals(this, other); }
	}

	[Serializable]
	public class Property<T> : Property, IProperty<T>
	{
		/// <inheritdoc />
		public Property([NotNull] string name)
			: this(name, null, default(T), false, false)
		{
		}

		/// <inheritdoc />
		public Property([NotNull] string name, T value)
			: this(name, null, value, false, false)
		{
		}

		/// <inheritdoc />
		public Property([NotNull] string name, T value, bool isFixed)
			: this(name, null, value, isFixed, false)
		{
		}

		/// <inheritdoc />
		public Property([NotNull] string name, T value, bool isFixed, bool isReadOnly)
			: this(name, null, value, isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		public Property([NotNull] string name, string text, T value, bool isFixed, bool isReadOnly)
			: base(name, text, value, typeof(T), isFixed, isReadOnly)
		{
		}

		/// <inheritdoc />
		protected Property([NotNull] SerializationInfo info, StreamingContext context) 
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