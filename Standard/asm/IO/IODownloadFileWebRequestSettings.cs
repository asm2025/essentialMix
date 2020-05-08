using System;
using asm.Web;

namespace asm.IO
{
	public class IODownloadFileWebRequestSettings : IOFileWebRequestSettings
	{
		private FileWebRequestMethod _method = FileWebRequestMethod.DownloadFile;

		/// <inheritdoc />
		public IODownloadFileWebRequestSettings()
		{
		}

		/// <inheritdoc />
		public IODownloadFileWebRequestSettings(IOSettings settings) 
			: base(settings)
		{
			if (!(settings is IODownloadFileWebRequestSettings ioDownloadFileWebRequestSettings)) return;
			Method = ioDownloadFileWebRequestSettings.Method;
		}

		public FileWebRequestMethod Method
		{
			get => _method;
			set
			{
				switch (value)
				{
					case FileWebRequestMethod.DownloadFile:
					case FileWebRequestMethod.Post:
						_method = value;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
			}
		}
	}
}