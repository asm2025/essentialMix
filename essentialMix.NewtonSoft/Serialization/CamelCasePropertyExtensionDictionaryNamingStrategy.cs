namespace essentialMix.Newtonsoft.Serialization;

public class CamelCasePropertyExtensionDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
{
	/// <inheritdoc />
	public CamelCasePropertyExtensionDictionaryNamingStrategy()
		: base(NamingStrategyType.CamelCase)
	{
	}
}