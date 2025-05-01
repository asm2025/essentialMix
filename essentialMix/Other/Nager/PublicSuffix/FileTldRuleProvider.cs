using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix;

public class FileTldRuleProvider(string fileName) : ITldRuleProvider
{
	public async Task<IEnumerable<TldRule>> BuildAsync()
	{
		string ruleData = await LoadFromFile().ConfigureAwait(false);

		TldRuleParser ruleParser = new TldRuleParser();
		IEnumerable<TldRule> rules = ruleParser.ParseRules(ruleData);
		return rules;
	}

	private async Task<string> LoadFromFile()
	{
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException("Rule file does not exist");
		}

		using (StreamReader reader = File.OpenText(fileName))
		{
			return await reader.ReadToEndAsync().ConfigureAwait(false);
		}
	}
}