using System.Collections;
using System.Configuration;
using JetBrains.Annotations;

namespace asm.Configuration
{
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

		public new T this[string name] => (T)BaseGet(name);

		[NotNull] protected override ConfigurationElement CreateNewElement() { return new T(); }

		[NotNull]
		protected static ConfigurationPropertyCollection BaseProperties => new ConfigurationPropertyCollection();

		[NotNull]
		protected override ConfigurationPropertyCollection Properties => BaseProperties;
	}
}