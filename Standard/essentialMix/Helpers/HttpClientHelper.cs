using System;
using System.Net.Http;
using essentialMix.Extensions;
using essentialMix.IO;
using essentialMix.Web.Handlers;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public class HttpClientHelper
	{
		[NotNull]
		public static HttpClient Create() { return Create((Uri)null, null); }
		[NotNull]
		public static HttpClient Create(IOHttpRequestSettings settings) { return Create((Uri)null, settings); }
		[NotNull]
		public static HttpClient Create([NotNull] string baseUri)  { return Create(baseUri, null); }
		[NotNull]
		public static HttpClient Create([NotNull] string baseUri, IOHttpRequestSettings settings) { return Create(UriHelper.ToUri(baseUri), settings); }
		[NotNull]
		public static HttpClient Create(Uri baseUri, IOHttpRequestSettings settings)
		{
			settings ??= new IOHttpRequestSettings();
			
			HttpClient client = new HttpClient(new ChunkedTransferEncodingHandler(new HttpClientHandler().Configure(settings)));
			client.Configure(baseUri, settings);
			return client;
		}
	}
}