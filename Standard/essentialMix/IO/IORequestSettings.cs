using System.Net;
using System.Net.Cache;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.IO
{
	public class IORequestSettings : IOResponseSettings
	{
		private int _timeout = WebRequestHelper.TIMEOUT_DEF;

		/// <inheritdoc />
		public IORequestSettings()
		{
		}

		/// <inheritdoc />
		public IORequestSettings(IOSettings settings) 
			: base(settings)
		{
			if (!(settings is IORequestSettings iioRequestSettings)) return;
			Timeout = iioRequestSettings.Timeout;
			Credentials = iioRequestSettings.Credentials;
		}

		public int Timeout
		{
			get => _timeout;
			set => _timeout = value.NotBelow(TimeSpanHelper.INFINITE);
		}

		public ICredentials Credentials { get; set; }

		public IWebProxy Proxy { get; set; }

		[NotNull]
		public HttpRequestCachePolicy CachePolicy { get; set; } = WebRequestHelper.CachePolicy;
	}
}