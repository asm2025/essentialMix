using System.Threading.Tasks;

namespace asm.Other.Nager.PublicSuffix
{
    public interface ICacheProvider
    {
        Task<string> GetAsync();
        Task SetAsync(string data);
        bool IsCacheValid();
    }
}
