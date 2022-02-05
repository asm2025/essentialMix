using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Newtonsoft.Serialization;

public abstract class CustomNamingStrategy : NamingStrategy
{
	private static readonly DefaultNamingStrategy __defaultNaming = new DefaultNamingStrategy();
	private static readonly CamelCaseNamingStrategy __camelCaseNaming = new CamelCaseNamingStrategy();
	private static readonly SnakeCaseNamingStrategy __snakeCaseNaming = new SnakeCaseNamingStrategy();

	/// <inheritdoc />
	protected CustomNamingStrategy()
		: this(NamingStrategyType.Default, NamingStrategyType.Default, NamingStrategyType.Default)
	{
	}

	/// <inheritdoc />
	protected CustomNamingStrategy(NamingStrategyType nameStrategy)
		: this(nameStrategy, nameStrategy, nameStrategy)
	{
	}

	/// <inheritdoc />
	protected CustomNamingStrategy(NamingStrategyType propertyNameStrategy, NamingStrategyType dictionaryKeyStrategy)
		: this(propertyNameStrategy, propertyNameStrategy, dictionaryKeyStrategy)
	{
	}

	/// <inheritdoc />
	protected CustomNamingStrategy(NamingStrategyType propertyNameStrategy, NamingStrategyType extensionDataNameStrategy, NamingStrategyType dictionaryKeyStrategy)
	{
		PropertyNameStrategy = propertyNameStrategy;
		ExtensionDataNameStrategy = extensionDataNameStrategy;
		DictionaryKeyStrategy = dictionaryKeyStrategy;
	}

	protected NamingStrategyType PropertyNameStrategy { get; set; }
	protected NamingStrategyType ExtensionDataNameStrategy { get; set; }
	protected NamingStrategyType DictionaryKeyStrategy { get; set; }
		
	public override string GetPropertyName(string name, bool hasSpecifiedName) { return ResolvePropertyName(name, hasSpecifiedName); }

	/// <inheritdoc />
	protected override string ResolvePropertyName(string name) { return ResolvePropertyName(name, false); }

	[NotNull]
	protected virtual string ResolvePropertyName([NotNull] string name, bool hasSpecifiedName)
	{
		switch (PropertyNameStrategy)
		{
			case NamingStrategyType.CamelCase:
				return __camelCaseNaming.GetPropertyName(name, hasSpecifiedName);
			case NamingStrategyType.SnakeCase:
				return __snakeCaseNaming.GetPropertyName(name, hasSpecifiedName);
			default:
				return __defaultNaming.GetPropertyName(name, hasSpecifiedName);
		}
	}

	/// <inheritdoc />
	public override string GetExtensionDataName(string name)
	{
		switch (ExtensionDataNameStrategy)
		{
			case NamingStrategyType.CamelCase:
				return __camelCaseNaming.GetExtensionDataName(name);
			case NamingStrategyType.SnakeCase:
				return __snakeCaseNaming.GetExtensionDataName(name);
			default:
				return __defaultNaming.GetExtensionDataName(name);
		}
	}

	/// <inheritdoc />
	public override string GetDictionaryKey(string name)
	{
		switch (DictionaryKeyStrategy)
		{
			case NamingStrategyType.CamelCase:
				return __camelCaseNaming.GetDictionaryKey(name);
			case NamingStrategyType.SnakeCase:
				return __snakeCaseNaming.GetDictionaryKey(name);
			default:
				return __defaultNaming.GetDictionaryKey(name);
		}
	}
}