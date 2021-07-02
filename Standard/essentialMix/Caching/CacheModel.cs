using System;
using System.Runtime.Serialization;

namespace essentialMix.Caching.ExpressionCache
{
	[Serializable, DataContract]
	public class CacheModel : ICacheModel
    {
		public CacheModel()
		{
		}

		[DataMember]
		public string CacheKey { get; set; }

		[DataMember]
		public DateTime Expires { get; set; }

		[DataMember]
		public ICacheObject CacheObject { get; set; }

		[DataMember]
		public ICacheOptions CacheOptions { get; set; }
    }
}