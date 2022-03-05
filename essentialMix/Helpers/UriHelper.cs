using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.IO;
using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class UriHelper
{
	public const int MAX_POST_SIZE = 3143640;
	public const int MAX_POST_SIZE_LARGE = int.MaxValue;

	private static readonly string __uriSchemeHttp = $"{Uri.UriSchemeHttp}{Uri.SchemeDelimiter}";
	private static readonly string __uriSchemeHttps = $"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}";

	private static readonly ConcurrentDictionary<string, HttpMethod> __httpMethodsCache = new ConcurrentDictionary<string, HttpMethod>(StringComparer.OrdinalIgnoreCase);

	public static ISet<string> Schemes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		Uri.UriSchemeHttp,
		Uri.UriSchemeHttps,
		Uri.UriSchemeMailto,
		Uri.UriSchemeFtp,
		Uri.UriSchemeGopher,
		Uri.UriSchemeNetPipe,
		Uri.UriSchemeNetTcp,
		Uri.UriSchemeNews,
		Uri.UriSchemeNntp,
		Uri.UriSchemeFile
	};

	public static SortedList<string, string> UserAgents { get; } = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase)
	{
		{"Mozilla 2.2", "Mozilla/5.0 (Windows; U; Windows NT 6.1; rv:2.2) Gecko/20110201"},
		{"Mozilla 1.9.2.3", "Mozilla/5.0 (Windows; U; Windows NT 5.1; pl; rv:1.9.2.3) Gecko/20100401 Lightningquail/3.6.3"},
		{"Internet Explorer 11.0", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko"},
		{"Internet Explorer 9.0", "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US))"},
		{"Internet Explorer 8.0", "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; GTB7.4; InfoPath.2; SV1; .NET CLR 3.3.69573; WOW64; en-US)"},
		{"Maxthon 3.0.8.2", "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.1 (KHTML, like Gecko) Maxthon/3.0.8.2 Safari/533.1"},
		{"Chrome 41.0.2228.0", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36"},
		{"Chrome 41.0.2227.1", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36"},
		{"Chrome 40.0.2214.93", "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.93 Safari/537.36"}
	};

	public static Uri Combine(Uri baseUri, string path) { return Combine(baseUri.String(), path); }
	public static Uri Combine(Uri baseUri, Uri path) { return Combine(baseUri.String(), path.String()); }
	public static Uri Combine(string baseUri, Uri path) { return Combine(baseUri, path.String()); }
	public static Uri Combine(string baseUri, string path)
	{
		UriBuilder builder = ToUriBuilder(baseUri, path, out bool relative);
		return builder == null
					? null
					: relative
						? builder.Uri.MakeRelativeUri()
						: builder.Uri;
	}

	public static Uri Combine(string baseUri, [NotNull] params string[] paths)
	{
		Uri bu = ToUri(baseUri);
		return bu == null
					? null
					: Combine(bu, paths);
	}

	public static Uri Combine(Uri baseUri, [NotNull] params string[] paths)
	{
		if (paths.IsNullOrEmpty()) return baseUri;

		UriBuilder builder;
		bool relative = false, partsAdded = false;

		if (baseUri == null)
		{
			builder = new UriBuilder();
			relative = true;
		}
		else
		{
			if (baseUri.IsAbsoluteUri)
			{
				builder = new UriBuilder(baseUri);
				partsAdded = true;
			}
			else
			{
				builder = new UriBuilder().AddPathSeparator();
				builder.Path += baseUri.String().TrimStart('/');
				relative = true;
				partsAdded = true;
			}
		}

		foreach (string path in paths)
		{
			string escaped = Escape(path);
			if (escaped == null) continue;

			if (Uri.IsWellFormedUriString(escaped, UriKind.Absolute))
			{
				if (partsAdded) throw new NotSupportedException("Operation is not supported for absolute URI.");
				builder = new UriBuilder(escaped);
				relative = false;
				partsAdded = true;
				continue;
			}

			builder.AddPathSeparator().Path += escaped.TrimStart('/');
			partsAdded = true;
		}

		return relative
					? builder.Uri.MakeRelativeUri()
					: builder.Uri;
	}

	public static Uri ToUri(string value, UriKind kind = UriKind.RelativeOrAbsolute) { return ToUri(value, null, kind); }
	public static Uri ToUri(string value, string path, UriKind kind = UriKind.RelativeOrAbsolute)
	{
		if (kind == UriKind.Absolute)
		{
			UriBuilder builder = ToUriBuilder(value, path, out bool relative);
			return builder == null
						? null
						: relative
							? builder.Uri.MakeRelativeUri()
							: builder.Uri;
		}

		value = Escape(value, true);
		if (string.IsNullOrEmpty(value)) return null;
		return Uri.TryCreate(value, kind, out Uri uri)
					? uri
					: null;
	}

	public static UriBuilder ToUriBuilder(string value, out bool relative) { return ToUriBuilder(value, null, out relative); }
	public static UriBuilder ToUriBuilder(string value, string path, out bool relative)
	{
		relative = false;
		value = Escape(value, true);
		path = Escape(path, string.IsNullOrEmpty(value));
		if (value == null && path == null) return null;
		if (path != null && Uri.IsWellFormedUriString(path, UriKind.Absolute)) return new UriBuilder(path);

		UriBuilder builder = null;

		if (value != null)
		{
			if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
			{
				builder = new UriBuilder(value);
			}
			else
			{
				builder = new UriBuilder().AddPathSeparator();
				builder.Path += value.TrimStart('/');
				relative = true;
			}
		}

		if (path == null || !Uri.IsWellFormedUriString(path, UriKind.Relative)) return builder;

		if (builder == null)
		{
			if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
			{
				builder = new UriBuilder(path);
			}
			else if (Uri.IsWellFormedUriString(path, UriKind.Relative))
			{
				builder = new UriBuilder().AddPathSeparator();
				builder.Path += path.TrimStart('/');
				relative = true;
			}
		}
		else
		{
			builder.AddPathSeparator().Path += path.TrimStart('/');
		}

		return builder;
	}

	public static bool TryBuildUri(string value, out Uri uri, UriKind kind = UriKind.RelativeOrAbsolute) { return TryBuildUri(value, null, out uri, kind); }
	public static bool TryBuildUri(string value, string path, out Uri uri, UriKind kind = UriKind.RelativeOrAbsolute)
	{
		uri = ToUri(value, path, kind);
		return uri != null;
	}

	public static bool TryBuildUri(string value, out UriBuilder builder, out bool relative) { return TryBuildUri(value, null, out builder, out relative); }
	public static bool TryBuildUri(string value, string path, out UriBuilder builder, out bool relative)
	{
		builder = ToUriBuilder(value, path, out relative);
		return builder != null;
	}

	public static string GetHostUrl([NotNull] string value) { return GetHostUrl(ToUri(value, UriKind.Absolute)); }
	public static string GetHostUrl(Uri value)
	{
		return value == null
					? null
					: value.GetLeftPart(UriPartial.Authority).Trim(Path.AltDirectorySeparatorChar);
	}

	public static string RandomUserAgent()
	{
		return UserAgents.Count == 0
					? null
					: UserAgents.Values[RNGRandomHelper.Default.Next(0, UserAgents.Values.Count - 1)];
	}

	public static WebRequest MakeHttpWebRequest([NotNull] string url, IOHttpRequestSettings settings) { return MakeHttpWebRequest<HttpWebRequest>(url, settings); }

	public static T MakeHttpWebRequest<T>([NotNull] string url, IOHttpRequestSettings settings)
		where T : HttpWebRequest
	{
		Uri uri = ToUri(url, UriKind.Absolute);
		return uri == null ? default(T) : MakeHttpWebRequest<T>(uri, settings);
	}

	[NotNull]
	public static WebRequest MakeHttpWebRequest([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings) { return MakeHttpWebRequest<HttpWebRequest>(url, settings); }

	[NotNull]
	public static T MakeHttpWebRequest<T>([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
		where T : HttpWebRequest
	{
		T request = (T)WebRequest.Create(url);
		request.AllowAutoRedirect = settings.AllowAutoRedirect;
		request.AllowWriteStreamBuffering = settings.AllowWriteStreamBuffering;
		request.Timeout = settings.Timeout;
		if (settings.Accept?.Count > 0) request.Accept = settings.Accept.FirstOrDefault()?.MediaType;
		request.CachePolicy = settings.CachePolicy;
		request.UserAgent = settings.UserAgent;
		request.UseDefaultCredentials = settings.Credentials == null;
		request.Credentials = settings.Credentials ?? CredentialCache.DefaultNetworkCredentials;
		request.Method = settings.Method.ToWebMethod();
		request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
		if (settings.Proxy != null) request.Proxy = settings.Proxy;
		return request;
	}

	public static FtpWebRequest MakeFtpRequest([NotNull] string url, IOFTPRequestSettings settings)
	{
		Uri uri = ToUri(url, UriKind.Absolute);
		return uri == null ? null : MakeFtpRequest(uri, settings);
	}

	[NotNull]
	public static FtpWebRequest MakeFtpRequest([NotNull] Uri url, [NotNull] IOFTPRequestSettings settings)
	{
		FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
		request.Timeout = settings.Timeout;
		request.UsePassive = settings.UsePassive;
		request.UseBinary = settings.UseBinary;
		request.Method = settings.Method.ToWebMethod();
		request.CachePolicy = settings.CachePolicy;
		if (settings.Credentials != null) request.Credentials = settings.Credentials;
		if (settings.Proxy != null) request.Proxy = settings.Proxy;
		return request;
	}

	public static FileWebRequest MakeFileWebRequest([NotNull] string url, IOFileWebRequestSettings settings)
	{
		Uri uri = ToUri(url, UriKind.Absolute);
		return uri == null ? null : MakeFileWebRequest(uri, settings);
	}

	[NotNull]
	public static FileWebRequest MakeFileWebRequest([NotNull] Uri url, [NotNull] IOFileWebRequestSettings settings)
	{
		FileWebRequest request = (FileWebRequest)WebRequest.Create(url);
		request.Timeout = settings.Timeout;
		request.CachePolicy = settings.CachePolicy;
		request.UseDefaultCredentials = settings.Credentials == null;
		request.Credentials = settings.Credentials ?? CredentialCache.DefaultNetworkCredentials;
		if (settings.Proxy != null) request.Proxy = settings.Proxy;

		switch (settings)
		{
			case IODownloadFileWebRequestSettings dnSettings:
				request.Method = dnSettings.Method.ToWebMethod();
				break;
			case IOUploadFileWebRequestSettings upSettings:
				request.Method = upSettings.Method.ToWebMethod();
				break;
		}

		return request;
	}

	public static bool IsWellFormedUriString(string value, UriKind kind, params string[] scheme)
	{
		value = value?.Trim();
		if (string.IsNullOrEmpty(value)) return false;
		if (!TryBuildUri(value, out Uri outUri, kind) || outUri == null) return false;
		if (!outUri.IsAbsoluteUri || scheme == null || scheme.Length == 0) return true;
		return scheme.Length == 0 || scheme.All(s => s != null && Schemes.Contains(s));
	}

	public static bool IsValidUrl(string value, UriKind kind = UriKind.Absolute) { return IsWellFormedUriString(value, kind); }

	public static bool IsAbsoluteUriOrIP(string value)
	{
		if (IsValidUrl(value)) return true;
		return IPAddressHelper.ExtractIP(value) != null;
	}

	public static bool IsSameHost(string host1, string host2)
	{
		host1 = GetHostName(host1);
		return host1 != null && host1.IsSame(GetHostName(host2));
	}

	public static string GetHostName(string url) { return GetHostName(url, out int _); }

	public static string GetHostName(string url, out int port)
	{
		port = 0;
		url = url?.Trim();
		if (string.IsNullOrEmpty(url)) return null;

		Uri uri = ToUri(url, UriKind.Absolute);
		if (uri == null) return null;
		if (uri.Port > 0 && uri.Port != 80) port = uri.Port;
		return uri.Host;
	}

	public static IPHostEntry ResolveHost(string urlOrIp)
	{
		urlOrIp = urlOrIp?.Trim();
		if (string.IsNullOrEmpty(urlOrIp)) return null;

		try
		{
			IPHostEntry ipHost = Dns.GetHostEntry(urlOrIp);
			return ipHost.AddressList != null ? ipHost : null;
		}
		catch
		{
			return null;
		}
	}

	public static string GetTitle([NotNull] string url, IOHttpRequestSettings settings)
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.GetTitle(settings);
	}

	public static string GetTitle([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
	{
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.GetTitle(settings);
	}

	[NotNull]
	public static Task<string> GetTitleAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.GetTitleAsync(settings, token);
	}

	[NotNull]
	public static Task<string> GetTitleAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.GetTitleAsync(settings, token);
	}

	public static (string Title, string Buffer) Peek([NotNull] string url, IOHttpRequestSettings settings)
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.Peek(settings);
	}

	public static (string Title, string Buffer) Peek([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
	{
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.Peek(settings);
	}

	[NotNull]
	public static Task<(string Title, string Buffer)> PeekAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.PeekAsync(settings, token);
	}

	[NotNull]
	public static Task<(string Title, string Buffer)> PeekAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.PeekAsync(settings, token);
	}

	[NotNull]
	public static UrlSearchResult Search([NotNull] string url, UrlSearchFlags flags, IOHttpRequestSettings settings) { return Search(url, null, flags, settings); }

	[NotNull]
	public static UrlSearchResult Search([NotNull] string url, string searchFor, UrlSearchFlags flags, IOHttpRequestSettings settings)
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.Search(searchFor, flags, settings);
	}

	[NotNull]
	public static UrlSearchResult Search([NotNull] Uri url, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings) { return Search(url, null, flags, settings); }

	[NotNull]
	public static UrlSearchResult Search([NotNull] Uri url, string searchFor, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings)
	{
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.Search(searchFor, flags, settings);
	}

	[NotNull]
	public static Task<UrlSearchResult> SearchAsync([NotNull] string url, UrlSearchFlags flags, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		return SearchAsync(url, null, flags, settings, token);
	}

	[NotNull]
	public static Task<UrlSearchResult> SearchAsync([NotNull] string url, string searchFor, UrlSearchFlags flags, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.SearchAsync(searchFor, flags, settings, token);
	}

	[NotNull]
	public static Task<UrlSearchResult> SearchAsync([NotNull] Uri url, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		return SearchAsync(url, null, flags, settings, token);
	}

	[NotNull]
	public static Task<UrlSearchResult> SearchAsync([NotNull] Uri url, string searchFor, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.SearchAsync(searchFor, flags, settings, token);
	}

	public static string GetString([NotNull] string url, IOHttpRequestSettings settings)
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.GetString(settings);
	}

	public static string GetString([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
	{
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.GetString(settings);
	}

	[NotNull]
	public static Task<string> GetStringAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
		return request.GetStringAsync(settings, token);
	}

	[NotNull]
	public static Task<string> GetStringAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		WebRequest request = MakeHttpWebRequest(url, settings);
		return request.GetStringAsync(settings, token);
	}

	public static FileStream DownloadFile([NotNull] string url, [NotNull] string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings)
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		Uri uri = ToUri(url, UriKind.Absolute);
		return DownloadFile(uri, fileName, settings);
	}

	public static FileStream DownloadFile([NotNull] Uri url, string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings)
	{
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		IProgress<int> progress = settings.Progress;
		progress?.Report(0);

		WebRequest request = MakeHttpWebRequest(url, settings);
		WebHeaderCollection headers = request.Headers;
		headers.Add(HttpRequestHeader.CacheControl, "no-cache");
		headers.Add(HttpRequestHeader.CacheControl, "no-store");
		headers.Add(HttpRequestHeader.CacheControl, "max-age=1");
		headers.Add(HttpRequestHeader.CacheControl, "s-maxage=1");
		headers.Add(HttpRequestHeader.Pragma, "no-cache");
		headers.Add(HttpRequestHeader.Expires, "-1");
		fileName = Path.GetFullPath(fileName);

		string path = Path.GetDirectoryName(fileName) ?? throw new ArgumentException("Cannot get parent directory path.");
		if (!DirectoryHelper.Ensure(path)) return null;

		FileStream fileStream = null;
		HttpWebResponse response = null;
		Stream responseStream = null;
		bool success = false;

		try
		{
			response = (HttpWebResponse)request.GetResponse(settings);

			if (response == null)
			{
				progress?.Report(100);
				return null;
			}

			double received = 0.0d;
			double len = response.ContentLength;

			if (len.Equals(0.0d))
			{
				progress?.Report(100);
				return null;
			}

			if (len < 0.0d) progress = null;

			FileMode mode = settings.Overwrite
								? FileMode.Create
								: FileMode.CreateNew;
			fileStream = new FileStream(fileName, mode, FileAccess.Write);

			int read;
			byte[] buffer = new byte[settings.BufferSize];
			responseStream = response.GetResponseStream() ?? throw new IOException("Could not get response stream.");

			while ((read = responseStream.Read(buffer)) > 0)
			{
				received += read;
				fileStream.Write(buffer, 0, read);
				if (progress == null) continue;
				progress.Report((int)(received / len * 100.0d));
				Thread.Sleep(0);
			}

			fileStream.Position = 0;
			success = true;
		}
		finally
		{
			request.Abort();
			ObjectHelper.Dispose(ref responseStream);
			ObjectHelper.Dispose(ref response);

			if (!success && fileStream != null)
			{
				ObjectHelper.Dispose(ref fileStream);
				FileHelper.Delete(fileName);
			}
		}

		progress?.Report(100);
		return fileStream;
	}

	[NotNull]
	public static Task<FileStream> DownloadFileAsync([NotNull] string url, [NotNull] string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		token.ThrowIfCancellationRequested();
		Uri uri = ToUri(url, UriKind.Absolute) ?? throw new ArgumentException("Url is invalid.", nameof(url));
		return DownloadFileAsync(uri, fileName, settings, token);
	}

	public static Task<FileStream> DownloadFileAsync([NotNull] Uri url, string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		IProgress<int> progress = settings.Progress;
		progress?.Report(0);

		WebRequest request = MakeHttpWebRequest(url, settings);
		WebHeaderCollection headers = request.Headers;
		headers.Add(HttpRequestHeader.CacheControl, "no-cache");
		headers.Add(HttpRequestHeader.CacheControl, "no-store");
		headers.Add(HttpRequestHeader.CacheControl, "max-age=1");
		headers.Add(HttpRequestHeader.CacheControl, "s-maxage=1");
		headers.Add(HttpRequestHeader.Pragma, "no-cache");
		headers.Add(HttpRequestHeader.Expires, "-1");
		fileName = Path.GetFullPath(fileName);

		string path = Path.GetDirectoryName(fileName) ?? throw new ArgumentException("Cannot get parent directory path.");
		if (!DirectoryHelper.Ensure(path)) return null;

		FileStream fileStream = null;
		Stream responseStream = null;
		bool success = false;
		return request.GetResponseAsync(settings, token)
					.ContinueWith(task =>
					{
						WebResponse response = task.Result;

						if (response == null)
						{
							progress?.Report(100);
							return null;
						}

						try
						{
							double received = 0.0d;
							double len = response.ContentLength;

							if (len.Equals(0.0d))
							{
								progress?.Report(100);
								return null;
							}

							if (len < 0.0d) progress = null;

							FileMode mode = settings.Overwrite
												? FileMode.Create
												: FileMode.CreateNew;
							fileStream = new FileStream(fileName, mode, FileAccess.Write);

							int read;
							byte[] buffer = new byte[settings.BufferSize];
							responseStream = response.GetResponseStream() ?? throw new IOException("Could not get response stream.");

							while (!token.IsCancellationRequested && (read = responseStream.ReadAsync(buffer, token).Execute()) > 0)
							{
								received += read;
								fileStream.WriteAsync(buffer, 0, read, token).Execute();
								progress?.Report((int)(received / len * 100.0d));
							}

							token.ThrowIfCancellationRequested();
							fileStream.Position = 0;
							success = true;
							progress?.Report(100);
							return fileStream;
						}
						finally
						{
							request.Abort();
							ObjectHelper.Dispose(ref responseStream);
							ObjectHelper.Dispose(ref response);

							if (!success && fileStream != null)
							{
								ObjectHelper.Dispose(ref fileStream);
								FileHelper.Delete(fileName);
							}
						}
					}, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
	}

	public static Uri UploadFile([NotNull] string fileName, [NotNull] string url, [NotNull] IOUploadFileWebRequestSettings settings)
	{
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		Uri uri = ToUri(url, UriKind.Absolute) ?? throw new ArgumentException("Url is invalid.", nameof(url));
		return UploadFile(fileName, uri, settings);
	}

	public static Uri UploadFile([NotNull] string fileName, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings)
	{
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			return UploadFile(stream, url, settings);
		}
	}

	public static Uri UploadFile([NotNull] Stream stream, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings)
	{
		if (stream.CanSeek && stream.Position > 0) stream.Position = 0;

		/*
		* todo: detect if this is a text file?
		*/
		using (BinaryReader reader = new BinaryReader(stream))
		{
			return UploadFile(reader, url, settings);
		}
	}

	public static Uri UploadFile([NotNull] StreamReader reader, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings)
	{
		FileWebRequest request = GetFileUploadRequest(reader.BaseStream, url, settings, out byte[] header, out byte[] footer, out long length);
		Uri uri;
		WebResponse response = null;
		Stream responseStream = null;
		Stream stream = null;

		try
		{
			stream = request.GetRequestStream();

			char[] buffer = new char[length];

			using (StreamWriter writer = new StreamWriter(stream, reader.CurrentEncoding, reader.CurrentEncoding.GetMaxByteCount(buffer.Length)))
			{
				int read;

				if (header is { Length: > 0 })
					stream.Write(header);

				while ((read = reader.Read(buffer)) > 0)
					writer.Write(buffer, 0, read);

				if (footer is { Length: > 0 })
					stream.Write(footer);
			}

			response = request.GetResponse();
			uri = response.ResponseUri;
		}
		finally
		{
			request.Abort();
			ObjectHelper.Dispose(ref stream);
			ObjectHelper.Dispose(ref responseStream);
			ObjectHelper.Dispose(ref response);
		}

		return uri;
	}

	public static Uri UploadFile([NotNull] BinaryReader reader, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings)
	{
		FileWebRequest request = GetFileUploadRequest(reader.BaseStream, url, settings, out byte[] header, out byte[] footer, out long length);
		Uri uri;
		WebResponse response = null;
		Stream responseStream = null;
		Stream stream = null;

		try
		{
			stream = request.GetRequestStream();

			if (header is { Length: > 0 })
				stream.Write(header);

			int read;
			byte[] buffer = new byte[length];

			while ((read = reader.Read(buffer)) > 0)
				stream.Write(buffer, 0, read);

			if (footer is { Length: > 0 })
				stream.Write(footer);

			response = request.GetResponse();
			uri = response.ResponseUri;
		}
		finally
		{
			request.Abort();
			ObjectHelper.Dispose(ref stream);
			ObjectHelper.Dispose(ref responseStream);
			ObjectHelper.Dispose(ref response);
		}

		return uri;
	}

	[NotNull]
	public static Task<Uri> UploadFileAsync([NotNull] string fileName, [NotNull] string url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
		token.ThrowIfCancellationRequested();
		Uri uri = ToUri(url, UriKind.Absolute) ?? throw new ArgumentException("Url is invalid.", nameof(url));
		return UploadFileAsync(fileName, uri, settings, token);
	}

	[NotNull]
	public static async Task<Uri> UploadFileAsync([NotNull] string fileName, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		token.ThrowIfCancellationRequested();

		FileStream stream = null;

		try
		{
			stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			return await UploadFileAsync(stream, url, settings, token);
		}
		finally
		{
			ObjectHelper.Dispose(ref stream);
		}
	}

	[NotNull]
	public static async Task<Uri> UploadFileAsync([NotNull] Stream stream, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (stream.CanSeek && stream.Position > 0) stream.Position = 0;

		/*
		* todo: detect if this is a text file?
		*/
		BinaryReader reader = null;

		try
		{
			reader = new BinaryReader(stream);
			return await UploadFileAsync(reader, url, settings, token);
		}
		finally
		{
			ObjectHelper.Dispose(ref reader);
		}
	}

	public static async Task<Uri> UploadFileAsync([NotNull] StreamReader reader, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		FileWebRequest request = GetFileUploadRequest(reader.BaseStream, url, settings, out byte[] header, out byte[] footer, out long length);
		Uri uri;
		WebResponse response = null;
		Stream responseStream = null;
		Stream stream = null;
		StreamWriter writer = null;

		try
		{
			stream = await request.GetRequestStreamAsync();
			token.ThrowIfCancellationRequested();

			char[] buffer = new char[length];
			writer = new StreamWriter(stream, reader.CurrentEncoding, reader.CurrentEncoding.GetMaxByteCount(buffer.Length));
			if (header is { Length: > 0 }) await stream.WriteAsync(header, token);

			int read;

			while (!token.IsCancellationRequested && (read = await reader.ReadAsync(buffer)) > 0)
				await writer.WriteAsync(buffer, 0, read);

			token.ThrowIfCancellationRequested();
			if (footer is { Length: > 0 }) await stream.WriteAsync(footer, token);
			response = await request.GetResponseAsync(settings, token);
			token.ThrowIfCancellationRequested();
			uri = response.ResponseUri;
		}
		finally
		{
			request.Abort();
			ObjectHelper.Dispose(ref writer);
			ObjectHelper.Dispose(ref stream);
			ObjectHelper.Dispose(ref responseStream);
			ObjectHelper.Dispose(ref response);
		}

		return uri;
	}

	public static async Task<Uri> UploadFileAsync([NotNull] BinaryReader reader, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		FileWebRequest request = GetFileUploadRequest(reader.BaseStream, url, settings, out byte[] header, out byte[] footer, out long length);
		Uri uri;
		WebResponse response = null;
		Stream responseStream = null;
		Stream stream = null;

		try
		{
			stream = await request.GetRequestStreamAsync();
			token.ThrowIfCancellationRequested();
			if (header is { Length: > 0 }) await stream.WriteAsync(header, token);

			int read;
			byte[] buffer = new byte[length];

			while ((read = reader.Read(buffer)) > 0)
			{
				token.ThrowIfCancellationRequested();
				await stream.WriteAsync(buffer, 0, read, token);
			}

			token.ThrowIfCancellationRequested();
			if (footer is { Length: > 0 }) await stream.WriteAsync(footer, token);
			response = await request.GetResponseAsync(settings, token);
			token.ThrowIfCancellationRequested();
			uri = response.ResponseUri;
		}
		finally
		{
			request.Abort();
			ObjectHelper.Dispose(ref stream);
			ObjectHelper.Dispose(ref responseStream);
			ObjectHelper.Dispose(ref response);
		}

		return uri;
	}

	[NotNull]
	public static string SetQueryParameter(string url, [NotNull] string key, string value)
	{
		url ??= string.Empty;
		if (string.IsNullOrEmpty(key)) return url;
		key = WebUtility.UrlEncode(key) ?? string.Empty;
		value = WebUtility.UrlEncode(value ?? string.Empty);

		Match existingMatch = Regex.Match(url, $"[?&]({Regex.Escape(key)}=?.*?)(?:&|/|$)");

		// Parameter already set to something
		if (existingMatch.Success)
		{
			Group group = existingMatch.Groups[1];
			url = url.Remove(group.Index, group.Length);
			url = url.Insert(group.Index, $"{key}={value}");
			return url;
		}

		char separator = url.IndexOf('?') >= 0 ? '&' : '?';
		return url + separator + key + '=' + value;
	}

	[NotNull]
	public static string SetPathParameter(string url, [NotNull] string key, string value)
	{
		url ??= string.Empty;
		if (string.IsNullOrEmpty(key)) return url;
		key = WebUtility.UrlEncode(key) ?? string.Empty;
		value = WebUtility.UrlEncode(value ?? string.Empty);

		Match existingMatch = Regex.Match(url, $"/({Regex.Escape(key)}/?.*?)(?:/|$)");
		if (!existingMatch.Success) return url + '/' + key + '/' + value;

		Group group = existingMatch.Groups[1];
		url = url.Remove(group.Index, group.Length);
		url = url.Insert(group.Index, $"{key}/{value}");
		return url;
	}

	[NotNull]
	public static IDictionary<string, string> GetDictionaryFromUrlQuery(string urlEncoded)
	{
		Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrEmpty(urlEncoded)) return dic;

		int n = urlEncoded.IndexOf('?');

		if (n >= 0)
		{
			if (n == urlEncoded.Length - 1) return dic;
			urlEncoded = urlEncoded.Substring(n + 1);
		}

		string[] keyValuePairsRaw = urlEncoded.Split('&');

		foreach (string keyValuePairRaw in keyValuePairsRaw)
		{
			string keyValuePairRawDecoded = WebUtility.UrlDecode(keyValuePairRaw);

			// Look for the equals sign
			int equalsPos = keyValuePairRawDecoded.IndexOf('=');
			if (equalsPos <= 0) continue;

			// Get the key and value
			string key = keyValuePairRawDecoded.Substring(0, equalsPos);
			string value = equalsPos < keyValuePairRawDecoded.Length
								? keyValuePairRawDecoded.Substring(equalsPos + 1)
								: string.Empty;
			dic[key] = value;
		}

		return dic;
	}

	public static bool IsBadRedirect(string url, string logOffUrl)
	{
		if (string.IsNullOrWhiteSpace(url)) return true;
		if (!IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) return true;

		Regex regex = CreateBadRedirectExpression(logOffUrl);
		return regex?.IsMatch(url) ?? false;
	}

	public static Regex CreateBadRedirectExpression(string logOffUrl)
	{
		const string URL_PART_SEPARATOR = "(?:%2F|/)";

		logOffUrl = logOffUrl?.Trim();
		if (string.IsNullOrEmpty(logOffUrl) || !IsWellFormedUriString(logOffUrl, UriKind.Relative)) return null;

		StringBuilder sb = new StringBuilder("(?:[?&]ReturnUrl=)?");

		if (logOffUrl.Contains('/'))
		{
			logOffUrl = logOffUrl.Replace("//", "/");
			string[] parts = logOffUrl.Split('/');

			foreach (string part in parts)
			{
				sb.Append(URL_PART_SEPARATOR + part);
			}
		}
		else
		{
			sb.Append(logOffUrl);
		}

		Regex regex = new Regex(sb.ToString(), RegexHelper.OPTIONS_I);
		return regex;
	}

	public static string LocalPath(string url)
	{
		if (string.IsNullOrEmpty(url)) return url;
		if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) return null;

		Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
		return uri.LocalPath;
	}

	public static HttpMethod HttpMethodFromString(string value)
	{
		value = value?.Trim();
		if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
		return __httpMethodsCache.GetOrAdd(value, _ =>
		{
			Type type = typeof(HttpMethod);
			PropertyInfo property = type.GetProperty(value, Constants.BF_PUBLIC_STATIC, type);
			return property != null
						? (HttpMethod)property.GetValue(null)
						: new HttpMethod(value);
		});
	}

	public static string GetFileName(string url) { return GetFileName(ToUri(url)); }
	public static string GetFileName(Uri url)
	{
		if (url == null) return null;

		if (!url.IsAbsoluteUri)
		{
			if (Uri.TryCreate(new Uri("https://temp.org"), url, out Uri tmp)) url = tmp;
		}

		string path = url.IsAbsoluteUri
						? url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)
						: url.PathAndQuery;
		return GetFileName(path);
	}

	public static string Trim(string url)
	{
		if (url == null) return null;
		url = url.TrimEnd('/', ' ');
		if (url.Length == 0) return string.Empty;

		int n = -1;

		while (n < url.Length - 1 && url[n + 1] == '/')
			n++;

		if (n > 0)
		{
			url = n == url.Length - 1
					? "/"
					: url.Right(url.Length - n);
		}

		return url;
	}

	public static string Secure(string url)
	{
		url = Trim(url);
		return url?.Replace(__uriSchemeHttp, __uriSchemeHttps, StringComparison.OrdinalIgnoreCase, 0, __uriSchemeHttps.Length);
	}

	public static string Escape(string value, bool checkScheme = false)
	{
		value = Trim(value);
		if (string.IsNullOrEmpty(value)) return value;
		if (checkScheme && !value.StartsWith('/') && !value.Contains(Uri.SchemeDelimiter, StringComparison.Ordinal)) value = Uri.UriSchemeHttp + Uri.SchemeDelimiter + value;
		if (Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute)) return value;

		if (value.Contains('/'))
		{
			int schemeSeparator = value.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);

			if (schemeSeparator > -1)
			{
				schemeSeparator += Uri.SchemeDelimiter.Length;
				if (schemeSeparator == value.Length - 1) return value;
			}

			StringBuilder sb = new StringBuilder(value.Length);
			bool firstPart = false;

			if (schemeSeparator > -1)
			{
				sb.Append(value.Left(schemeSeparator));
				value = value.Right(value.Length - schemeSeparator);
				firstPart = true;
			}

			foreach (string part in value.Enumerate('/')
										.SkipNullOrEmpty())
			{
				if (sb.Length > 0 && sb[sb.Length - 1] != '/') sb.Append('/');

				if (firstPart)
				{
					int index = part.IndexOf(':');

					if (index > -1)
					{
						if (part.Contains(':', index + 1))
						{
							// this is probably an IPv6. Just add it for now.
							sb.Append(part);
						}
						else
						{
							// this entry probably contains a port number.
							string[] parts = part.Split(2, StringSplitOptions.RemoveEmptyEntries, ':');
							if (Uri.EscapeDataString(parts[0]) != parts[0]) throw new InvalidOperationException($"Bad host name '{parts[0]}'.");
							sb.Append(parts[0]);

							if (parts.Length > 1)
							{
								if (!int.TryParse(parts[1], out int port)) throw new InvalidOperationException($"Invalid port number '{parts[1]}'.");

								if (port > 0 && port != 80)
								{
									sb.Append(':');
									sb.Append(parts[1]);
								}
							}
						}
					}
					else
					{
						sb.Append(Uri.EscapeDataString(part));
					}

					firstPart = false;
				}
				else
				{
					sb.Append(Uri.EscapeDataString(part));
				}
			}

			value = sb.ToString();
		}
		else
		{
			value = Uri.EscapeDataString(value);
		}

		return value;
	}

	[NotNull]
	private static FileWebRequest GetFileUploadRequest([NotNull] Stream baseStream, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, out byte[] header, out byte[] footer, out long length)
	{
		WebHeaderCollection headers = new WebHeaderCollection
		{
			["Content-Type"] = "application/octet-stream"
		};

		long contentLength = 0;
		bool needsHeaderAndBoundary = url.Scheme != Uri.UriSchemeFile;
		length = Constants.GetBufferMB(8);

		if (settings.Method == FileWebRequestMethod.Post)
		{
			if (needsHeaderAndBoundary)
			{
				string fileName = baseStream.GetFileName();
				if (string.IsNullOrEmpty(fileName)) fileName = FileHelper.GetRandomName();

				string separator = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
				headers["Content-Type"] = $"multipart/form-data; boundary={separator}";
				string headerStr =
					$"--{separator}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{Path.GetFileName(fileName)}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
				header = Encoding.UTF8.GetBytes(headerStr);
				footer = Encoding.ASCII.GetBytes($"\r\n--{separator}--\r\n");
			}
			else
			{
				header = null;
				footer = null;
			}

			if (baseStream.CanSeek)
			{
				contentLength = baseStream.Length + (header?.LongLength ?? 0L) + (footer?.LongLength ?? 0L);
				length = Math.Min(length, baseStream.Length);
			}
		}
		else
		{
			header = null;
			footer = null;

			if (baseStream.CanSeek)
			{
				contentLength = baseStream.Length;
				length = (int)Math.Min(length, baseStream.Length);
			}
		}

		if (contentLength <= 0L) contentLength = -1L;

		FileWebRequest request = MakeFileWebRequest(url, settings);
		request.CopyHeaders(headers);
		request.ContentLength = contentLength;
		if (request.RequestUri.Scheme == Uri.UriSchemeFile) header = footer = null;
		return request;
	}
}