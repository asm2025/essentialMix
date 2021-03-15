using System;

namespace essentialMix.Caching
{
    public class CacheItem
    {
        public CacheItem()
        {
            AllowSlidingTime = false;
        }

        public string Key { get; set; }
        public DateTime Expires { get; set; }
        public byte[] Value { get; set; }
        public bool AllowSlidingTime { get; set; }
    }
}