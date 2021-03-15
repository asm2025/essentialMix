using System;
using System.Runtime.Serialization;

namespace essentialMix.Caching
{
	[Serializable, DataContract]
	public class CacheOptions : ICacheOptions
	{
		public const int EXPIRATION_TIME_DEFAULT = 900000;
		public static readonly TimeSpan ExpirationTimeDefault = TimeSpan.FromMilliseconds(EXPIRATION_TIME_DEFAULT);

		public static readonly ICacheOptions Default = new CacheOptions();

		public CacheOptions()
		{
			AllowSlidingTime = false;
			ExpirationInMilliSeconds = EXPIRATION_TIME_DEFAULT;
			SkipProvider = Array.Empty<string>();
		}

		[DataMember]
		public bool AllowSlidingTime { get; set; }

		[DataMember]
		public long ExpirationInMilliSeconds { get; set; }

		[DataMember]
		public string[] SkipProvider { get; set; }
	}
}