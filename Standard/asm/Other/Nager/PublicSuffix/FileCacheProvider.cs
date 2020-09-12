using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Other.Nager.PublicSuffix
{
    public class FileCacheProvider : ICacheProvider
    {
        private readonly string _cacheFilePath;
        private readonly TimeSpan _timeToLive;

        public FileCacheProvider([NotNull] string cacheFileName = "publicsuffixcache.dat", TimeSpan? cacheTimeToLive = null)
        {
            _timeToLive = cacheTimeToLive ?? TimeSpan.FromDays(1);
            string tempPath = Path.GetTempPath();
            _cacheFilePath = Path.Combine(tempPath, cacheFileName);
        }

        public bool IsCacheValid()
        {
            bool cacheInvalid = true;

            FileInfo fileInfo = new FileInfo(_cacheFilePath);
            if (fileInfo.Exists)
            {
                if (fileInfo.LastWriteTimeUtc > DateTime.UtcNow.Subtract(_timeToLive))
                {
                    cacheInvalid = false;
                }
            }

            return !cacheInvalid;
        }

        public async Task<string> GetAsync()
        {
            if (!IsCacheValid())
            {
                return null;
            }

            using (StreamReader reader = File.OpenText(_cacheFilePath))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        public async Task SetAsync(string data)
        {
            using (StreamWriter streamWriter = File.CreateText(_cacheFilePath))
            {
                await streamWriter.WriteAsync(data).ConfigureAwait(false);
            }
        }
    }
}
