namespace asm.Newtonsoft.Serialization
{
	public class SnakeCasePropertyExtensionDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public SnakeCasePropertyExtensionDictionaryNamingStrategy()
			: base(NamingStrategyType.SnakeCase)
		{
		}
	}
}