using System;
using System.Runtime.Serialization;

namespace essentialMix.Caching
{
	[Serializable, DataContract]
	public class CacheObject : ICacheObject
    {
		public CacheObject()
		{
		}

		[DataMember]
		public string Validator { get; set; }

		[DataMember]
		public byte[] Item { get; set; }
    }
}