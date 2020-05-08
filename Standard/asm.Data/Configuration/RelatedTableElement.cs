using System.Configuration;
using asm.Extensions;

namespace asm.Data.Configuration
{
	public class RelatedTableElement : TableElement
	{
		private static readonly ConfigurationProperty FOREIGN_KEY_PROPERTY;
		private static readonly ConfigurationProperty RELATED_FIELD_PROPERTY;
		private static readonly ConfigurationProperty TYPE_PROPERTY;
		
		static RelatedTableElement()
		{
			FOREIGN_KEY_PROPERTY = new ConfigurationProperty("foreignKey", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
			BaseProperties.Add(FOREIGN_KEY_PROPERTY);

			RELATED_FIELD_PROPERTY = new ConfigurationProperty("relatedField", typeof(string), null);
			BaseProperties.Add(RELATED_FIELD_PROPERTY);

			TYPE_PROPERTY = new ConfigurationProperty("type", typeof(RelatedTableTypeEnum), null, ConfigurationPropertyOptions.IsRequired);
			BaseProperties.Add(TYPE_PROPERTY);
		}

		public RelatedTableElement()
		{
		}

		[ConfigurationProperty("foreignKey", IsRequired = true)]
		public string ForeignKey => (string)base[FOREIGN_KEY_PROPERTY];

		[ConfigurationProperty("relatedField")]
		public string RelatedField => ((string)base[RELATED_FIELD_PROPERTY]).IfNullOrEmpty(PrimaryKey?.Name);

		[ConfigurationProperty("type", IsRequired = true)]
		public RelatedTableTypeEnum Type => (RelatedTableTypeEnum)base[TYPE_PROPERTY];
	}
}