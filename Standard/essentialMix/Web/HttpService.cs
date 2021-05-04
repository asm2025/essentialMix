using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Web
{
	public class HttpService : Disposable
	{
		private static HttpService __instance;

		private HttpClient _client;

		public HttpService() { }

		public HttpService([NotNull] HttpClient client) { _client = client ?? throw new ArgumentNullException(nameof(client)); }

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _client);
			base.Dispose(disposing);
		}

		[NotNull]
		public HttpClient Client
		{
			get
			{
				ThrowIfDisposed();
				return _client ??= DefaultClient();
			}
		}

		public HttpResponseMessage PerformRequest(HttpRequestMessage request)
		{
			ThrowIfDisposed();
			return PerformRequestAsync(request).Execute();
		}

		public Task<HttpResponseMessage> PerformRequestAsync(HttpRequestMessage request, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			return token.IsCancellationRequested
				? null
				:  Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
		}

		public string GetString([NotNull] string url)
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			return GetStringAsync(url).Execute();
		}

		public string GetString([NotNull] Uri url)
		{
			ThrowIfDisposed();
			return GetStringAsync(url).Execute();
		}

		public Task<string> GetStringAsync([NotNull] string url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			if (token.IsCancellationRequested) return null;
			if (!UriHelper.TryBuildUri(url, out Uri uri)) throw new ArgumentException("Uri is not well formatted.", nameof(url));
			return GetStringAsync(uri, token);
		}

		public async Task<string> GetStringAsync([NotNull] Uri url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (token.IsCancellationRequested) return null;

			HttpRequestMessage request = null;
			HttpResponseMessage response = null;

			try
			{
				request = new HttpRequestMessage(HttpMethod.Get, url);
				response = await PerformRequestAsync(request, token).ConfigureAwait();
				response.EnsureSuccessStatusCode();
				return await response.Content
									.ReadAsStringAsync();
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				ObjectHelper.Dispose(ref request);
			}
		}

		public Task<Stream> GetStreamAsync([NotNull] string url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			if (token.IsCancellationRequested) return null;
			if (!UriHelper.TryBuildUri(url, out Uri uri)) throw new ArgumentException("Uri is not well formatted.", nameof(url));
			return GetStreamAsync(uri, token);
		}

		public async Task<Stream> GetStreamAsync([NotNull] Uri url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (token.IsCancellationRequested) return null;

			HttpRequestMessage request = null;
			HttpResponseMessage response = null;

			try
			{
				request = new HttpRequestMessage(HttpMethod.Get, url);
				response = await PerformRequestAsync(request, token).ConfigureAwait();
				response.EnsureSuccessStatusCode();
				return await response.Content.ReadAsStreamAsync();
			}
			catch
			{
				ObjectHelper.Dispose(ref response);
				ObjectHelper.Dispose(ref request);
				throw;
			}
		}

		public static HttpService Instance
		{
			get
			{
				if (__instance.IsDisposed()) __instance = new HttpService();
				return __instance;
			}
		}

		[NotNull]
		private static HttpClient DefaultClient()
		{
			HttpClientHandler httpClientHandler = new HttpClientHandler();
			if (httpClientHandler.SupportsAutomaticDecompression) httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			httpClientHandler.UseCookies = false;
			return new HttpClient(httpClientHandler);
		}
	}
}