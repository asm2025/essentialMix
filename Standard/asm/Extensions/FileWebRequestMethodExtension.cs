using System.Net;
using asm.Web;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class FileWebRequestMethodExtension
	{
		[NotNull]
		public static string ToWebMethod(this FileWebRequestMethod thisValue)
		{
			switch (thisValue)
			{
				case FileWebRequestMethod.DownloadFile:
					return WebRequestMethods.File.DownloadFile;
				case FileWebRequestMethod.UploadFile:
					return WebRequestMethods.File.UploadFile;
				case FileWebRequestMethod.Post:
					return WebRequestMethods.Http.Post;
				default:
					return string.Empty;
			}
		}
	}
}