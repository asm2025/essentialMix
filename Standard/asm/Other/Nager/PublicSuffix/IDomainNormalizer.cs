using System.Collections.Generic;

namespace asm.Other.Nager.PublicSuffix
{
    public interface IDomainNormalizer
    {
        List<string> PartlyNormalizeDomainAndExtractFullyNormalizedParts(string domain, out string partlyNormalizedDomain);
    }
}
