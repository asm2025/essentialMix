using System.Net;
using JetBrains.Annotations;
using essentialMix.Web;

namespace essentialMix.Helpers
{
	public static class FileWebRequestMethodHelper
	{
		public static FileWebRequestMethod ToFileWebRequestMethod([NotNull] string value)
		{
			switch (value.ToUpper())
			{
				case WebRequestMethods.File.DownloadFile:
					return FileWebRequestMethod.DownloadFile;
				case WebRequestMethods.File.UploadFile:
					return FileWebRequestMethod.UploadFile;
				case WebRequestMethods.Http.Post:
					return FileWebRequestMethod.Post;
				default:
					return FileWebRequestMethod.Unknown;
			}
		}
	}
}