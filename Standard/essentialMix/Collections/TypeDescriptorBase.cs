using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[TypeConverter(typeof(WrapperEmptyDisplayNameExpandableObjectConverter))]
	public abstract class TypeDescriptorBase<TSource> : ICustomTypeDescriptor, IWrapper<TSource>
	{
		protected TypeDescriptorBase([NotNull] TSource source)
		{
			Source = source;
		}

		public TSource Source { get; }

		[NotNull]
		public virtual AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(Source, true); }

		public virtual string GetClassName() { return TypeDescriptor.GetClassName(Source, true); }

		public virtual string GetComponentName() { return TypeDescriptor.GetComponentName(Source, true); }

		public virtual TypeConverter GetConverter() { return TypeDescriptor.GetConverter(Source, true); }

		public virtual EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(Source, true); }

		public virtual PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(Source, true); }

		public virtual object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(Source, editorBaseType, true); }

		[NotNull]
		public virtual EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(Source, true); }

		[NotNull]
		public virtual EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(Source, attributes, true); }

		[NotNull]
		public virtual object GetPropertyOwner(PropertyDescriptor pd) { return Source; }

		public abstract PropertyDescriptorCollection GetProperties();
		public abstract PropertyDescriptorCollection GetProperties(Attribute[] attributes);
	}
}