using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using essentialMix.Events;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.IO;
using essentialMix.Web.Handlers;

namespace essentialMix.Web;

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

	public HttpResponseMessage Delete(string requestUri) { return DeleteAsync(requestUri).Execute(); }
	public HttpResponseMessage Delete(string requestUri, CancellationToken cancellationToken) { return DeleteAsync(requestUri, cancellationToken).Execute(); }
	public HttpResponseMessage Delete(Uri requestUri) { return DeleteAsync(requestUri).Execute(); }
	public HttpResponseMessage Delete(Uri requestUri, CancellationToken cancellationToken) { return DeleteAsync(requestUri, cancellationToken).Execute(); }
	public HttpResponseMessage Get(string requestUri) { return GetAsync(requestUri).Execute(); }
	public HttpResponseMessage Get(string requestUri, HttpCompletionOption completionOption) { return GetAsync(requestUri, completionOption).Execute(); }
	public HttpResponseMessage Get(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return GetAsync(requestUri, completionOption, cancellationToken).Execute(); }
	public HttpResponseMessage Get(string requestUri, CancellationToken cancellationToken) { return GetAsync(requestUri, cancellationToken).Execute(); }
	public HttpResponseMessage Get(Uri requestUri) { return GetAsync(requestUri).Execute(); }
	public HttpResponseMessage Get(Uri requestUri, HttpCompletionOption completionOption) { return GetAsync(requestUri, completionOption).Execute(); }
	public HttpResponseMessage Get(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return GetAsync(requestUri, completionOption, cancellationToken).Execute(); }
	public HttpResponseMessage Get(Uri requestUri, CancellationToken cancellationToken) { return GetAsync(requestUri, cancellationToken).Execute(); }
	public byte[] GetByteArray(string requestUri) { return GetByteArrayAsync(requestUri).Execute(); }
	public byte[] GetByteArray(Uri requestUri) { return GetByteArrayAsync(requestUri).Execute(); }
	public Stream GetStream(string requestUri) { return GetStreamAsync(requestUri).Execute(); }
	public Stream GetStream(Uri requestUri) { return GetStreamAsync(requestUri).Execute(); }
	public string GetString(string requestUri) { return GetStringAsync(requestUri).Execute(); }
	public string GetString(Uri requestUri) { return GetStringAsync(requestUri).Execute(); }
	public HttpResponseMessage Post(string requestUri, HttpContent content) { return PostAsync(requestUri, content).Execute(); }
	public HttpResponseMessage Post(string requestUri, HttpContent content, CancellationToken cancellationToken) { return PostAsync(requestUri, content, cancellationToken).Execute(); }
	public HttpResponseMessage Post(Uri requestUri, HttpContent content) { return PostAsync(requestUri, content).Execute(); }
	public HttpResponseMessage Post(Uri requestUri, HttpContent content, CancellationToken cancellationToken) { return PostAsync(requestUri, content, cancellationToken).Execute(); }
	public HttpResponseMessage Put(string requestUri, HttpContent content) { return PutAsync(requestUri, content).Execute(); }
	public HttpResponseMessage Put(string requestUri, HttpContent content, CancellationToken cancellationToken) { return PutAsync(requestUri, content, cancellationToken).Execute(); }
	public HttpResponseMessage Put(Uri requestUri, HttpContent content) { return PutAsync(requestUri, content).Execute(); }
	public HttpResponseMessage Put(Uri requestUri, HttpContent content, CancellationToken cancellationToken) { return PutAsync(requestUri, content, cancellationToken).Execute(); }
#if NET10_0_OR_GREATER
	public new HttpResponseMessage Send(HttpRequestMessage request) { return SendAsync(request).Execute(); }
	public new HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) { return SendAsync(request, completionOption).Execute(); }
	public new HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return SendAsync(request, completionOption, cancellationToken).Execute(); }
	public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) { return SendAsync(request, cancellationToken).Execute(); }
#else
	public HttpResponseMessage Send(HttpRequestMessage request) { return SendAsync(request).Execute(); }
	public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) { return SendAsync(request, completionOption).Execute(); }
	public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) { return SendAsync(request, completionOption, cancellationToken).Execute(); }
	public HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) { return SendAsync(request, cancellationToken).Execute(); }
#endif

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

	[NotNull]
	public static HttpClientEx FromHttpClient([NotNull] HttpClient client) { return new HttpClientEx(client); }

	[NotNull]
	public static HttpClientEx Create() { return Create((Uri)null, null); }
	[NotNull]
	public static HttpClientEx Create(IOHttpRequestSettings settings) { return Create((Uri)null, settings); }
	[NotNull]
	public static HttpClientEx Create([NotNull] string baseUri) { return Create(baseUri, null); }
	[NotNull]
	public static HttpClientEx Create([NotNull] string baseUri, IOHttpRequestSettings settings) { return Create(UriHelper.ToUri(baseUri), settings); }
	[NotNull]
	public static HttpClientEx Create(Uri baseUri, IOHttpRequestSettings settings)
	{
		settings ??= new IOHttpRequestSettings();
		HttpClientEx client = new HttpClientEx(new ChunkedTransferEncodingHandler(new HttpClientHandler().Configure(settings)));
		client.Configure(baseUri, settings);
		return client;
	}
}