using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    public interface ITldRuleProvider
    {
        /// <summary>
        /// Builds the list of TldRules
        /// </summary>
        /// <returns>List of TldRules</returns>
        Task<IEnumerable<TldRule>> BuildAsync();
    }
}
