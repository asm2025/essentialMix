namespace essentialMix.Newtonsoft.Serialization
{
	public class SnakeCasePropertyNoExtensionDictionaryNamingStrategy : PropertyNoExtensionDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public SnakeCasePropertyNoExtensionDictionaryNamingStrategy()
			: base(NamingStrategyType.SnakeCase)
		{
		}
	}
}