using System.Configuration;
using asm.Configuration;

namespace asm.Data.Configuration
{
	public class FieldElement : ConfigurationElementBase
	{
		private static readonly ConfigurationProperty NAME_PROPERTY;

		static FieldElement()
		{
			NAME_PROPERTY = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
			BaseProperties.Add(NAME_PROPERTY);
		}

		public FieldElement()
		{
		}

		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name => (string)base[NAME_PROPERTY];
	}
}