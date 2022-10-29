using System;
using System.Configuration;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Windows.Helpers;

public static class ConfigurationHelper
{
	public static string GetConnectionString([NotNull] Configuration configuration, string name)
	{
		name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));

		ConnectionStringsSection section = configuration.ConnectionStrings;
		if (section == null) return null;
		if (section.SectionInformation.IsProtected) section.SectionInformation.UnprotectSection();
		return section.ConnectionStrings[name]?.ConnectionString;
	}

	public static void AddConnectionString([NotNull] Configuration configuration, string name, string connectionString, string provider)
	{
		name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
		provider = provider.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(provider));

		ConnectionStringsSection section = configuration.ConnectionStrings;

		if (section == null)
		{
			section = new ConnectionStringsSection();
			configuration.Sections.Add(section.SectionInformation.SectionName, section);
		}

		ConnectionStringSettings settings = new ConnectionStringSettings(name, connectionString, provider);
		section.ConnectionStrings.Add(settings);
		section.SectionInformation.ForceSave = true;
		configuration.Save(ConfigurationSaveMode.Modified);
	}

	public static void RemoveConnectionString([NotNull] Configuration configuration, string name)
	{
		name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));

		ConnectionStringsSection section = configuration.ConnectionStrings;
		if (section?.ConnectionStrings[name] == null) return;
		section.ConnectionStrings.Remove(name);
		section.SectionInformation.ForceSave = true;
		configuration.Save(ConfigurationSaveMode.Modified);
	}

	/// <summary>
	/// T can be DpapiProtectedConfigurationProvider or RSAProtectedConfigurationProvider for example
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="configuration"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public static bool Protect<T>([NotNull] Configuration configuration, string name)
		where T : ProtectedConfigurationProvider
	{
		name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
		ConfigurationSection section = configuration.GetSection(name);
		if (section == null || section.SectionInformation.IsProtected) return false;
		return Protect<T>(configuration, section);
	}

	/// <summary>
	/// T can be DpapiProtectedConfigurationProvider or RSAProtectedConfigurationProvider for example
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="configuration"></param>
	/// <param name="section"></param>
	/// <returns></returns>
	public static bool Protect<T>([NotNull] Configuration configuration, [NotNull] ConfigurationSection section)
		where T : ProtectedConfigurationProvider
	{
		if (section.SectionInformation.IsProtected) return false;
		section.SectionInformation.ProtectSection(typeof(T).Name);
		section.SectionInformation.ForceSave = true;
		configuration.Save(ConfigurationSaveMode.Full);
		return true;
	}

	public static bool UnProtect([NotNull] Configuration configuration, string name)
	{
		name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
		ConfigurationSection section = configuration.GetSection(name);
		if (section == null || !section.SectionInformation.IsProtected) return false;
		return UnProtect(configuration, section);
	}

	public static bool UnProtect([NotNull] Configuration configuration, [NotNull] ConfigurationSection section)
	{
		if (!section.SectionInformation.IsProtected) return false;
		section.SectionInformation.UnprotectSection();
		section.SectionInformation.ForceSave = true;
		configuration.Save(ConfigurationSaveMode.Full);
		return true;
	}
}