using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel;

/// <inheritdoc />
/// <summary>
/// This class is a modified version of a class from Stephen Taub's NET Matters, ICustomTypeDescriptor article in NET
/// Matter April/May 2005
/// https://www.codeproject.com/Articles/12615/Automatic-Expandable-Properties-in-a-PropertyGrid
/// </summary>
public class ExpandablePropertiesTypeDescriptionProvider : TypeDescriptionProvider
{
	/// <inheritdoc />
	/// <summary>
	/// This class is a modified version of a class from Stephen Taub's NET Matters, ICustomTypeDescriptor article:
	/// </summary>
	private class ExpandablePropertiesTypeDescriptor(
		[NotNull] ExpandablePropertiesTypeDescriptionProvider provider,
		[NotNull] ICustomTypeDescriptor descriptor,
		[NotNull] Type objectType)
		: CustomTypeDescriptor(descriptor)
	{
		private readonly Type _objectType = objectType;
		private readonly ExpandablePropertiesTypeDescriptionProvider _provider = provider;

		public override PropertyDescriptorCollection GetProperties() { return GetProperties(null); }

		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			// Retrieve cached properties and filtered properties
			bool filtering = attributes is { Length: > 0 };
			FilterCache cache = _provider._filterCache;
			PropertyDescriptorCollection properties = _provider._propCache;

			// Use a cached version if we can
			if (filtering && cache != null && cache.IsValid(attributes)) return cache.FilteredProperties;
			if (!filtering && properties != null) return properties;

			// Otherwise, create the property collection
			properties = new PropertyDescriptorCollection(null);

			foreach (PropertyInfo property in _objectType.GetProperties())
			{
				// FieldInfo[] pflds = p.PropertyType.GetFields();
				PropertyInfo[] propertyInfos = property.PropertyType.GetProperties();
				// if the property in not an array and has public fields or properties - use ExpandablePropertyDescriptor
				PropertyDescriptor desc = !property.PropertyType.HasElementType && propertyInfos.Length > 0 
											? new ExpandablePropertyDescriptor(property) 
											: new DefaultPropertyDescriptor(property);

				if (!filtering || desc.Attributes.Contains(attributes)) properties.Add(desc);
			}

			// Store the updated properties
			if (filtering)
			{
				cache = new FilterCache
				{
					FilteredProperties = properties,
					Attributes = attributes
				};
				_provider._filterCache = cache;
			}
			else
				_provider._propCache = properties;

			// Return the computed properties
			return properties;
		}
	}

	/// <inheritdoc />
	public class ExpandablePropertyDescriptor([NotNull] PropertyInfo property) : DefaultPropertyDescriptor(property)
	{
		private TypeConverter _converter;

		public override TypeConverter Converter =>
			_converter ??= base.Converter?.GetType() != typeof(TypeConverter)
								? base.Converter
								: new ExpandableObjectConverter();
	}

	/// <summary>
	/// This class is a direct lift from Stephen Taub's NET Matters, ICustomTypeDescriptor article in NET Matter April/May
	/// 2005
	/// </summary>
	private class FilterCache
	{
		public Attribute[] Attributes;
		public PropertyDescriptorCollection FilteredProperties;

		public FilterCache() { }

		/// <summary>
		/// Perform straight-match on attributes: non-null, same-length, same contents
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsValid(Attribute[] other)
		{
			if (other == null || Attributes == null) return false;
			if (Attributes.Length != other.Length) return false;
			return !other.Where((t, i) => !Attributes[i].Match(t)).Any();
		}
	}

	private readonly TypeDescriptionProvider _baseProvider;
	private PropertyDescriptorCollection _propCache;
	private FilterCache _filterCache;

	/// <inheritdoc />
	public ExpandablePropertiesTypeDescriptionProvider([NotNull] Type type) { _baseProvider = TypeDescriptor.GetProvider(type); }

	public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
	{
		ICustomTypeDescriptor typeDescriptor = _baseProvider.GetTypeDescriptor(objectType, instance);
		return typeDescriptor == null 
					? null
					: new ExpandablePropertiesTypeDescriptor(this, typeDescriptor, objectType);
	}
}