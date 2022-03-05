using System.Collections.Generic;
using System.Net.Http.Headers;
using essentialMix.Helpers;
using essentialMix.Web;

namespace essentialMix.IO;

public class IOHttpRequestSettings : IORequestSettings
{
	private string _userAgent = UriHelper.RandomUserAgent();

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

	public IList<MediaTypeWithQualityHeaderValue> Accept { get; set; }

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