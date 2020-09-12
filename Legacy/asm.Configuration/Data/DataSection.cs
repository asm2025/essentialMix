using System.Configuration;

namespace asm.Configuration.Data
{
	public sealed class DataSection : ConfigurationSectionBase
	{
		private static readonly ConfigurationProperty TABLES_PROPERTY;

		static DataSection()
		{
			TABLES_PROPERTY = new ConfigurationProperty(string.Empty, typeof(TableSettingsCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);
			BaseProperties.Add(TABLES_PROPERTY);
		}

		public DataSection()
		{
		}

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public TableSettingsCollection Tables => (TableSettingsCollection)base[TABLES_PROPERTY];
	}
}