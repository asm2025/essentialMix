namespace asm.Newtonsoft.Serialization
{
	public class CamelCasePropertyNoExtensionNoDictionaryNamingStrategy : PropertyNoExtensionNoDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public CamelCasePropertyNoExtensionNoDictionaryNamingStrategy()
			: base(NamingStrategyType.CamelCase)
		{
		}
	}
}