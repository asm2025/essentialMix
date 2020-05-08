using System.Configuration;

namespace asm.Data.Configuration
{
	[ConfigurationCollection(typeof(RelatedTableElement))]
	public class RelatedTableSettingsCollection : TableSettingsCollection<RelatedTableElement>
	{
		public RelatedTableSettingsCollection()
		{
		}
	}
}