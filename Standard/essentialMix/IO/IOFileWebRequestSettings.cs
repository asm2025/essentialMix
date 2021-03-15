using System;

namespace essentialMix.IO
{
	public class IOFileWebRequestSettings : IORequestSettings
	{
		/// <inheritdoc />
		public IOFileWebRequestSettings()
		{
		}

		/// <inheritdoc />
		public IOFileWebRequestSettings(IOSettings settings) 
			: base(settings)
		{
			switch (settings)
			{
				case IOFileWebRequestSettings ioFileWebRequestSettings:
					Progress = ioFileWebRequestSettings.Progress;
					break;
				case IOHttpFileWebRequestSettings ioHttpFileWebRequestSettings:
					Progress = ioHttpFileWebRequestSettings.Progress;
					break;
			}
		}

		public IProgress<int> Progress { get; set; }
	}
}