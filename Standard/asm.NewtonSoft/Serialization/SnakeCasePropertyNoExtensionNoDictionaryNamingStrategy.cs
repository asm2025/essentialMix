namespace asm.Newtonsoft.Serialization
{
	public class SnakeCasePropertyNoExtensionNoDictionaryNamingStrategy : PropertyNoExtensionNoDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public SnakeCasePropertyNoExtensionNoDictionaryNamingStrategy()
			: base(NamingStrategyType.SnakeCase)
		{
		}
	}
}