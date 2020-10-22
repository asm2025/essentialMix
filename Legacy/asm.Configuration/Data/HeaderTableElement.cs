using System;
using System.Configuration;
using System.Threading;

namespace asm.Configuration.Data
{
	public class HeaderTableElement : TableElement
	{
		private static readonly Lazy<ConfigurationProperty> __related_Tables_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty(string.Empty, typeof(RelatedTableSettingsCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection), LazyThreadSafetyMode.PublicationOnly);

		static HeaderTableElement()
		{
			BaseProperties.Add(__related_Tables_Property.Value);
		}

		public HeaderTableElement()
		{
		}

		[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
		public RelatedTableSettingsCollection RelatedTables => (RelatedTableSettingsCollection)base[__related_Tables_Property.Value];
	}
}