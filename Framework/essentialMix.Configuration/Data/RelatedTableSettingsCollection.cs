using System.Configuration;

namespace essentialMix.Configuration.Data;

[ConfigurationCollection(typeof(RelatedTableElement))]
public class RelatedTableSettingsCollection : TableSettingsCollection<RelatedTableElement>
{
	public RelatedTableSettingsCollection()
	{
	}
}