namespace asm.Newtonsoft.Serialization
{
	public class PropertyNoExtensionDictionaryNamingStrategy : PropertyExtensionDictionaryNamingStrategy
	{
		/// <inheritdoc />
		public PropertyNoExtensionDictionaryNamingStrategy()
			: this(NamingStrategyType.Default)
		{
		}

		/// <inheritdoc />
		protected PropertyNoExtensionDictionaryNamingStrategy(NamingStrategyType nameStrategy)
			: base(nameStrategy)
		{
			ProcessExtensionDataNames = false;
		}
	}
}