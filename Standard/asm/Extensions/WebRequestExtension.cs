using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Helpers;
using asm.IO;
using asm.Web;

namespace asm.Extensions
{
	public static class WebRequestExtension
	{
		private static readonly ConcurrentDictionary<string, PropertyInfo> __headerSet = new ConcurrentDictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

		public static bool WriteMultipart([NotNull] this WebRequest thisValue, string body, Action<Exception> onError = null, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(body)) return false;

			bool result;

			try
			{
				string[] multiParts = body.Contains(WebRequestHelper.DIVIDER) ? Regex.Split(body, WebRequestHelper.DIVIDER) : new[] { body };
				encoding ??= EncodingHelper.Default;

				using (MemoryStream ms = new MemoryStream())
				{
					foreach (string part in multiParts)
					{
						byte[] bytes = File.Exists(part) ? File.ReadAllBytes(part) : encoding.GetBytes(part.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"));
						ms.Write(bytes);
					}

					thisValue.ContentLength = ms.Length;

					using (Stream stream = thisValue.GetRequestStream())
					{
						ms.WriteTo(stream);
					}
				}

				result = true;
			}
			catch (Exception ex) when (onError != null)
			{
				result = false;
				onError(ex);
			}

			return result;
		}

		public static bool Read([NotNull] this WebRequest thisValue, [NotNull] IOResponseSettings settings)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return false;

			try
			{
				return settings.OnResponseReceived?.Invoke(response) != false && response.Read(settings);
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				settings.OnError(ex);
				return false;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static async Task<bool> ReadAsync([NotNull] this WebRequest thisValue, [NotNull] IOResponseSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return false;

			try
			{
				return settings.OnResponseReceived?.Invoke(response) != false && await response.ReadAsync(settings, token).ConfigureAwait();
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				settings.OnError(ex);
				return false;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static string ReadToEnd([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : response.ReadToEnd(settings);
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static async Task<string> ReadToEndAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : await response.ReadToEndAsync(settings, token);
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static string GetTitle([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : response.GetTitle(settings);
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static async Task<string> GetTitleAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : await response.GetTitleAsync(settings, token).ConfigureAwait();
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static (string Title, string Buffer) Peek([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return (null, null);

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? (null, null) : response.Peek(settings);
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return (null, null);
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static async Task<(string Title, string Buffer)> PeekAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return (null, null);

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? (null, null) : await response.PeekAsync(settings, token).ConfigureAwait();
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return (null, null);
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		[NotNull]
		public static UrlSearchResult Search([NotNull] this WebRequest thisValue, UrlSearchFlags flags, IOResponseSettings settings = null)
		{
			return Search(thisValue, null, flags, settings);
		}

		[NotNull]
		public static UrlSearchResult Search([NotNull] this WebRequest thisValue, string searchFor, UrlSearchFlags flags, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return new UrlSearchResult { Status = UrlSearchStatus.Failed };

			try
			{
				if (settings?.OnResponseReceived != null)
				{
					if (!settings.OnResponseReceived(response)) return new UrlSearchResult { Status = UrlSearchStatus.Failed };
				}

				UrlSearchResult result = response.Search(searchFor, flags, settings);
				result.RedirectUri = thisValue.RequestUri != response.ResponseUri
					? response.ResponseUri.IsAbsoluteUri || !thisValue.RequestUri.IsAbsoluteUri
						? response.ResponseUri
						: new Uri(thisValue.RequestUri, response.ResponseUri)
					: thisValue.RequestUri;
				return result;
			}
			catch (Exception ex)
			{
				settings?.OnError?.Invoke(ex);
				return new UrlSearchResult
				{
					Status = UrlSearchStatus.Error,
					Exception = ex
				};
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		[NotNull]
		public static async Task<UrlSearchResult> SearchAsync([NotNull] this WebRequest thisValue, UrlSearchFlags flags, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			return await SearchAsync(thisValue, null, flags, settings, token).ConfigureAwait();
		}

		[NotNull]
		public static async Task<UrlSearchResult> SearchAsync([NotNull] this WebRequest thisValue, string searchFor, UrlSearchFlags flags, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return null;

			try
			{
				if (settings?.OnResponseReceived != null)
				{
					if (!settings.OnResponseReceived(response)) return new UrlSearchResult { Status = UrlSearchStatus.Failed };
				}

				UrlSearchResult result = await response.SearchAsync(searchFor, flags, settings, token).ConfigureAwait();
				token.ThrowIfCancellationRequested();
				result.RedirectUri = thisValue.RequestUri != response.ResponseUri
					? response.ResponseUri.IsAbsoluteUri || !thisValue.RequestUri.IsAbsoluteUri
						? response.ResponseUri
						: new Uri(thisValue.RequestUri, response.ResponseUri)
					: thisValue.RequestUri;
				return result;
			}
			catch (Exception ex)
			{
				settings?.OnError?.Invoke(ex);
				return new UrlSearchResult
				{
					Status = UrlSearchStatus.Error,
					Exception = ex
				};
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static string GetString([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : response.GetString(settings);
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static async Task<string> GetStringAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : await response.GetStringAsync(settings, token).ConfigureAwait();
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
			finally
			{
				ObjectHelper.Dispose(ref response);
				thisValue.Abort();
			}
		}

		public static Stream GetStream([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			WebResponse response = GetResponse(thisValue, settings);
			if (response == null) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : response.GetStream();
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
		}

		public static async Task<Stream> GetStreamAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			WebResponse response = await GetResponseAsync(thisValue, settings, token);
			if (response == null || token.IsCancellationRequested) return null;

			try
			{
				return settings?.OnResponseReceived?.Invoke(response) == false ? null : response.GetStream();
			}
			catch (Exception ex) when (settings?.OnError != null)
			{
				settings.OnError(ex);
				return null;
			}
		}

		public static WebResponse GetResponse([NotNull] this WebRequest thisValue, IOResponseSettings settings = null)
		{
			if (settings == null) return thisValue.GetResponse();

			WebResponse response = null;
			int retries = 0;
			int maxRetries = settings.Retries;
			int timeBeforeRetries = settings.TimeBeforeRetries.TotalIntMilliseconds();

			while (response == null && retries < maxRetries)
			{
				if (retries > 0 && timeBeforeRetries > 0) Thread.Sleep(timeBeforeRetries);

				try
				{
					response = thisValue.GetResponse();
				}
				catch (Exception ex)
				{
					retries++;
					if (retries >= maxRetries) settings.OnError?.Invoke(ex);
				}
			}

			return response;
		}

		public static async Task<WebResponse> GetResponseAsync([NotNull] this WebRequest thisValue, IOResponseSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested) return null;
			if (settings == null) return await thisValue.GetResponseAsync();

			WebResponse response = null;
			int retries = 0;
			int maxRetries = settings.Retries;
			int timeBeforeRetries = settings.TimeBeforeRetries.TotalIntMilliseconds();

			while (!token.IsCancellationRequested && response == null && retries < maxRetries)
			{
				if (retries > 0 && timeBeforeRetries > 0) Thread.Sleep(timeBeforeRetries);

				try
				{
					response = await thisValue.GetResponseAsync().ConfigureAwait();
				}
				catch (Exception ex)
				{
					retries++;
					if (retries >= maxRetries) settings.OnError?.Invoke(ex);
				}
			}

			return response;
		}

		public static void CopyHeaders([NotNull] this WebRequest thisValue, [NotNull] NameValueCollection collection)
		{
			if (!collection.HasKeys()) return;

			WebHeaderCollection headers = thisValue.Headers;

			if (!(thisValue is HttpWebRequest httpWebRequest))
			{
				foreach (string key in collection.Keys)
					headers[key] = collection[key];

				return;
			}

			Type type = typeof(HttpWebRequest);

			foreach (string key in collection.Keys)
			{
				string normalizedKey = key.ToPascalCase();
				PropertyInfo property = __headerSet.GetOrAdd(normalizedKey, k => type.GetProperty(k, Constants.BF_PUBLIC_INSTANCE));
				
				if (property == null)
				{
					headers[key] = collection[key];
					continue;
				}

				string value = collection[key];
				if (string.IsNullOrEmpty(value)) continue;

				try
				{
					property.SetValue(httpWebRequest, value);
				}
				catch
				{
					// ignored
				}
			}
		}

		public static void CopyHeaders<TValue>([NotNull] this WebRequest thisValue, [NotNull] IEnumerable<KeyValuePair<string, TValue>> enumerable)
		{
			WebHeaderCollection headers = thisValue.Headers;

			if (!(thisValue is HttpWebRequest httpWebRequest))
			{
				foreach (KeyValuePair<string, TValue> pair in enumerable)
				{
					string value = Convert.ToString(pair.Value);
					if (string.IsNullOrEmpty(value)) continue;
					headers[pair.Key] = value;
				}

				return;
			}

			foreach (KeyValuePair<string, TValue> pair in enumerable)
			{
				string value = Convert.ToString(pair.Value);
				if (string.IsNullOrEmpty(value)) continue;

				if (!__headerSet.TryGetValue(pair.Key, out PropertyInfo property))
				{
					headers[pair.Key] = value;
					continue;
				}

				property.SetValue(httpWebRequest, value);
			}
		}

		public static void CopyHeaders([NotNull] this WebRequest thisValue, [NotNull] Hashtable hashtable)
		{
			if (hashtable.Count == 0) return;

			WebHeaderCollection headers = thisValue.Headers;

			if (!(thisValue is HttpWebRequest httpWebRequest))
			{
				foreach (DictionaryEntry entry in hashtable)
				{
					string value = Convert.ToString(entry.Value);
					if (string.IsNullOrEmpty(value)) continue;
					headers[(string)entry.Key] = value;
				}

				return;
			}

			foreach (DictionaryEntry entry in hashtable)
			{
				string value = Convert.ToString(entry.Value);
				if (string.IsNullOrEmpty(value)) continue;

				string key = (string)entry.Key;

				if (!__headerSet.TryGetValue(key, out PropertyInfo property))
				{
					headers[key] = value;
					continue;
				}

				property.SetValue(httpWebRequest, value);
			}
		}
	}
}