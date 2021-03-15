using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel
{
	/// <inheritdoc />
	/// <summary>
	/// https://www.codeproject.com/Articles/12615/Automatic-Expandable-Properties-in-a-PropertyGrid
	/// </summary>
	/// <seealso cref="T:System.ComponentModel.PropertyDescriptor" />
	public class DefaultPropertyDescriptor : PropertyDescriptor
	{
		private bool? _isReadOnly;

		/// <inheritdoc />
		public DefaultPropertyDescriptor([NotNull] PropertyInfo property)
			: base(property.Name, property.GetAttributes(true).ToArray())
		{
			Property = property;
		}

		public override bool IsReadOnly
		{
			get
			{
				_isReadOnly ??= Property.GetSetMethod() == null;
				return _isReadOnly.Value;
			}
		}

		public override Type ComponentType => Property.DeclaringType;

		[NotNull]
		public override Type PropertyType => Property.PropertyType;

		public override void ResetValue(object component) { }
		public override bool CanResetValue(object component) { return false; }

		public override bool ShouldSerializeValue(object component) { return true; }

		public override object GetValue(object component) { return Property.GetValue(component, null); }

		public override void SetValue(object component, object value)
		{
			Property.SetValue(component, value, null);
			OnValueChanged(component, EventArgs.Empty);
		}

		public override int GetHashCode() { return Property.GetHashCode(); }

		public override bool Equals(object obj) { return obj is DefaultPropertyDescriptor other && other.Property.Equals(Property); }

		[NotNull]
		public PropertyInfo Property { get; }
	}
}