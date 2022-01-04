using System;
using System.Configuration;
using System.Threading;

namespace essentialMix.Configuration.Data;

public sealed class DataSection : ConfigurationSectionBase
{
	private static readonly Lazy<ConfigurationProperty> __tablesProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty(string.Empty, typeof(TableSettingsCollection), null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection), LazyThreadSafetyMode.PublicationOnly);

	static DataSection()
	{
		BaseProperties.Add(__tablesProperty.Value);
	}

	public DataSection()
	{
	}

	[ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
	public TableSettingsCollection Tables => (TableSettingsCollection)base[__tablesProperty.Value];
}