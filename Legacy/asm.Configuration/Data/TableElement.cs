using System;
using System.Configuration;
using System.Threading;
using asm.Extensions;

namespace asm.Configuration.Data
{
	public class TableElement : ConfigurationElementBase
	{
		private static readonly Lazy<ConfigurationProperty> __keyProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __nameProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("name", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __queryProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("query", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __fixedProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("fixed", typeof(bool), false), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __filterProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("filter", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __primaryKeysProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty(string.Empty, typeof(FieldSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection), LazyThreadSafetyMode.PublicationOnly);

		static TableElement()
		{
			BaseProperties.Add(__keyProperty.Value);
			BaseProperties.Add(__nameProperty.Value);
			BaseProperties.Add(__queryProperty.Value);
			BaseProperties.Add(__fixedProperty.Value);
			BaseProperties.Add(__filterProperty.Value);
			BaseProperties.Add(__primaryKeysProperty.Value);
		}

		public TableElement()
		{
		}

		[ConfigurationProperty("key", IsRequired = true, IsKey = true)]
		public string Key => (string)base[__keyProperty.Value];

		[ConfigurationProperty("name")]
		public string Name => ((string)base[__nameProperty.Value]).IfNullOrEmpty(Key);

		[ConfigurationProperty("query", IsRequired = true, IsKey = true)]
		public string Query => (string)base[__queryProperty.Value];

		[ConfigurationProperty("fixed")]
		public bool Fixed => (bool)base[__fixedProperty.Value];

		[ConfigurationProperty("filter")]
		public string Filter => (string)base[__filterProperty.Value];

		public FieldElement PrimaryKey => PrimaryKeys.Count == 0 ? null : PrimaryKeys[0];

		[ConfigurationProperty("primaryKeys", IsDefaultCollection = true)]
		public FieldSettingsCollection PrimaryKeys => (FieldSettingsCollection)base[__primaryKeysProperty.Value];
	}
}