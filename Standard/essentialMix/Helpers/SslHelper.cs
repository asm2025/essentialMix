using System.Net;

namespace essentialMix.Helpers
{
	public static class SSLHelper
	{
		public static void NukeSsl(bool debugOnly = true)
		{
			if (debugOnly && !DebugHelper.DebugMode) return;
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
		}
	}
}