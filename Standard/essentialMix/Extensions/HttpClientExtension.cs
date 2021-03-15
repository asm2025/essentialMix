using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using JetBrains.Annotations;
using essentialMix.IO;

namespace essentialMix.Extensions
{
	public static class HttpClientExtension
	{
		[NotNull]
		public static T Configure<T>([NotNull] this T thisValue, Uri baseAddress = null, IOHttpRequestSettings settings = null)
			where T : HttpClient
		{
			if (baseAddress != null) thisValue.BaseAddress = baseAddress;

			HttpRequestHeaders headers = thisValue.DefaultRequestHeaders;
			headers.CacheControl = new CacheControlHeaderValue
			{
				NoCache = true
			};

			if (settings == null) return thisValue;
			if (settings.Timeout > 0) thisValue.Timeout = TimeSpan.FromMilliseconds(settings.Timeout);

			if (settings.Accept.Count > 0)
			{
				headers.Accept.Clear();

				foreach (MediaTypeWithQualityHeaderValue headerValue in settings.Accept)
				{
					headers.Accept.Add(headerValue);
				}
			}

			if (settings.Encoding != null)
			{
				headers.AcceptEncoding.Clear();
				headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(settings.Encoding.WebName));
			}

			return thisValue;
		}

		[NotNull]
		public static IOHttpRequestSettings GetRequestSettings([NotNull] this HttpClient thisValue)
		{
			HttpRequestHeaders headers = thisValue.DefaultRequestHeaders;
			IOHttpRequestSettings settings = new IOHttpRequestSettings
			{
				Timeout = thisValue.Timeout.TotalIntMilliseconds(),
				Accept = thisValue.DefaultRequestHeaders.Accept.ToList()
			};

			try
			{
				string encoding = headers.AcceptEncoding.FirstOrDefault()?.Value;
				settings.Encoding = encoding == null
										? Encoding.UTF8
										: Encoding.GetEncoding(encoding);
			}
			catch
			{
				settings.Encoding = Encoding.UTF8;
			}

			return settings;
		}
	}
}