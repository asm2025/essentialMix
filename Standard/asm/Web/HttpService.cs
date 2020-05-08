using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Web
{
	public class HttpService : Disposable, IDisposable
	{
		private static HttpService _instance;

		private HttpClient _client;

		public HttpService() { }

		public HttpService([NotNull] HttpClient client) { _client = client ?? throw new ArgumentNullException(nameof(client)); }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref _client);
			}
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
			return TaskHelper.Run(() => PerformRequestAsync(request));
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
			return TaskHelper.Run(() => GetStringAsync(url));
		}

		public string GetString([NotNull] Uri url)
		{
			ThrowIfDisposed();
			return TaskHelper.Run(() => GetStringAsync(url));
		}

		public async Task<string> GetStringAsync([NotNull] string url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			if (token.IsCancellationRequested) return null;
			if (!UriHelper.ToUri(url, out Uri uri)) throw new ArgumentException("Uri is not well formatted.", nameof(url));
			return await GetStringAsync(uri, token).ConfigureAwait();
		}

		public async Task<string> GetStringAsync([NotNull] Uri url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (token.IsCancellationRequested) return null;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
			{
				using (HttpResponseMessage response = await PerformRequestAsync(request, token).ConfigureAwait())
				{
					response.EnsureSuccessStatusCode();
					return await response.Content
										.ReadAsStringAsync().ConfigureAwait();
				}
			}
		}

		public async Task<Stream> GetStreamAsync([NotNull] string url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			if (token.IsCancellationRequested) return null;
			if (!UriHelper.ToUri(url, out Uri uri)) throw new ArgumentException("Uri is not well formatted.", nameof(url));
			return await GetStreamAsync(uri, token).ConfigureAwait();
		}

		public async Task<Stream> GetStreamAsync([NotNull] Uri url, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (token.IsCancellationRequested) return null;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
			{
				HttpResponseMessage response = await PerformRequestAsync(request, token).ConfigureAwait();
				response.EnsureSuccessStatusCode();
				return await response.Content.ReadAsStreamAsync().ConfigureAwait();
			}
		}

		public static HttpService Instance
		{
			get
			{
				if (_instance.IsDisposed()) _instance = new HttpService();
				return _instance;
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