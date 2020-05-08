namespace asm.Newtonsoft.Serialization
{
	public class PropertyExtensionNoDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public PropertyExtensionNoDictionaryNamingStrategy()
			: this(NamingStrategyType.Default)
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionNoDictionaryNamingStrategy(NamingStrategyType nameStrategy)
			: base(nameStrategy)
		{
			ProcessDictionaryKeys = false;
		}
	}
}