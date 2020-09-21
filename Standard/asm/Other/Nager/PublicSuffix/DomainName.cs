using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    public class DomainName
    {
        public string Domain { get; }
        public string TLD { get; }
        public string SubDomain { get; }
        public string RegistrableDomain { get; }
        public string Hostname { get; }
        public TldRule TLDRule { get; }

        public DomainName()
        {
        }

        public DomainName(string domain, TldRule tldRule)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return;
            }

            if (tldRule == null)
            {
                return;
            }

            List<string> domainParts = domain.Split('.').Reverse().ToList();
            List<string> ruleParts = tldRule.Name.Split('.').Skip(tldRule.Type == TldRuleType.WildcardException ? 1 : 0).Reverse().ToList();
            string tld = string.Join(".", domainParts.Take(ruleParts.Count).Reverse());
            string registrableDomain = string.Join(".", domainParts.Take(ruleParts.Count + 1).Reverse());

            if (domain.Equals(tld))
            {
                return;
            }

            TLDRule = tldRule;
            Hostname = domain;
            TLD = tld;
            RegistrableDomain = registrableDomain;

            Domain = domainParts.Skip(ruleParts.Count).FirstOrDefault();
            string subDomain = string.Join(".", domainParts.Skip(ruleParts.Count + 1).Reverse());
            SubDomain = string.IsNullOrEmpty(subDomain) ? null : subDomain;
        }
    }
}
