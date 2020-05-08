using System.Net;
using JetBrains.Annotations;

namespace asm.Web
{
	public enum FtpWebRequestMethod
	{
		Unknown,
		DownloadFile,
		UploadFile,
		UploadFileWithUniqueName,
		AppendFile,
		DeleteFile,
		FileSize,
		PrintWorkingDirectory,
		ListDirectory,
		ListDirectoryDetails,
		MakeDirectory,
		RemoveDirectory,
		RenameDirectory,
		DateTimestamp
	}

	public static class FtpWebRequestMethodHelper
	{
		public static FtpWebRequestMethod ToFtpWebRequestMethod([NotNull] string value)
		{
			switch (value.ToUpper())
			{
				case WebRequestMethods.Ftp.DownloadFile:
					return FtpWebRequestMethod.DownloadFile;
				case WebRequestMethods.Ftp.UploadFile:
					return FtpWebRequestMethod.UploadFile;
				case WebRequestMethods.Ftp.UploadFileWithUniqueName:
					return FtpWebRequestMethod.UploadFileWithUniqueName;
				case WebRequestMethods.Ftp.AppendFile:
					return FtpWebRequestMethod.AppendFile;
				case WebRequestMethods.Ftp.GetFileSize:
					return FtpWebRequestMethod.FileSize;
				case WebRequestMethods.Ftp.PrintWorkingDirectory:
					return FtpWebRequestMethod.PrintWorkingDirectory;
				case WebRequestMethods.Ftp.ListDirectory:
					return FtpWebRequestMethod.ListDirectory;
				case WebRequestMethods.Ftp.ListDirectoryDetails:
					return FtpWebRequestMethod.ListDirectoryDetails;
				case WebRequestMethods.Ftp.MakeDirectory:
					return FtpWebRequestMethod.MakeDirectory;
				case WebRequestMethods.Ftp.RemoveDirectory:
					return FtpWebRequestMethod.RemoveDirectory;
				case WebRequestMethods.Ftp.Rename:
					return FtpWebRequestMethod.RenameDirectory;
				case WebRequestMethods.Ftp.GetDateTimestamp:
					return FtpWebRequestMethod.DateTimestamp;
				default:
					return FtpWebRequestMethod.Unknown;
			}
		}
	}

	public static class FtpWebRequestMethodExtension
	{
		[NotNull]
		public static string ToWebMethod(this FtpWebRequestMethod thisValue)
		{
			switch (thisValue)
			{
				case FtpWebRequestMethod.DownloadFile:
					return WebRequestMethods.Ftp.DownloadFile;
				case FtpWebRequestMethod.UploadFile:
					return WebRequestMethods.Ftp.UploadFile;
				case FtpWebRequestMethod.UploadFileWithUniqueName:
					return WebRequestMethods.Ftp.UploadFileWithUniqueName;
				case FtpWebRequestMethod.AppendFile:
					return WebRequestMethods.Ftp.AppendFile;
				case FtpWebRequestMethod.DeleteFile:
					return WebRequestMethods.Ftp.DeleteFile;
				case FtpWebRequestMethod.FileSize:
					return WebRequestMethods.Ftp.GetFileSize;
				case FtpWebRequestMethod.PrintWorkingDirectory:
					return WebRequestMethods.Ftp.PrintWorkingDirectory;
				case FtpWebRequestMethod.ListDirectory:
					return WebRequestMethods.Ftp.ListDirectory;
				case FtpWebRequestMethod.ListDirectoryDetails:
					return WebRequestMethods.Ftp.ListDirectoryDetails;
				case FtpWebRequestMethod.MakeDirectory:
					return WebRequestMethods.Ftp.MakeDirectory;
				case FtpWebRequestMethod.RemoveDirectory:
					return WebRequestMethods.Ftp.RemoveDirectory;
				case FtpWebRequestMethod.RenameDirectory:
					return WebRequestMethods.Ftp.Rename;
				case FtpWebRequestMethod.DateTimestamp:
					return WebRequestMethods.Ftp.GetDateTimestamp;
				default:
					return string.Empty;
			}
		}
	}
}