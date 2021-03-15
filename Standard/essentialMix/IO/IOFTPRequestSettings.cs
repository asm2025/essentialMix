using essentialMix.Web;

namespace essentialMix.IO
{
	public class IOFTPRequestSettings : IORequestSettings
	{
		/// <inheritdoc />
		public IOFTPRequestSettings()
		{
		}

		/// <inheritdoc />
		public IOFTPRequestSettings(IOSettings settings) 
			: base(settings)
		{
			if (!(settings is IOFTPRequestSettings ioftpRequestSettings)) return;
			Method = ioftpRequestSettings.Method;
		}

		public FtpWebRequestMethod Method { get; set; } = FtpWebRequestMethod.DownloadFile;
		public bool UsePassive { get; set; }
		public bool UseBinary { get; set; } = true;
	}
}