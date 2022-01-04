using System.Configuration;

namespace essentialMix.Configuration;

public abstract class ConfigurationSectionBase : ConfigurationSection
{
	protected ConfigurationSectionBase()
	{
	}

	protected static ConfigurationPropertyCollection BaseProperties { get; } = new ConfigurationPropertyCollection();
	protected override ConfigurationPropertyCollection Properties => BaseProperties;
}