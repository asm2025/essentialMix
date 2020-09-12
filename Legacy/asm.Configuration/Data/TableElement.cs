using System.Configuration;
using asm.Extensions;

namespace asm.Configuration.Data
{
	public class TableElement : ConfigurationElementBase
	{
		private static readonly ConfigurationProperty KEY_PROPERTY;
		private static readonly ConfigurationProperty NAME_PROPERTY;
		private static readonly ConfigurationProperty QUERY_PROPERTY;
		private static readonly ConfigurationProperty FIXED_PROPERTY;
		private static readonly ConfigurationProperty FILTER_PROPERTY;
		private static readonly ConfigurationProperty PRIMARY_KEYS_PROPERTY;

		static TableElement()
		{
			KEY_PROPERTY = new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			BaseProperties.Add(KEY_PROPERTY);

			NAME_PROPERTY = new ConfigurationProperty("name", typeof(string), null);
			BaseProperties.Add(NAME_PROPERTY);

			QUERY_PROPERTY = new ConfigurationProperty("query", typeof(string), null);
			BaseProperties.Add(QUERY_PROPERTY);

			FIXED_PROPERTY = new ConfigurationProperty("fixed", typeof(bool), false);
			BaseProperties.Add(FIXED_PROPERTY);

			FILTER_PROPERTY = new ConfigurationProperty("filter", typeof(string), null);
			BaseProperties.Add(FILTER_PROPERTY);

			PRIMARY_KEYS_PROPERTY = new ConfigurationProperty(string.Empty, typeof(FieldSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
			BaseProperties.Add(PRIMARY_KEYS_PROPERTY);
		}

		public TableElement()
		{
		}

		[ConfigurationProperty("key", IsRequired = true, IsKey = true)]
		public string Key => (string)base[KEY_PROPERTY];

		[ConfigurationProperty("name")]
		public string Name => ((string)base[NAME_PROPERTY]).IfNullOrEmpty(Key);

		[ConfigurationProperty("query", IsRequired = true, IsKey = true)]
		public string Query => (string)base[QUERY_PROPERTY];

		[ConfigurationProperty("fixed")]
		public bool Fixed => (bool)base[FIXED_PROPERTY];

		[ConfigurationProperty("filter")]
		public string Filter => (string)base[FILTER_PROPERTY];

		public FieldElement PrimaryKey => PrimaryKeys.Count == 0 ? null : PrimaryKeys[0];

		[ConfigurationProperty("primaryKeys", IsDefaultCollection = true)]
		public FieldSettingsCollection PrimaryKeys => (FieldSettingsCollection)base[PRIMARY_KEYS_PROPERTY];
	}
}