using System.Collections.Generic;
using System.Net.Http.Headers;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Web;

namespace essentialMix.IO;

public class IOHttpRequestSettings : IORequestSettings
{
	private string _userAgent = UriHelper.RandomUserAgent();
	private IList<MediaTypeWithQualityHeaderValue> _accept;

	/// <inheritdoc />
	public IOHttpRequestSettings()
	{
	}

	/// <inheritdoc />
	public IOHttpRequestSettings(IOSettings settings) 
		: base(settings)
	{
		if (settings is not IOHttpRequestSettings ioHttpRequestSettings) return;
		Method = ioHttpRequestSettings.Method;
		AllowAutoRedirect = ioHttpRequestSettings.AllowAutoRedirect;
		AllowWriteStreamBuffering = ioHttpRequestSettings.AllowWriteStreamBuffering;
		Accept = ioHttpRequestSettings.Accept;
	}

	public HttpWebRequestMethod Method { get; set; } = HttpWebRequestMethod.Get;
	public bool AllowAutoRedirect { get; set; } = true;
	public bool AllowWriteStreamBuffering { get; set; }

	[NotNull]
	public IList<MediaTypeWithQualityHeaderValue> Accept
	{
		get => _accept ??= new List<MediaTypeWithQualityHeaderValue>(); 
		set => _accept = value;
	}

	public string UserAgent
	{
		get => _userAgent;
		set
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) value = UriHelper.RandomUserAgent();
			_userAgent = value;
		}
	}
}