using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    public class FileTldRuleProvider : ITldRuleProvider
    {
        private readonly string _fileName;

        public FileTldRuleProvider(string fileName)
        {
            _fileName = fileName;
        }

        public async Task<IEnumerable<TldRule>> BuildAsync()
        {
            string ruleData = await LoadFromFile().ConfigureAwait(false);

            TldRuleParser ruleParser = new TldRuleParser();
            IEnumerable<TldRule> rules = ruleParser.ParseRules(ruleData);
            return rules;
        }

        private async Task<string> LoadFromFile()
        {
            if (!File.Exists(_fileName))
            {
                throw new FileNotFoundException("Rule file does not exist");
            }

            using (StreamReader reader = File.OpenText(_fileName))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
