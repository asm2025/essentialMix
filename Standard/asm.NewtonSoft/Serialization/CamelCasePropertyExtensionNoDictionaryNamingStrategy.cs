namespace asm.Newtonsoft.Serialization
{
	public class CamelCasePropertyExtensionNoDictionaryNamingStrategy : PropertyExtensionNoDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public CamelCasePropertyExtensionNoDictionaryNamingStrategy()
			: base(NamingStrategyType.CamelCase)
		{
		}
	}
}