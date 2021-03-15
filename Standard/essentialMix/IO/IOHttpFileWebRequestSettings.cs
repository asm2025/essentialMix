using System;

namespace essentialMix.IO
{
	public class IOHttpFileWebRequestSettings : IOHttpRequestSettings
	{
		/// <inheritdoc />
		public IOHttpFileWebRequestSettings()
		{
		}

		/// <inheritdoc />
		public IOHttpFileWebRequestSettings(IOSettings settings) 
			: base(settings)
		{
			switch (settings)
			{
				case IOHttpFileWebRequestSettings ioHttpFileWebRequestSettings:
					Progress = ioHttpFileWebRequestSettings.Progress;
					break;
				case IOFileWebRequestSettings ioFileWebRequestSettings:
					Progress = ioFileWebRequestSettings.Progress;
					break;
			}
		}

		public IProgress<int> Progress { get; set; }
	}
}