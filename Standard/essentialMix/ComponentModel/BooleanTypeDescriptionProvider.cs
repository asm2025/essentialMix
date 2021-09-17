using System;
using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel
{
	public sealed class BooleanTypeDescriptionProvider : TypeDescriptionProvider
	{
		private sealed class WrappedTypeDescriptor : ICustomTypeDescriptor
		{
			private readonly ICustomTypeDescriptor _tail;

			public WrappedTypeDescriptor(ICustomTypeDescriptor tail)
			{
				_tail = tail;
			}

			public AttributeCollection GetAttributes() { return _tail.GetAttributes(); }

			public string GetClassName() { return _tail.GetClassName(); }

			public string GetComponentName() { return _tail.GetComponentName(); }

			[NotNull]
			public TypeConverter GetConverter() { return new BooleanConverter(); }

			public EventDescriptor GetDefaultEvent() { return _tail.GetDefaultEvent(); }

			public PropertyDescriptor GetDefaultProperty() { return _tail.GetDefaultProperty(); }

			public object GetEditor(Type editorBaseType) { return _tail.GetEditor(editorBaseType); }

			public EventDescriptorCollection GetEvents() { return _tail.GetEvents(); }

			public EventDescriptorCollection GetEvents(Attribute[] attributes) { return _tail.GetEvents(attributes); }

			public PropertyDescriptorCollection GetProperties() { return _tail.GetProperties(); }

			public PropertyDescriptorCollection GetProperties(Attribute[] attributes) { return _tail.GetProperties(attributes); }

			public object GetPropertyOwner(PropertyDescriptor pd) { return _tail.GetPropertyOwner(pd); }
		}

		private readonly TypeDescriptionProvider _tail;

		private BooleanTypeDescriptionProvider([NotNull] TypeDescriptionProvider tail)
		{
			_tail = tail;
		}

		/// <inheritdoc />
		public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			return _tail.CreateInstance(provider, objectType, argTypes, args);
		}

		/// <inheritdoc />
		public override IDictionary GetCache(object instance) { return _tail.GetCache(instance); }

		/// <inheritdoc />
		[NotNull]
		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			return new WrappedTypeDescriptor(_tail.GetTypeDescriptor(objectType, instance));
		}

		/// <inheritdoc />
		public override bool IsSupportedType(Type type) { return _tail.IsSupportedType(type); }

		public static void Register()
		{
			Type type = typeof(bool);
			TypeDescriptionProvider tail = TypeDescriptor.GetProvider(type);
			if (tail is BooleanTypeDescriptionProvider) return;
			BooleanTypeDescriptionProvider head = new BooleanTypeDescriptionProvider(tail);
			TypeDescriptor.AddProvider(head, type);
		}

		public static void Unregister()
		{
			Type type = typeof(bool);
			TypeDescriptionProvider tail = TypeDescriptor.GetProvider(type);
			if (tail is not BooleanTypeDescriptionProvider) return;
			TypeDescriptor.RemoveProvider(tail, type);
		}
	}
}