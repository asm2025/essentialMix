using System.Collections.Generic;

namespace asm.Other.Nager.PublicSuffix
{
    /// <summary>
    /// Represents a tree of TLD domains
    /// </summary>
    public class DomainDataStructure
    {
        /// <summary>
        /// The TLD Domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The type of TLD Domain. <see cref="TldRule"/>.
        /// </summary>
        public TldRule TldRule { get; set; }

        /// <summary>
        /// The children of this TLD Domain
        /// </summary>
        public Dictionary<string, DomainDataStructure> Nested { get; set; }

        /// <summary>
        /// Creates a new <see cref="DomainDataStructure"/> for <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The Domain.</param>
        public DomainDataStructure(string domain)
        {
            Domain = domain;
            Nested = new Dictionary<string, DomainDataStructure>();
        }

        /// <summary>
        /// Creates a new <see cref="DomainDataStructure"/> for <paramref name="domain"/>.
        /// </summary>
        /// <param name="domain">The Domain.</param>
        /// <param name="tldRule">The type of TLD domain.</param>
        public DomainDataStructure(string domain, TldRule tldRule)
        {
            Domain = domain;
            TldRule = tldRule;
            Nested = new Dictionary<string, DomainDataStructure>();
        }
    }
}
