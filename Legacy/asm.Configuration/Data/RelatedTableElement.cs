using System;
using System.Configuration;
using System.Threading;
using asm.Data;
using asm.Extensions;

namespace asm.Configuration.Data
{
	public class RelatedTableElement : TableElement
	{
		private static readonly Lazy<ConfigurationProperty> __foreign_Key_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("foreignKey", typeof(string), null, ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __related_Field_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("relatedField", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<ConfigurationProperty> __type_Property = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("type", typeof(RelatedTableTypeEnum), null, ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
		
		static RelatedTableElement()
		{
			BaseProperties.Add(__foreign_Key_Property.Value);
			BaseProperties.Add(__related_Field_Property.Value);
			BaseProperties.Add(__type_Property.Value);
		}

		public RelatedTableElement()
		{
		}

		[ConfigurationProperty("foreignKey", IsRequired = true)]
		public string ForeignKey => (string)base[__foreign_Key_Property.Value];

		[ConfigurationProperty("relatedField")]
		public string RelatedField => ((string)base[__related_Field_Property.Value]).IfNullOrEmpty(PrimaryKey?.Name);

		[ConfigurationProperty("type", IsRequired = true)]
		public RelatedTableTypeEnum Type => (RelatedTableTypeEnum)base[__type_Property.Value];
	}
}