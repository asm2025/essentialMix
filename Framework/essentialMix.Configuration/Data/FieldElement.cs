using System;
using System.Configuration;
using System.Threading;

namespace essentialMix.Configuration.Data;

public class FieldElement : ConfigurationElementBase
{
	private static readonly Lazy<ConfigurationProperty> __nameProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);

	static FieldElement()
	{
		BaseProperties.Add(__nameProperty.Value);
	}

	public FieldElement()
	{
	}

	[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
	public string Name => (string)base[__nameProperty.Value];
}