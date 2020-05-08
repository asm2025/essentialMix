using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Events;
using asm.Extensions;
using asm.Helpers;
using asm.IO;
using asm.Web.Handlers;

namespace asm.Web
{
	public class HttpClientEx : HttpClient
	{
		/// <inheritdoc />
		protected HttpClientEx()
			: this(new HttpClientHandler(), true, null, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx([NotNull] string baseUrl)
			: this(new HttpClientHandler(), true, UriHelper.ToUri(baseUrl), null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx([NotNull] string baseUrl, IOHttpRequestSettings settings)
			: this(new HttpClientHandler(), true, UriHelper.ToUri(baseUrl), settings)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(Uri baseUrl)
			: this(new HttpClientHandler(), true, baseUrl, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(Uri baseUrl, IOHttpRequestSettings settings)
			: this(new HttpClientHandler(), true, baseUrl, settings)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(HttpMessageHandler handler)
			: this(handler, true, null, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(HttpMessageHandler handler, Uri baseUrl)
			: this(handler, true, baseUrl, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(HttpMessageHandler handler, bool disposeHandler)
			: this(handler, disposeHandler, null, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(HttpMessageHandler handler, bool disposeHandler, Uri baseUrl)
			: this(handler, disposeHandler, baseUrl, null)
		{
		}

		/// <inheritdoc />
		protected HttpClientEx(HttpMessageHandler handler, bool disposeHandler, Uri baseUrl, IOHttpRequestSettings settings)
			: base(handler, disposeHandler)
		{
			this.Configure(baseUrl, settings);
		}


		private HttpClientEx([NotNull] HttpClient client)
			: this(client.GetHandler(), false, client.BaseAddress, client.GetRequestSettings())
		{
		}

		public event EventHandler<HttpResponseMessageEventArgs> Response;
		public event EventHandler<HttpRequestExceptionEventArgs> Error;

		public Func<HttpResponseMessage, bool> Filter { get; set; }


		public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
				return OnResponse(response)
							? response
							: null;
			}
			catch (Exception ex)
			{
				if (!OnError(request, ex)) throw;
				return null;
			}
		}

		public HttpResponseMessage Delete(string requestUri) { return TaskHelper.Run(() => DeleteAsync(requestUri)); }
		public HttpResponseMessage Delete(string requestUri, CancellationToken cancellationToken) { return TaskHelper.Run(() => DeleteAsync(requestUri, cancellationToken)); }
		public HttpResponseMessage Delete(Uri requestUri) { return TaskHelper.Run(() => DeleteAsync(requestUri)); }
		public HttpResponseMessage Delete(Uri requestUri, CancellationToken cancellationToken) { return TaskHelper.Run(() => DeleteAsync(requestUri, cancellationToken)); }
		public HttpResponseMessage Get(string requestUri) { return TaskHelper.Run(() => GetAsync(requestUri)); }
		public HttpResponseMessage Get(string requestUri, HttpCompletionOption completionOption) { return TaskHelper.Run(() => GetAsync(requestUri, completionOption)); }
		public HttpResponseMessage Get(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return TaskHelper.Run(() => GetAsync(requestUri, completionOption, cancellationToken)); }
		public HttpResponseMessage Get(string requestUri, CancellationToken cancellationToken) { return TaskHelper.Run(() => GetAsync(requestUri, cancellationToken)); }
		public HttpResponseMessage Get(Uri requestUri) { return TaskHelper.Run(() => GetAsync(requestUri)); }
		public HttpResponseMessage Get(Uri requestUri, HttpCompletionOption completionOption) { return TaskHelper.Run(() => GetAsync(requestUri, completionOption)); }
		public HttpResponseMessage Get(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return TaskHelper.Run(() => GetAsync(requestUri, completionOption, cancellationToken)); }
		public HttpResponseMessage Get(Uri requestUri, CancellationToken cancellationToken) { return TaskHelper.Run(() => GetAsync(requestUri, cancellationToken)); }
		public byte[] GetByteArray(string requestUri) { return TaskHelper.Run(() => GetByteArrayAsync(requestUri)); }
		public byte[] GetByteArray(Uri requestUri) { return TaskHelper.Run(() => GetByteArrayAsync(requestUri)); }
		public Stream GetStream(string requestUri) { return TaskHelper.Run(() => GetStreamAsync(requestUri)); }
		public Stream GetStream(Uri requestUri) { return TaskHelper.Run(() => GetStreamAsync(requestUri)); }
		public string GetString(string requestUri) { return TaskHelper.Run(() => GetStringAsync(requestUri)); }
		public string GetString(Uri requestUri) { return TaskHelper.Run(() => GetStringAsync(requestUri)); }
		public HttpResponseMessage Post(string requestUri, HttpContent content) { return TaskHelper.Run(() => PostAsync(requestUri, content)); }
		public HttpResponseMessage Post(string requestUri, HttpContent content, CancellationToken cancellationToken) { return TaskHelper.Run(() => PostAsync(requestUri, content, cancellationToken)); }
		public HttpResponseMessage Post(Uri requestUri, HttpContent content) { return TaskHelper.Run(() => PostAsync(requestUri, content)); }
		public HttpResponseMessage Post(Uri requestUri, HttpContent content, CancellationToken cancellationToken) { return TaskHelper.Run(() => PostAsync(requestUri, content, cancellationToken)); }
		public HttpResponseMessage Put(string requestUri, HttpContent content) { return TaskHelper.Run(() => PutAsync(requestUri, content)); }
		public HttpResponseMessage Put(string requestUri, HttpContent content, CancellationToken cancellationToken) { return TaskHelper.Run(() => PutAsync(requestUri, content, cancellationToken)); }
		public HttpResponseMessage Put(Uri requestUri, HttpContent content) { return TaskHelper.Run(() => PutAsync(requestUri, content)); }
		public HttpResponseMessage Put(Uri requestUri, HttpContent content, CancellationToken cancellationToken) { return TaskHelper.Run(() => PutAsync(requestUri, content, cancellationToken)); }
		public HttpResponseMessage Send(HttpRequestMessage request) { return TaskHelper.Run(() => SendAsync(request)); }
		public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) { return TaskHelper.Run(() => SendAsync(request, completionOption)); }
		public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return TaskHelper.Run(() => SendAsync(request, completionOption, cancellationToken)); }
		public HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) { return TaskHelper.Run(() => SendAsync(request, cancellationToken)); }

		protected virtual bool OnResponse([NotNull] HttpResponseMessage response)
		{
			if (!(Filter?.Invoke(response) ?? true)) return false;
			if (Response == null) return true;
			HttpResponseMessageEventArgs arg = new HttpResponseMessageEventArgs(response);
			Response?.Invoke(this, arg);
			return !arg.Cancel;
		}

		protected virtual bool OnError([NotNull] HttpRequestMessage request, [NotNull] Exception exception)
		{
			if (Error == null) return false;
			HttpRequestExceptionEventArgs arg = new HttpRequestExceptionEventArgs(request, exception);
			Error?.Invoke(this, arg);
			return arg.Handled;
		}

		[NotNull] public static HttpClientEx FromHttpClient([NotNull] HttpClient client) { return new HttpClientEx(client); }

		[NotNull] public static HttpClientEx Create() { return Create((Uri)null, null); }
		[NotNull] public static HttpClientEx Create(IOHttpRequestSettings settings) { return Create((Uri)null, settings); }
		public static HttpClientEx Create([NotNull] string baseUri) { return Create(baseUri, null); }
		[NotNull] public static HttpClientEx Create([NotNull] string baseUri, IOHttpRequestSettings settings) { return Create(UriHelper.ToUri(baseUri), settings); }
		[NotNull]
		public static HttpClientEx Create(Uri baseUri, IOHttpRequestSettings settings)
		{
			if (settings == null) settings = new IOHttpRequestSettings();

			HttpClientEx client = new HttpClientEx(new ChunkedTransferEncodingHandler(new HttpClientHandler().Configure(settings)));
			client.Configure(baseUri, settings);
			return client;
		}
	}
}