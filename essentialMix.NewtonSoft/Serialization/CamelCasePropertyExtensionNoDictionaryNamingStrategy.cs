namespace essentialMix.Newtonsoft.Serialization;

public class CamelCasePropertyExtensionNoDictionaryNamingStrategy : PropertyExtensionNoDictionaryNamingStrategy
{
	/// <inheritdoc />
	public CamelCasePropertyExtensionNoDictionaryNamingStrategy()
		: base(NamingStrategyType.CamelCase)
	{
	}
}