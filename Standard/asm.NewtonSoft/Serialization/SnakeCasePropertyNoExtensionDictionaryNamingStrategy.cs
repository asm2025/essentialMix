namespace asm.Newtonsoft.Serialization
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