using System;
using System.ComponentModel;
using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class TypePropertyDescriptorBase : PropertyDescriptor
	{
		protected const string TARGET_PROPERTY_NAME = "DisplayName";

		private Type _componentType;
		private string _name;
		private string _displayName;
		private string _description;

		protected TypePropertyDescriptorBase([NotNull] string name, Attribute[] attrs) 
			: base(name, attrs)
		{
		}

		protected TypePropertyDescriptorBase([NotNull] MemberDescriptor descriptor) 
			: base(descriptor)
		{
		}

		protected TypePropertyDescriptorBase([NotNull] MemberDescriptor descriptor, Attribute[] attrs) 
			: base(descriptor, attrs)
		{
		}

		public override AttributeCollection Attributes { get; } = new AttributeCollection();
		public override string Name => _name ??= GetName();

		public override string DisplayName => _displayName ??= GetDisplayName();

		[NotNull]
		public override string Description => _description ??= GetDescription();

		public override Type ComponentType => _componentType ??= GetComponentType();

		public override Type PropertyType => GetTargetValue()?.GetType();

		public override object GetValue(object component) { return GetTargetValue(); }

		public override bool CanResetValue(object component) { return !IsReadOnly; }

		public override void ResetValue(object component)
		{
			if (!CanResetValue(component)) return;
			SetTargetValue(PropertyType.Default());
		}

		public override void SetValue(object component, object value)
		{
			SetTargetValue(PropertyType.Default());
		}

		public override bool ShouldSerializeValue(object component) { return PropertyType.IsSerializable; }

		protected abstract string GetName();
		protected abstract Type GetComponentType();
		protected abstract object GetTargetValue();
		protected abstract void SetTargetValue(object value);

		protected virtual string GetDisplayName()
		{
			object target = GetTargetValue();
			if (target == null) return Name;

			if (target is ICustomAttributeProvider provider) return provider.GetDisplayName(Name);

			return target.GetPropertyValue(TARGET_PROPERTY_NAME, out string displayName, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, typeof(string), Name) ? displayName : Name;
		}

		[NotNull]
		protected virtual string GetDescription()
		{
			object target = GetTargetValue();
			if (target == null) return string.Empty;

			ICustomAttributeProvider provider = target as ICustomAttributeProvider;
			return provider?.GetDescription(string.Empty) ?? string.Empty;
		}
	}
}