using System.Collections;
using System.Configuration;
using JetBrains.Annotations;

namespace essentialMix.Configuration.Data
{
	[ConfigurationCollection(typeof(TableElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "table")]
	public class TableSettingsCollection<T> : ConfigurationElementCollection<T>
		where T : TableElement, new()
	{
		public TableSettingsCollection()
		{
		}

		public TableSettingsCollection([NotNull] IComparer comparer) 
			: base(comparer)
		{
		}

		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

		protected override object GetElementKey(ConfigurationElement element) { return ((T)element).Name; }

		protected override string ElementName => "table";
	}

	public class TableSettingsCollection : TableSettingsCollection<TableElement>
	{
		public TableSettingsCollection()
		{
		}
	}
}