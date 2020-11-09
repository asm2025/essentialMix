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
using asm.Extensions;
using JetBrains.Annotations;
using asm.IO;
using asm.Web;

namespace asm.Helpers
{
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

		static UriHelper()
		{
			//Windows 7 fix
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		}

		public static Uri Combine([NotNull] Uri baseUri, string path) { return Combine(baseUri.ToString(), path); }
		public static Uri Combine([NotNull] Uri baseUri, [NotNull] Uri path) { return Combine(baseUri.ToString(), path.ToString()); }
		public static Uri Combine(string baseUri, [NotNull] Uri path) { return Combine(baseUri, path.ToString()); }
		public static Uri Combine(string baseUri, string path)
		{
			UriBuilder builder = ToUriBuilder(baseUri, path);
			return builder?.Uri;
		}

		[NotNull]
		public static Uri Join([NotNull] string baseUri, [NotNull] params string[] paths) { return Join(ToUri(baseUri), paths); }

		[NotNull]
		public static Uri Join([NotNull] Uri baseUri, [NotNull] params string[] paths)
		{
			if (paths.IsNullOrEmpty()) return baseUri;

			UriBuilder builder = new UriBuilder(baseUri);

			foreach (string s in paths.SkipNullOrEmptyTrim())
			{
				if (!Uri.IsWellFormedUriString(s, UriKind.Relative)) continue;
				builder.Path += s;
			}

			return builder.Uri;
		}

		public static Uri ToUri([NotNull] string value, UriKind kind = UriKind.RelativeOrAbsolute, string path = null)
		{
			if (kind == UriKind.Absolute) return ToUriBuilder(value, path)?.Uri;
			value = Trim(value);
			return value == null
						? null
						: Uri.TryCreate(value, kind, out Uri uri)
							? uri
							: null;
		}

		public static bool ToUri([NotNull] string value, out Uri uri, UriKind kind = UriKind.RelativeOrAbsolute, string path = null)
		{
			uri = ToUri(value, kind, path);
			return uri != null;
		}

		public static UriBuilder ToUriBuilder(string value, string path = null)
		{
			value = Trim(value);
			path = Trim(path);
			if (value == null && path == null) return null;

			UriBuilder builder = null;

			if (!string.IsNullOrEmpty(path) && Uri.IsWellFormedUriString(path, UriKind.Absolute))
				builder = new UriBuilder(path);

			if (builder != null) return builder;
			if (!string.IsNullOrEmpty(value) && Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute)) builder = new UriBuilder(value);
			if (string.IsNullOrEmpty(path) || !Uri.IsWellFormedUriString(path, UriKind.Relative)) return builder;

			if (builder == null) builder = new UriBuilder(path);
			else builder.Path += Path.AltDirectorySeparatorChar + path.TrimStart(Path.AltDirectorySeparatorChar);

			return builder;
		}

		public static bool ToUriBuilder(string value, out UriBuilder builder, string path = null)
		{
			builder = ToUriBuilder(value, path);
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
						: UserAgents.Values[RandomHelper.Default.Next(0, UserAgents.Values.Count - 1)];
		}

		public static WebRequest BasicHttpWebRequest([NotNull] string url, IOHttpRequestSettings settings) { return BasicHttpWebRequest<HttpWebRequest>(url, settings); }

		public static T BasicHttpWebRequest<T>([NotNull] string url, IOHttpRequestSettings settings)
			where T : HttpWebRequest
		{
			Uri uri = ToUri(url, UriKind.Absolute);
			return uri == null ? default(T) : BasicHttpWebRequest<T>(uri, settings);
		}

		[NotNull]
		public static WebRequest BasicHttpWebRequest([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings) { return BasicHttpWebRequest<HttpWebRequest>(url, settings); }

		[NotNull]
		public static T BasicHttpWebRequest<T>([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
			where T : HttpWebRequest
		{
			T request = (T)WebRequest.Create(url);
			request.AllowAutoRedirect = settings.AllowAutoRedirect;
			request.AllowWriteStreamBuffering = settings.AllowWriteStreamBuffering;
			request.Timeout = settings.Timeout;
			if (settings.Accept.Count > 0) request.Accept = settings.Accept.FirstOrDefault()?.MediaType;
			request.CachePolicy = settings.CachePolicy;
			request.UserAgent = settings.UserAgent;
			request.UseDefaultCredentials = settings.Credentials == null;
			request.Credentials = settings.Credentials ?? CredentialCache.DefaultNetworkCredentials;
			request.Method = settings.Method.ToWebMethod();
			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			if (settings.Proxy != null) request.Proxy = settings.Proxy;
			return request;
		}

		public static FtpWebRequest BasicFtpRequest([NotNull] string url, IOFTPRequestSettings settings)
		{
			Uri uri = ToUri(url, UriKind.Absolute);
			return uri == null ? null : BasicFtpRequest(uri, settings);
		}

		[NotNull]
		public static FtpWebRequest BasicFtpRequest([NotNull] Uri url, [NotNull] IOFTPRequestSettings settings)
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

		public static FileWebRequest BasicFileWebRequest([NotNull] string url, IOFileWebRequestSettings settings)
		{
			Uri uri = ToUri(url, UriKind.Absolute);
			return uri == null ? null : BasicFileWebRequest(uri, settings);
		}

		[NotNull]
		public static FileWebRequest BasicFileWebRequest([NotNull] Uri url, [NotNull] IOFileWebRequestSettings settings)
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
			if (!ToUri(value, out Uri outUri, kind) || outUri == null) return false;
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
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.GetTitle(settings);
		}

		public static string GetTitle([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
		{
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.GetTitle(settings);
		}

		[NotNull]
		public static Task<string> GetTitleAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.GetTitleAsync(settings, token);
		}

		[NotNull]
		public static Task<string> GetTitleAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.GetTitleAsync(settings, token);
		}

		public static (string Title, string Buffer) Peek([NotNull] string url, IOHttpRequestSettings settings)
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.Peek(settings);
		}

		public static (string Title, string Buffer) Peek([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
		{
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.Peek(settings);
		}

		[NotNull]
		public static Task<(string Title, string Buffer)> PeekAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.PeekAsync(settings, token);
		}

		[NotNull]
		public static Task<(string Title, string Buffer)> PeekAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.PeekAsync(settings, token);
		}

		[NotNull]
		public static UrlSearchResult Search([NotNull] string url, UrlSearchFlags flags, IOHttpRequestSettings settings) { return Search(url, null, flags, settings); }

		[NotNull]
		public static UrlSearchResult Search([NotNull] string url, string searchFor, UrlSearchFlags flags, IOHttpRequestSettings settings)
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.Search(searchFor, flags, settings);
		}

		[NotNull]
		public static UrlSearchResult Search([NotNull] Uri url, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings) { return Search(url, null, flags, settings); }

		[NotNull]
		public static UrlSearchResult Search([NotNull] Uri url, string searchFor, UrlSearchFlags flags, [NotNull] IOHttpRequestSettings settings)
		{
			WebRequest request = BasicHttpWebRequest(url, settings);
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
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
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
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.SearchAsync(searchFor, flags, settings, token);
		}

		public static string GetString([NotNull] string url, IOHttpRequestSettings settings)
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.GetString(settings);
		}

		public static string GetString([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings)
		{
			WebRequest request = BasicHttpWebRequest(url, settings);
			return request.GetString(settings);
		}

		[NotNull]
		public static Task<string> GetStringAsync([NotNull] string url, IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings) ?? throw new InvalidOperationException("Could not create a request.");
			return request.GetStringAsync(settings, token);
		}

		[NotNull]
		public static Task<string> GetStringAsync([NotNull] Uri url, [NotNull] IOHttpRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebRequest request = BasicHttpWebRequest(url, settings);
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
			
			WebRequest request = BasicHttpWebRequest(url, settings);
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

		public static async Task<FileStream> DownloadFileAsync([NotNull] string url, [NotNull] string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
			token.ThrowIfCancellationRequested();
			Uri uri = ToUri(url, UriKind.Absolute) ?? throw new ArgumentException("Url is invalid.", nameof(url));
			return await DownloadFileAsync(uri, fileName, settings, token).ConfigureAwait();
		}

		public static async Task<FileStream> DownloadFileAsync([NotNull] Uri url, string fileName, [NotNull] IOHttpDownloadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

			IProgress<int> progress = settings.Progress;
			progress?.Report(0);

			WebRequest request = BasicHttpWebRequest(url, settings);
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
			WebResponse response = null;
			Stream responseStream = null;
			bool success = false;

			try
			{
				response = await request.GetResponseAsync(settings, token);
				token.ThrowIfCancellationRequested();

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

				while (!token.IsCancellationRequested && (read = await responseStream.ReadAsync(buffer, token)) > 0)
				{
					received += read;
					await fileStream.WriteAsync(buffer, 0, read, token);
					if (progress == null) continue;
					progress.Report((int)(received / len * 100.0d));
					Thread.Sleep(0);
				}

				token.ThrowIfCancellationRequested();
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

					if (header != null && header.Length > 0)
						stream.Write(header);

					while ((read = reader.Read(buffer)) > 0) 
						writer.Write(buffer, 0, read);

					if (footer != null && footer.Length > 0)
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
				ObjectHelper.Dispose(ref request);
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

				if (header != null && header.Length > 0)
					stream.Write(header);

				int read;
				byte[] buffer = new byte[length];

				while ((read = reader.Read(buffer)) > 0) 
					stream.Write(buffer, 0, read);

				if (footer != null && footer.Length > 0)
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
				ObjectHelper.Dispose(ref request);
			}

			return uri;
		}

		public static async Task<Uri> UploadFileAsync([NotNull] string fileName, [NotNull] string url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
			token.ThrowIfCancellationRequested();
			Uri uri = ToUri(url, UriKind.Absolute) ?? throw new ArgumentException("Url is invalid.", nameof(url));
			return await UploadFileAsync(fileName, uri, settings, token).ConfigureAwait();
		}

		public static async Task<Uri> UploadFileAsync([NotNull] string fileName, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
			token.ThrowIfCancellationRequested();

			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return await UploadFileAsync(stream, url, settings, token);
			}
		}

		public static async Task<Uri> UploadFileAsync([NotNull] Stream stream, [NotNull] Uri url, [NotNull] IOUploadFileWebRequestSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (stream.CanSeek && stream.Position > 0) stream.Position = 0;

			/*
			 * todo: detect if this is a text file?
			 */
			using (BinaryReader reader = new BinaryReader(stream))
			{
				return await UploadFileAsync(reader, url, settings, token);
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

			try
			{
				stream = await request.GetRequestStreamAsync();
				token.ThrowIfCancellationRequested();

				char[] buffer = new char[length];

				using (StreamWriter writer = new StreamWriter(stream, reader.CurrentEncoding, reader.CurrentEncoding.GetMaxByteCount(buffer.Length)))
				{
					int read;
					token.ThrowIfCancellationRequested();
					if (header != null && header.Length > 0) await stream.WriteAsync(header, token);

					while ((read = await reader.ReadAsync(buffer)) > 0)
					{
						token.ThrowIfCancellationRequested();
						await writer.WriteAsync(buffer, 0, read);
					}

					token.ThrowIfCancellationRequested();
					if (footer != null && footer.Length > 0) await stream.WriteAsync(footer, token);
				}

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
				ObjectHelper.Dispose(ref request);
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
				if (header != null && header.Length > 0) await stream.WriteAsync(header, token);

				int read;
				byte[] buffer = new byte[length];

				while ((read = reader.Read(buffer)) > 0)
				{
					token.ThrowIfCancellationRequested();
					await stream.WriteAsync(buffer, 0, read, token);
				}

				token.ThrowIfCancellationRequested();
				if (footer != null && footer.Length > 0) await stream.WriteAsync(footer, token);
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
				ObjectHelper.Dispose(ref request);
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

			return __httpMethodsCache.GetOrAdd(value, v =>
			{
				Type type = typeof(HttpMethod);
				PropertyInfo property = type.GetProperty(value, Constants.BF_PUBLIC_STATIC, type);
				return property != null
										? (HttpMethod)property.GetValue(null)
										: new HttpMethod(value);
			});
		}

		public static string GetFileName(Uri url)
		{
			if (url == null) return null;

			if (!url.IsAbsoluteUri)
			{
				if (Uri.TryCreate(new Uri("https://temp.org"), url, out Uri tmp)) url = tmp;
			}

			string path = url.IsAbsoluteUri
							? url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)
							: url.ToString();
			return GetFileName(path);
		}

		public static string GetFileName(string url)
		{
			url = url?.Trim(' ', '/');
			if (string.IsNullOrEmpty(url)) return null;
			if (!IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) return null;

			int n = url.IndexOf('?');
			if (n > -1) url = url.Substring(0, n);
			n = url.LastIndexOf('/');

			if (n > -1)
			{
				n++;
				url = n < url.Length ? url.Substring(n) : null;
			}

			if (url != null && url.Length == 0) url = null;
			return url;
		}

		public static string Trim(string url) { return url?.Trim(PathHelper.AltDirectorySeparator, ' ').ToNullIfEmpty(); }

		public static string Secure(string url)
		{
			url = Trim(url);
			return url?.Replace(__uriSchemeHttp, __uriSchemeHttps, StringComparison.OrdinalIgnoreCase, 0, __uriSchemeHttps.Length);
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

			FileWebRequest request = BasicFileWebRequest(url, settings);
			request.CopyHeaders(headers);
			request.ContentLength = contentLength;
			if (request.RequestUri.Scheme == Uri.UriSchemeFile) header = footer = null;
			return request;
		}
	}
}