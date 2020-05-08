using System.Configuration;

namespace asm.Data.Configuration
{
	public class HeaderTableElement : TableElement
	{
		private static readonly ConfigurationProperty RELATED_TABLES_PROPERTY;

		static HeaderTableElement()
		{
			RELATED_TABLES_PROPERTY = new ConfigurationProperty(string.Empty, typeof(RelatedTableSettingsCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);
			BaseProperties.Add(RELATED_TABLES_PROPERTY);
		}

		public HeaderTableElement()
		{
		}

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public RelatedTableSettingsCollection RelatedTables => (RelatedTableSettingsCollection)base[RELATED_TABLES_PROPERTY];
	}
}