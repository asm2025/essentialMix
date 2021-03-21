using System;
using System.Configuration;
using System.Threading;

namespace essentialMix.Configuration.Data
{
	public class HeaderTableElement : TableElement
	{
		private static readonly Lazy<ConfigurationProperty> __relatedTablesProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty(string.Empty, typeof(RelatedTableSettingsCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection), LazyThreadSafetyMode.PublicationOnly);

		static HeaderTableElement()
		{
			BaseProperties.Add(__relatedTablesProperty.Value);
		}

		public HeaderTableElement()
		{
		}

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public RelatedTableSettingsCollection RelatedTables => (RelatedTableSettingsCollection)base[__relatedTablesProperty.Value];
	}
}