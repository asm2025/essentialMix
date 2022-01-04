using System.Collections;
using System.Configuration;
using JetBrains.Annotations;

namespace essentialMix.Configuration;

public abstract class ConfigurationElementCollection<T> : ConfigurationElementCollection
	where T : ConfigurationElement, new()
{
	protected ConfigurationElementCollection()
	{
	}

	protected ConfigurationElementCollection([NotNull] IComparer comparer) 
		: base(comparer)
	{
	}

	public T this[int index]
	{
		get => (T)BaseGet(index);
		set
		{
			if (BaseGet(index) != null) BaseRemoveAt(index);
			BaseAdd(index, value);
		}
	}

	public new T this[[NotNull] string name] => (T)BaseGet(name);

	protected override ConfigurationElement CreateNewElement() { return new T(); }

	[NotNull]
	protected static ConfigurationPropertyCollection BaseProperties => new ConfigurationPropertyCollection();

	[NotNull]
	protected override ConfigurationPropertyCollection Properties => BaseProperties;
}