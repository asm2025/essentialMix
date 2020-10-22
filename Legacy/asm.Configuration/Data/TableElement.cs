using System;
using System.Configuration;
using System.Threading;
using asm.Extensions;

namespace asm.Configuration.Data
{
	public class TableElement : ConfigurationElementBase
	{
		private static readonly Lazy<ConfigurationProperty> __key_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __name_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("name", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __query_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("query", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __fixed_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("fixed", typeof(bool), false), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __filter_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("filter", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __primary_Keys_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty(string.Empty, typeof(FieldSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection), LazyThreadSafetyMode.PublicationOnly);

		static TableElement()
		{
			BaseProperties.Add(__key_Property.Value);
			BaseProperties.Add(__name_Property.Value);
			BaseProperties.Add(__query_Property.Value);
			BaseProperties.Add(__fixed_Property.Value);
			BaseProperties.Add(__filter_Property.Value);
			BaseProperties.Add(__primary_Keys_Property.Value);
		}

		public TableElement()
		{
		}

		[ConfigurationProperty("key", IsRequired = true, IsKey = true)]
		public string Key => (string)base[__key_Property.Value];

		[ConfigurationProperty("name")]
		public string Name => ((string)base[__name_Property.Value]).IfNullOrEmpty(Key);

		[ConfigurationProperty("query", IsRequired = true, IsKey = true)]
		public string Query => (string)base[__query_Property.Value];

		[ConfigurationProperty("fixed")]
		public bool Fixed => (bool)base[__fixed_Property.Value];

		[ConfigurationProperty("filter")]
		public string Filter => (string)base[__filter_Property.Value];

		public FieldElement PrimaryKey => PrimaryKeys.Count == 0 ? null : PrimaryKeys[0];

		[ConfigurationProperty("primaryKeys", IsDefaultCollection = true)]
		public FieldSettingsCollection PrimaryKeys => (FieldSettingsCollection)base[__primary_Keys_Property.Value];
	}
}