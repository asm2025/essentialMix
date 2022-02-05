namespace essentialMix.Newtonsoft.Serialization;

public class CamelCasePropertyNoExtensionDictionaryNamingStrategy : PropertyNoExtensionDictionaryNamingStrategy
{
	/// <inheritdoc />
	public CamelCasePropertyNoExtensionDictionaryNamingStrategy()
		: base(NamingStrategyType.CamelCase)
	{
	}
}