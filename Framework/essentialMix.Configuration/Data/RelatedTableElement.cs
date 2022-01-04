using System;
using System.Configuration;
using System.Threading;
using essentialMix.Data;
using essentialMix.Extensions;

namespace essentialMix.Configuration.Data;

public class RelatedTableElement : TableElement
{
	private static readonly Lazy<ConfigurationProperty> __foreignKeyProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("foreignKey", typeof(string), null, ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<ConfigurationProperty> __relatedFieldProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("relatedField", typeof(string), null), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<ConfigurationProperty> __typeProperty = new Lazy<ConfigurationProperty>(() => new ConfigurationProperty("type", typeof(RelatedTableTypeEnum), null, ConfigurationPropertyOptions.IsRequired), LazyThreadSafetyMode.PublicationOnly);
		
	static RelatedTableElement()
	{
		BaseProperties.Add(__foreignKeyProperty.Value);
		BaseProperties.Add(__relatedFieldProperty.Value);
		BaseProperties.Add(__typeProperty.Value);
	}

	public RelatedTableElement()
	{
	}

	[ConfigurationProperty("foreignKey", IsRequired = true)]
	public string ForeignKey => (string)base[__foreignKeyProperty.Value];

	[ConfigurationProperty("relatedField")]
	public string RelatedField => ((string)base[__relatedFieldProperty.Value]).IfNullOrEmpty(PrimaryKey?.Name);

	[ConfigurationProperty("type", IsRequired = true)]
	public RelatedTableTypeEnum Type => (RelatedTableTypeEnum)base[__typeProperty.Value];
}