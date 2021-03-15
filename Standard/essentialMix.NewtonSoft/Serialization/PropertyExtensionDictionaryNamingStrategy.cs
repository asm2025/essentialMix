namespace essentialMix.Newtonsoft.Serialization
{
	public class PropertyExtensionDictionaryNamingStrategy : CustomNamingStrategy
	{
		/// <inheritdoc />
		public PropertyExtensionDictionaryNamingStrategy()
			: this(NamingStrategyType.Default)
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionDictionaryNamingStrategy(NamingStrategyType nameStrategy)
			: base(nameStrategy)
		{
			ProcessExtensionDataNames = true;
			ProcessDictionaryKeys = true;
		}
	}
}