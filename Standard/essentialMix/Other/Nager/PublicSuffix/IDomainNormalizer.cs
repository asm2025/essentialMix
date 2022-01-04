using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix;

public interface IDomainNormalizer
{
	List<string> PartlyNormalizeDomainAndExtractFullyNormalizedParts(string domain, out string partlyNormalizedDomain);
}