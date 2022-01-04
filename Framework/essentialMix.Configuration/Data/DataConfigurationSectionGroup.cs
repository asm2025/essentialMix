using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace essentialMix.Configuration.Data;

public sealed class DataConfigurationSectionGroup : ConfigurationSectionGroup
{
	public DataConfigurationSectionGroup()
	{
	}

	[ConfigurationProperty("connectionStrings")]
	public ConnectionStringsSection ConnectionStringsSection => GetSection<ConnectionStringsSection>("connectionStrings");

	public ConnectionStringSettingsCollection ConnectionStrings => ConnectionStringsSection?.ConnectionStrings;

	[ConfigurationProperty("data")]
	public DataSection DataSection => GetSection<DataSection>("data");

	public TableSettingsCollection Tables => DataSection?.Tables;

	private T GetSection<T>([NotNull] string name)
		where T : ConfigurationSection
	{
		return GetSection<T>(name, false);
	}

	[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
	private T GetSection<T>([NotNull] string name, bool required)
		where T : ConfigurationSection
	{
		T section = Sections[name] as T;
		if (required && section == null) throw new ConfigurationErrorsException($"'{name}' section's declaration is not present or invalid");
		return section;
	}

	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	private T GetSectionGroup<T>([NotNull] string name)
		where T : ConfigurationSectionGroup
	{
		return GetSectionGroup<T>(name, false);
	}

	[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
	private T GetSectionGroup<T>([NotNull] string name, bool required)
		where T : ConfigurationSectionGroup
	{
		T sectionGroup = SectionGroups[name] as T;
		if (required && sectionGroup == null) throw new ConfigurationErrorsException($"'{name}' section group's declaration is invalid");
		return sectionGroup;
	}
}