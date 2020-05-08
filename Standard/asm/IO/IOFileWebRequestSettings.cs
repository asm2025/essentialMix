using System;

namespace asm.IO
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
			if (!(settings is IOFileWebRequestSettings ioFileWebRequestSettings)) return;
			Progress = ioFileWebRequestSettings.Progress;
		}

		public IProgress<int> Progress { get; set; }
	}
}