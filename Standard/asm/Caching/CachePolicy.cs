using System;

namespace asm.Caching
{
	public sealed class CachePolicy
	{
		public CachePolicy(TimeSpan expiresAfter, bool renewLeaseOnAccess = false)
		{
			ExpiresAfter = expiresAfter;
			RenewLeaseOnAccess = renewLeaseOnAccess;
		}

		public TimeSpan ExpiresAfter { get; }

		/// <summary>
		/// If specified, each read of the item from the cache will reset the expiration time
		/// </summary>
		public bool RenewLeaseOnAccess { get; }
	}
}