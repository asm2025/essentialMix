using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Other.Nager.PublicSuffix
{
    public class TldRule
    {
        public string Name { get; private set; }
        public TldRuleType Type { get; private set; }
        public int LabelCount { get; private set; }
        public TldRuleDivision Division { get; private set; }

        public TldRule([NotNull] string ruleData, TldRuleDivision division = TldRuleDivision.Unknown)
        {
            if (string.IsNullOrEmpty(ruleData))
            {
                throw new ArgumentException("RuleData is empty");
            }

            Division = division;

            List<string> parts = ruleData.Split('.').Select(x => x.Trim()).ToList();
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    throw new FormatException("Rule contains empty part");
                }

                if (part.Contains("*") && part != "*")
                {
                    throw new FormatException("Wildcard syntax not correct");
                }
            }

            if (ruleData.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
            {
                Type = TldRuleType.WildcardException;
                Name = ruleData.Substring(1).ToLower();
                LabelCount = parts.Count - 1; //Left-most label is removed for Wildcard Exceptions
            }
            else if (ruleData.Contains("*"))
            {
                Type = TldRuleType.Wildcard;
                Name = ruleData.ToLower();
                LabelCount = parts.Count;
            }
            else
            {
                Type = TldRuleType.Normal;
                Name = ruleData.ToLower();
                LabelCount = parts.Count;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
