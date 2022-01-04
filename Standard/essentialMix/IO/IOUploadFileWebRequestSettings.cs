using System;
using essentialMix.Web;

namespace essentialMix.IO;

public class IOUploadFileWebRequestSettings : IOFileWebRequestSettings
{
	private FileWebRequestMethod _method = FileWebRequestMethod.UploadFile;

	/// <inheritdoc />
	public IOUploadFileWebRequestSettings()
	{
	}

	/// <inheritdoc />
	public IOUploadFileWebRequestSettings(IOSettings settings) 
		: base(settings)
	{
		if (settings is not IOUploadFileWebRequestSettings ioUploadFileWebRequestSettings) return;
		Method = ioUploadFileWebRequestSettings.Method;
	}

	public FileWebRequestMethod Method
	{
		get => _method;
		set
		{
			switch (value)
			{
				case FileWebRequestMethod.UploadFile:
				case FileWebRequestMethod.Post:
					_method = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}
	}
}