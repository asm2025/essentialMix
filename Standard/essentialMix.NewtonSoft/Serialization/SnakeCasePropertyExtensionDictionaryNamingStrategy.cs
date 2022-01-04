namespace essentialMix.Newtonsoft.Serialization;

public class SnakeCasePropertyExtensionDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
{
	/// <inheritdoc />
	public SnakeCasePropertyExtensionDictionaryNamingStrategy()
		: base(NamingStrategyType.SnakeCase)
	{
	}
}