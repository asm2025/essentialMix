namespace essentialMix.IO
{
	public class IOHttpDownloadFileWebRequestSettings : IOHttpFileWebRequestSettings
	{
		/// <inheritdoc />
		public IOHttpDownloadFileWebRequestSettings()
		{
		}

		/// <inheritdoc />
		public IOHttpDownloadFileWebRequestSettings(IOSettings settings) 
			: base(settings)
		{
			if (settings is not IOHttpDownloadFileWebRequestSettings ioHttpDownloadFileWebRequestSettings) return;
			Overwrite = ioHttpDownloadFileWebRequestSettings.Overwrite;
		}

		public bool Overwrite { get; set; }
	}
}