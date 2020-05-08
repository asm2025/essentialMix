using System.Configuration;
using asm.Configuration;
using JetBrains.Annotations;

namespace asm.Data.Configuration
{
	[ConfigurationCollection(typeof(FieldElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "field")]
	public class FieldSettingsCollection : ConfigurationElementCollection<FieldElement>
	{
		public FieldSettingsCollection()
		{
		}

		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

		protected override object GetElementKey(ConfigurationElement element) { return ((FieldElement)element).Name; }

		[NotNull]
		protected override string ElementName => "field";
	}
}