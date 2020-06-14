using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Other.Nager.PublicSuffix
{
    public class WebTldRuleProvider : ITldRuleProvider
    {
        private readonly string _fileUrl;
        private readonly ICacheProvider _cacheProvider;

        public ICacheProvider CacheProvider { get { return _cacheProvider; } }

        public WebTldRuleProvider(string url = "https://publicsuffix.org/list/public_suffix_list.dat", ICacheProvider cacheProvider = null)
        {
            _fileUrl = url;

            if (cacheProvider == null)
            {
                _cacheProvider = new FileCacheProvider();
                return;
            }

            _cacheProvider = cacheProvider;
        }

        [ItemNotNull]
		public async Task<IEnumerable<TldRule>> BuildAsync()
        {
            TldRuleParser ruleParser = new TldRuleParser();

            string ruleData;
            if (!_cacheProvider.IsCacheValid())
            {
                ruleData = await LoadFromUrl(_fileUrl).ConfigureAwait(false);
                await _cacheProvider.SetAsync(ruleData).ConfigureAwait(false);
            }
            else
            {
                ruleData = await _cacheProvider.GetAsync().ConfigureAwait(false);
            }

            IEnumerable<TldRule> rules = ruleParser.ParseRules(ruleData);
            return rules;
        }

        public async Task<string> LoadFromUrl(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            using (HttpResponseMessage response = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new RuleLoadException($"Cannot load from {url} {response.StatusCode}");
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}
