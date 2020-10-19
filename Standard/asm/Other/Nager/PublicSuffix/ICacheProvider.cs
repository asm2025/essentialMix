using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
namespace Other.Nager.PublicSuffix
{
    public interface ICacheProvider
    {
        Task<string> GetAsync();
        Task SetAsync(string data);
        bool IsCacheValid();
    }
}
