using System.Collections.Generic;
using System.Threading.Tasks;

namespace asm.Other.Nager.PublicSuffix
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
