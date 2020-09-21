using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    public class TldRuleParser
    {
        [NotNull]
		public IEnumerable<TldRule> ParseRules([NotNull] string data)
        {
            string[] lines = data.Split('\n', '\r');
            return ParseRules(lines);
        }

        [NotNull]
		public IEnumerable<TldRule> ParseRules([NotNull] IEnumerable<string> lines)
        {
            List<TldRule> items = new List<TldRule>();
            TldRuleDivision division = TldRuleDivision.Unknown;

            foreach (string line in lines)
            {
                //Ignore empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                //Ignore comments (and set Division)
                if (line.StartsWith("//"))
                {
                    //Detect Division
                    if (line.StartsWith("// ===BEGIN ICANN DOMAINS==="))
                    {
                        division = TldRuleDivision.ICANN;
                    }
                    else if (line.StartsWith("// ===END ICANN DOMAINS==="))
                    {
                        division = TldRuleDivision.Unknown;
                    }
                    else if (line.StartsWith("// ===BEGIN PRIVATE DOMAINS==="))
                    {
                        division = TldRuleDivision.Private;
                    }
                    else if (line.StartsWith("// ===END PRIVATE DOMAINS==="))
                    {
                        division = TldRuleDivision.Unknown;
                    }

                    continue;
                }

                TldRule tldRule = new TldRule(line.Trim(), division);
                items.Add(tldRule);
            }

            return items;
        }
    }
}
