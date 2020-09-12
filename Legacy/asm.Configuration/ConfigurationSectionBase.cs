using System.Configuration;

namespace asm.Configuration
{
	public abstract class ConfigurationSectionBase : ConfigurationSection
	{
		protected ConfigurationSectionBase()
		{
		}

		protected static ConfigurationPropertyCollection BaseProperties { get; } = new ConfigurationPropertyCollection();
		protected override ConfigurationPropertyCollection Properties => BaseProperties;
	}
}