using System.Net;
using System.Net.Http;
using JetBrains.Annotations;
using essentialMix.IO;

namespace essentialMix.Extensions
{
	public static class HttpClientHandlerExtension
	{
		[NotNull]
		public static T Configure<T>([NotNull] this T thisValue, IOHttpRequestSettings settings = null)
			where T : HttpClientHandler
		{
			settings ??= new IOHttpRequestSettings();
			thisValue.AllowAutoRedirect = settings.AllowAutoRedirect;
			thisValue.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			thisValue.UseDefaultCredentials = settings.Credentials == null;
			thisValue.Credentials = settings.Credentials ?? CredentialCache.DefaultNetworkCredentials;
			thisValue.ClientCertificateOptions = ClientCertificateOption.Automatic;
			thisValue.Proxy = settings.Proxy;
			thisValue.UseProxy = settings.Proxy != null;
			return thisValue;
		}
	}
}