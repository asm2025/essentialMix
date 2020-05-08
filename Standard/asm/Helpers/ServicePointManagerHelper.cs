using System.Net;
using asm.Web;

namespace asm.Helpers
{
	public static class ServicePointManagerHelper
	{
		public static void Setup(ServicePointManagerOptions options)
		{
			if (options.CertificateValidation.HasValue)
			{
				ServicePointManager.ServerCertificateValidationCallback = delegate { return options.CertificateValidation == false; };
			}

			if (options.CertificateRevocationList.HasValue)
			{
				ServicePointManager.CheckCertificateRevocationList = options.CertificateRevocationList.Value;
			}

			if (options.DefaultConnectionLimit.HasValue)
			{
				ServicePointManager.DefaultConnectionLimit = options.DefaultConnectionLimit.Value;
			}

			if (options.DnsRefreshTimeout.HasValue)
			{
				ServicePointManager.DnsRefreshTimeout = options.DnsRefreshTimeout.Value;
			}

			if (options.DnsRoundRobin.HasValue)
			{
				ServicePointManager.EnableDnsRoundRobin = options.DnsRoundRobin.Value;
			}

			if (options.Expect100Continue.HasValue)
			{
				ServicePointManager.Expect100Continue = options.Expect100Continue.Value;
			}

			if (options.ServicePointIdleTime.HasValue)
			{
				ServicePointManager.MaxServicePointIdleTime = options.ServicePointIdleTime.Value;
			}

			if (options.MaxServicePoints.HasValue)
			{
				ServicePointManager.MaxServicePoints = options.MaxServicePoints.Value;
			}

			if (options.ReusePort.HasValue)
			{
				ServicePointManager.ReusePort = options.ReusePort.Value;
			}
		}
	}
}