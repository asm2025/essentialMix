using System;
using System.Configuration;
using System.Threading;

namespace asm.Configuration.Data
{
	public class FieldElement : ConfigurationElementBase
	{
		private static readonly Lazy<ConfigurationProperty> __name_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);

		static FieldElement()
		{
			BaseProperties.Add(__name_Property.Value);
		}

		public FieldElement()
		{
		}

		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name => (string)base[__name_Property.Value];
	}
}