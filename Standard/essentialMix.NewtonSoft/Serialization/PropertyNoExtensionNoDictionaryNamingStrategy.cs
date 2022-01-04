namespace essentialMix.Newtonsoft.Serialization;

public class PropertyNoExtensionNoDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
{
	/// <inheritdoc />
	public PropertyNoExtensionNoDictionaryNamingStrategy()
		: this(NamingStrategyType.Default)
	{
	}

	/// <inheritdoc />
	protected PropertyNoExtensionNoDictionaryNamingStrategy(NamingStrategyType nameStrategy)
		: base(nameStrategy)
	{
		ProcessExtensionDataNames = false;
		ProcessDictionaryKeys = false;
	}
}