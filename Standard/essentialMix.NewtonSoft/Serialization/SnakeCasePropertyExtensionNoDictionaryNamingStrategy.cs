namespace essentialMix.Newtonsoft.Serialization
{
	public class SnakeCasePropertyExtensionNoDictionaryNamingStrategy : PropertyExtensionNoDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public SnakeCasePropertyExtensionNoDictionaryNamingStrategy()
			: base(NamingStrategyType.SnakeCase)
		{
		}
	}
}