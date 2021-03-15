using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    /// <summary>
    /// A TLD Domain parser
    /// </summary>
    public class DomainParser
    {
        private DomainDataStructure _domainDataStructure;
        private readonly IDomainNormalizer _domainNormalizer;
        private readonly TldRule _rootTldRule = new TldRule("*");

        /// <summary>
        /// Creates and Initializes a DomainParse.
        /// </summary>
        /// <param name="rules">The list of rules.</param>
        /// <param name="domainNormalizer">An <see cref="IDomainNormalizer"/>.</param>
        public DomainParser([NotNull] IEnumerable<TldRule> rules, IDomainNormalizer domainNormalizer = null)
            : this(domainNormalizer)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            AddRules(rules);
        }

        /// <summary>
        /// Creates and initializes a DomainParser.
        /// </summary>
        /// <param name="ruleProvider">A <see cref="TldRule"/> provider.</param>
        /// <param name="domainNormalizer">An <see cref="IDomainNormalizer"/>.</param>
        public DomainParser([NotNull] ITldRuleProvider ruleProvider, IDomainNormalizer domainNormalizer = null)
            : this(domainNormalizer)
        {
            IEnumerable<TldRule> rules = ruleProvider.BuildAsync().GetAwaiter().GetResult();
            AddRules(rules);
        }

        /// <summary>
        /// Creates a DomainParser based on an already initialized tree.
        /// </summary>
        /// <param name="initializedDataStructure">An already initialized tree.</param>
        /// <param name="domainNormalizer">An <see cref="IDomainNormalizer"/>.</param>
        public DomainParser(DomainDataStructure initializedDataStructure, IDomainNormalizer domainNormalizer = null)
            : this(domainNormalizer)
        {
            _domainDataStructure = initializedDataStructure;
        }

        private DomainParser(IDomainNormalizer domainNormalizer)
        {
            _domainNormalizer = domainNormalizer ?? new UriNormalizer();
        }

        /// <summary>
        /// Tries to get a Domain from <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The domain to parse.</param>
        /// <returns><strong>null</strong> if <paramref name="domain"/> it's invalid.</returns>
        [NotNull]
		public DomainName Get([NotNull] Uri domain)
        {
            string partlyNormalizedDomain = domain.Host;
            string normalizedHost = domain.GetComponents(UriComponents.NormalizedHost, UriFormat.UriEscaped); //Normalize punycode

            List<string> parts = normalizedHost
								.Split('.')
								.Reverse()
								.ToList();

            return GetDomainFromParts(partlyNormalizedDomain, parts);
        }

        /// <summary>
        /// Tries to get a Domain from <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The domain to parse.</param>
        /// <returns><strong>null</strong> if <paramref name="domain"/> it's invalid.</returns>
        [NotNull]
		public DomainName Get(string domain)
        {
            List<string> parts = _domainNormalizer.PartlyNormalizeDomainAndExtractFullyNormalizedParts(domain, out string partlyNormalizedDomain);
            return GetDomainFromParts(partlyNormalizedDomain, parts);
        }

        /// <summary>
        /// Return whether <paramref name="domain"/> is valid or not.
        /// </summary>
        /// <param name="domain">The domain to check.</param>
        /// <returns><strong>true</strong> if <paramref name="domain"/> it's valid.</returns>
        public bool IsValidDomain(string domain)
        {
            List<string> parts = _domainNormalizer.PartlyNormalizeDomainAndExtractFullyNormalizedParts(domain, out string partlyNormalizedDomain);
            DomainName domainName = GetDomainFromParts(partlyNormalizedDomain, parts);
			return !domainName.TLDRule.Equals(_rootTldRule);
        }

        private void AddRules([NotNull] IEnumerable<TldRule> tldRules)
        {
            _domainDataStructure ??= new DomainDataStructure("*", _rootTldRule);
            _domainDataStructure.AddRules(tldRules);
        }

        [NotNull]
		private DomainName GetDomainFromParts(string domain, [NotNull] List<string> parts)
        {
            if (parts == null || parts.Count == 0 || parts.Any(x => x.Equals(string.Empty)))
            {
                throw new ParseException("Invalid domain part detected");
            }

            DomainDataStructure structure = _domainDataStructure;
            List<TldRule> matches = new List<TldRule>();
            FindMatches(parts, structure, matches);

            //Sort so exceptions are first, then by biggest label count (with wildcards at bottom) 
            IOrderedEnumerable<TldRule> sortedMatches = matches.OrderByDescending(x => x.Type == TldRuleType.WildcardException ? 1 : 0)
																.ThenByDescending(x => x.LabelCount)
																.ThenByDescending(x => x.Name);

            TldRule winningRule = sortedMatches.FirstOrDefault();

            //Domain is TLD
            if (winningRule != null && parts.Count == winningRule.LabelCount)
            {
                parts.Reverse();
                string tld = string.Join(".", parts);

                if (winningRule.Type == TldRuleType.Wildcard)
                {
                    if (tld.EndsWith(winningRule.Name.Substring(1)))
                    {
                        throw new ParseException("Domain is a TLD according public suffix", winningRule);
                    }
                }
                else
                {
                    if (tld.Equals(winningRule.Name))
                    {
                        throw new ParseException("Domain is a TLD according public suffix", winningRule);
                    }
                }

                throw new ParseException($"Unknown domain {domain}");
            }

            return new DomainName(domain, winningRule);
        }

        private static void FindMatches([NotNull] IEnumerable<string> parts, [NotNull] DomainDataStructure structure, List<TldRule> matches)
        {
            if (structure.TldRule != null)
            {
                matches.Add(structure.TldRule);
            }

            string part = parts.FirstOrDefault();
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            if (structure.Nested.TryGetValue(part, out DomainDataStructure foundStructure))
            {
                FindMatches(parts.Skip(1), foundStructure, matches);
            }

            if (structure.Nested.TryGetValue("*", out foundStructure))
            {
                FindMatches(parts.Skip(1), foundStructure, matches);
            }
        }
    }
}
