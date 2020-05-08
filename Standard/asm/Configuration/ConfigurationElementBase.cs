using System.Configuration;

namespace asm.Configuration
{
	public abstract class ConfigurationElementBase : ConfigurationElement
	{
		protected ConfigurationElementBase()
		{
		}

		protected static ConfigurationPropertyCollection BaseProperties { get; } = new ConfigurationPropertyCollection();
		protected override ConfigurationPropertyCollection Properties => BaseProperties;
	}
}