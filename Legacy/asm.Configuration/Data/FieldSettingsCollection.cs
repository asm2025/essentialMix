using System.Configuration;

namespace asm.Configuration.Data
{
	[ConfigurationCollection(typeof(FieldElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "field")]
	public class FieldSettingsCollection : ConfigurationElementCollection<FieldElement>
	{
		public FieldSettingsCollection()
		{
		}

		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

		protected override object GetElementKey(ConfigurationElement element) { return ((FieldElement)element).Name; }

		protected override string ElementName => "field";
	}
}