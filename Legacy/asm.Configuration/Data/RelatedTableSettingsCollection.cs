using System.Configuration;

namespace asm.Configuration.Data
{
	[ConfigurationCollection(typeof(RelatedTableElement))]
	public class RelatedTableSettingsCollection : TableSettingsCollection<RelatedTableElement>
	{
		public RelatedTableSettingsCollection()
		{
		}
	}
}