using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.IO;
using essentialMix.Web;

namespace essentialMix.Extensions
{
	public static class WebResponseExtension
	{
		public static Stream GetStream([NotNull] this WebResponse thisValue)
		{
			Stream stream = thisValue.GetResponseStream();
			if (stream == null) return null;

			if (!(thisValue is HttpWebResponse httpWebResponse)) return stream;

			if (httpWebResponse.ContentEncoding.IndexOf("gzip", StringComparison.InvariantCultureIgnoreCase) > -1) stream = new GZipStream(stream, CompressionMode.Decompress);
			else if (httpWebResponse.ContentEncoding.IndexOf("deflate", StringComparison.InvariantCultureIgnoreCase) > -1) stream = new DeflateStream(stream, CompressionMode.Decompress);

			return stream;
		}

		public static bool Read([NotNull] this WebResponse thisValue, [NotNull] IOSettings settings)
		{
			IIOOnRead ioOnRead = settings as IIOOnRead ?? throw new ArgumentException("Argument must provide an OnRead implementation.", nameof(settings));
			Stream stream = null;
			StreamReader reader = null;

			try
			{
				stream = GetStream(thisValue);
				if (stream == null) return false;
				reader = new StreamReader(stream, settings.Encoding, true);
				int length;
				char[] chars = new char[settings.BufferSize];

				do
				{
					length = reader.Read(chars);
				}
				while (length > 0 && ioOnRead.OnRead(chars, length));

				return true;
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				settings.OnError(ex);
				return false;
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
				ObjectHelper.Dispose(ref stream);
			}
		}

		public static async Task<bool> ReadAsync([NotNull] this WebResponse thisValue, [NotNull] IOSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			IIOOnRead ioOnRead = settings as IIOOnRead ?? throw new ArgumentException("Argument must provide an OnRead implementation.", nameof(settings));
			Stream stream = null;
			StreamReader reader = null;

			try
			{
				stream = GetStream(thisValue);
				token.ThrowIfCancellationRequested();
				if (stream == null) return false;
				reader = new StreamReader(stream, settings.Encoding, true);

				int length;
				char[] chars = new char[settings.BufferSize];

				do
				{
					length = await reader.ReadAsync(chars).ConfigureAwait();
				}
				while (!token.IsCancellationRequested && length > 0 && ioOnRead.OnRead(chars, length));

				token.ThrowIfCancellationRequested();
				return true;
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				settings.OnError(ex);
				return false;
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
				ObjectHelper.Dispose(ref stream);
			}
		}

		public static string ReadToEnd([NotNull] this WebResponse thisValue, IOSettings settings = null)
		{
			settings ??= new IOSettings();

			Stream stream = null;
			StreamReader reader = null;
			string result;

			try
			{
				stream = GetStream(thisValue);
				if (stream == null) return null;
				reader = new StreamReader(stream, settings.Encoding, true, settings.BufferSize);
				result = reader.ReadToEnd();
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				result = null;
				settings.OnError(ex);
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
				ObjectHelper.Dispose(ref stream);
			}

			return result;
		}

		public static async Task<string> ReadToEndAsync([NotNull] this WebResponse thisValue, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			settings ??= new IOSettings();

			Stream stream = null;
			StreamReader reader = null;
			string result;

			try
			{
				stream = GetStream(thisValue);
				token.ThrowIfCancellationRequested();
				if (stream == null) return null;
				reader = new StreamReader(stream, settings.Encoding, true, settings.BufferSize);
				result = await reader.ReadToEndAsync().ConfigureAwait();
				token.ThrowIfCancellationRequested();
			}
			catch (Exception ex) when (settings.OnError != null)
			{
				result = null;
				settings.OnError(ex);
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
				ObjectHelper.Dispose(ref stream);
			}

			return result;
		}

		public static string GetTitle([NotNull] this WebResponse thisValue, IOSettings settings = null)
		{
			string result = null;
			StringBuilder sb = new StringBuilder();
			IOReadSettings readSettings = new IOReadSettings(settings, (bf, len) =>
			{
				sb.Append(bf);

				string contents = sb.ToString();
				Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

				if (m.Success)
				{
					// we found a <title></title> match =]
					result = m.Groups[1].Value;
					return false;
				}

				// reached end of head-block; no title found =[
				return !contents.Contains("</head>", StringComparison.OrdinalIgnoreCase);
			});
			return Read(thisValue, readSettings) ? result : null;
		}

		public static async Task<string> GetTitleAsync([NotNull] this WebResponse thisValue, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			string result = null;
			StringBuilder sb = new StringBuilder();
			IOReadSettings readSettings = new IOReadSettings(settings, (bf, len) =>
			{
				token.ThrowIfCancellationRequested();
				sb.Append(bf);

				string contents = sb.ToString();
				Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

				if (m.Success)
				{
					// we found a <title></title> match =]
					result = m.Groups[1].Value;
					return false;
				}

				// reached end of head-block; no title found =[
				return !contents.Contains("</head>", StringComparison.OrdinalIgnoreCase);
			});
			if (!await ReadAsync(thisValue, readSettings, token).ConfigureAwait()) return null;
			token.ThrowIfCancellationRequested();
			return result;
		}

		public static (string Title, string Buffer) Peek([NotNull] this WebResponse thisValue, IOSettings settings = null)
		{
			bool titleFound = false;
			bool bufferFilled = false;
			string title = null;
			int bufferSize = settings?.BufferSize ?? IOSettings.BUFFER_DEFAULT;
			StringBuilder bufferFetch = new StringBuilder(bufferSize);
			StringBuilder sb = new StringBuilder(bufferSize);
			IOReadSettings readSettings = new IOReadSettings(settings, (buf, len) =>
			{
				sb.Append(buf);

				string contents = sb.ToString();

				if (!bufferFilled)
				{
					bufferFetch.Append(buf);
					bufferFilled = bufferFetch.Length >= bufferSize;
				}

				if (!titleFound)
				{
					Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

					if (m.Success)
					{
						// we found a <title></title> match =]
						title = m.Groups[1].Value;
						titleFound = true;
					}
					else
					{
						// reached end of head-block; no title found =[
						titleFound = contents.Contains("</head>", StringComparison.OrdinalIgnoreCase);
					}
				}

				return !titleFound || !bufferFilled;
			});

			return Read(thisValue, readSettings) 
				? (title, bufferFetch.ToString()) 
				: (null, null);
		}

		public static async Task<(string Title, string Buffer)> PeekAsync([NotNull] this WebResponse thisValue, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			bool titleFound = false;
			bool bufferFilled = false;
			string title = null;
			int bufferSize = settings?.BufferSize ?? IOSettings.BUFFER_DEFAULT;
			StringBuilder bufferFetch = new StringBuilder(bufferSize);
			StringBuilder sb = new StringBuilder(bufferSize);
			IOReadSettings readSettings = new IOReadSettings(settings, (buf, len) =>
			{
				token.ThrowIfCancellationRequested();
				sb.Append(buf);

				string contents = sb.ToString();

				if (!bufferFilled)
				{
					bufferFetch.Append(buf);
					bufferFilled = bufferFetch.Length >= bufferSize;
				}

				if (!titleFound)
				{
					Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

					if (m.Success)
					{
						// we found a <title></title> match =]
						title = m.Groups[1].Value;
						titleFound = true;
					}
					else
					{
						// reached end of head-block; no title found =[
						titleFound = contents.Contains("</head>", StringComparison.OrdinalIgnoreCase);
					}
				}

				token.ThrowIfCancellationRequested();
				return !titleFound || !bufferFilled;
			});

			if (!await ReadAsync(thisValue, readSettings, token).ConfigureAwait()) return (null, null);
			token.ThrowIfCancellationRequested();
			return (title, bufferFetch.ToString());
		}

		[NotNull]
		public static UrlSearchResult Search([NotNull] this WebResponse thisValue, UrlSearchFlags flags, IOSettings settings = null) { return Search(thisValue, null, flags, settings); }

		[NotNull]
		public static UrlSearchResult Search([NotNull] this WebResponse thisValue, string searchFor, UrlSearchFlags flags, IOSettings settings = null)
		{
			bool hasTitleFlag = flags.HasFlag(UrlSearchFlags.Title);
			bool hasBufferFlag = flags.HasFlag(UrlSearchFlags.Buffer);
			bool hasSearchFlag = !string.IsNullOrEmpty(searchFor);

			UrlSearchResult result = new UrlSearchResult();

			try
			{
				Func<char[], int, bool> onRead;

				if (hasTitleFlag || hasBufferFlag || hasSearchFlag)
				{
					StringBuilder sb = new StringBuilder();
					StringComparison comparison = flags.HasFlag(UrlSearchFlags.IgnoreCase)
							? StringComparison.InvariantCultureIgnoreCase
							: StringComparison.InvariantCulture;
					onRead = (c, i) =>
					{
						if (result.Status == UrlSearchStatus.Unknown) result.Status = UrlSearchStatus.Success;
						sb.Append(c);

						if (hasBufferFlag)
						{
							result.Buffer = sb.ToString();
							hasBufferFlag = false;
						}

						if (!hasTitleFlag && !hasSearchFlag) return false;

						string contents = sb.ToString();

						if (hasTitleFlag)
						{
							Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

							if (m.Success)
							{
								result.Title = m.Groups[1].Value;
								hasTitleFlag = false;
							}

							if (hasTitleFlag && contents.Contains("</head>", StringComparison.OrdinalIgnoreCase)) hasTitleFlag = false;
						}

						if (hasSearchFlag)
						{
							// ReSharper disable once AssignNullToNotNullAttribute
							if (contents.Contains(searchFor, comparison))
							{
								result.Status = UrlSearchStatus.Found;
								hasSearchFlag = false;
							}
						}

						return hasTitleFlag || hasSearchFlag || hasBufferFlag;
					};
				}
				else
				{
					onRead = (c, i) =>
					{
						if (result.Status == UrlSearchStatus.Unknown) result.Status = UrlSearchStatus.Success;
						return false;
					};
				}

				IOReadSettings readSettings = new IOReadSettings(settings, onRead);
				if (!Read(thisValue, readSettings)) result.Status = UrlSearchStatus.Failed;
			}
			catch (WebException wex)
			{
				result.Status = UrlSearchStatus.Unauthorized;
				result.Exception = wex;
			}
			catch (Exception ex)
			{
				result.Status = UrlSearchStatus.Error;
				result.Exception = ex;
			}

			return result;
		}

		[NotNull]
		public static async Task<UrlSearchResult> SearchAsync([NotNull] this WebResponse thisValue, UrlSearchFlags flags, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			return await SearchAsync(thisValue, null, flags, settings, token).ConfigureAwait();
		}

		[NotNull]
		[ItemNotNull]
		public static async Task<UrlSearchResult> SearchAsync([NotNull] this WebResponse thisValue, string searchFor, UrlSearchFlags flags, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			bool hasTitleFlag = flags.HasFlag(UrlSearchFlags.Title);
			bool hasBufferFlag = flags.HasFlag(UrlSearchFlags.Buffer);
			bool hasSearchFlag = !string.IsNullOrEmpty(searchFor);

			UrlSearchResult result = new UrlSearchResult();

			try
			{
				Func<char[], int, bool> onRead;

				if (hasTitleFlag || hasBufferFlag || hasSearchFlag)
				{
					StringBuilder sb = new StringBuilder();
					StringComparison comparison = flags.HasFlag(UrlSearchFlags.IgnoreCase)
							? StringComparison.InvariantCultureIgnoreCase
							: StringComparison.InvariantCulture;
					onRead = (c, i) =>
					{
						if (result.Status == UrlSearchStatus.Unknown) result.Status = UrlSearchStatus.Success;
						sb.Append(c);

						if (hasBufferFlag)
						{
							result.Buffer = sb.ToString();
							hasBufferFlag = false;
						}

						token.ThrowIfCancellationRequested();
						if (!hasTitleFlag && !hasSearchFlag) return false;

						string contents = sb.ToString();

						if (hasTitleFlag)
						{
							Match m = WebResponseHelper.TitleCheckExpression.Match(contents);

							if (m.Success)
							{
								result.Title = m.Groups[1].Value;
								hasTitleFlag = false;
							}

							if (hasTitleFlag && contents.Contains("</head>", StringComparison.OrdinalIgnoreCase)) hasTitleFlag = false;
						}

						if (hasSearchFlag)
						{
							// ReSharper disable once AssignNullToNotNullAttribute
							if (contents.Contains(searchFor, comparison))
							{
								result.Status = UrlSearchStatus.Found;
								hasSearchFlag = false;
							}
						}

						token.ThrowIfCancellationRequested();
						return hasTitleFlag || hasSearchFlag || hasBufferFlag;
					};
				}
				else
				{
					onRead = (c, i) =>
					{
						token.ThrowIfCancellationRequested();
						if (result.Status == UrlSearchStatus.Unknown) result.Status = UrlSearchStatus.Success;
						return false;
					};
				}

				IOReadSettings readSettings = new IOReadSettings(settings, onRead);
				if (! await ReadAsync(thisValue, readSettings, token).ConfigureAwait()) result.Status = UrlSearchStatus.Failed;
			}
			catch (WebException wex)
			{
				result.Status = UrlSearchStatus.Unauthorized;
				result.Exception = wex;
			}
			catch (Exception ex)
			{
				result.Status = UrlSearchStatus.Error;
				result.Exception = ex;
			}

			return result;
		}

		public static string GetString([NotNull] this WebResponse thisValue, IOSettings settings = null)
		{
			bool bufferFilled = false;
			int bufferSize = settings?.BufferSize ?? IOSettings.BUFFER_DEFAULT;
			StringBuilder sb = new StringBuilder(bufferSize);
			IOReadSettings readSettings = new IOReadSettings(settings, (buf, len) =>
			{
				if (!bufferFilled)
				{
					sb.Append(buf);
					bufferFilled = sb.Length >= bufferSize;
				}

				return !bufferFilled;
			});
			return Read(thisValue, readSettings) && sb.Length > 0
				? sb.ToString()
				: null;
		}

		public static async Task<string> GetStringAsync([NotNull] this WebResponse thisValue, IOSettings settings = null, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			bool bufferFilled = false;
			int bufferSize = settings?.BufferSize ?? IOSettings.BUFFER_DEFAULT;
			StringBuilder sb = new StringBuilder(bufferSize);
			IOReadSettings readSettings = new IOReadSettings(settings, (buf, len) =>
			{
				if (!bufferFilled)
				{
					sb.Append(buf);
					bufferFilled = sb.Length >= bufferSize;
				}

				token.ThrowIfCancellationRequested();
				return !bufferFilled;
			});
			if (!await ReadAsync(thisValue, readSettings, token).ConfigureAwait()) return null;
			token.ThrowIfCancellationRequested();
			return sb.ToString();
		}
	}
}