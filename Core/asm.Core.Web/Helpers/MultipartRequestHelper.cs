using System;

namespace asm.Core.Web.Helpers
{
	// https://github.com/aspnet/AspNetCore.Docs/blob/master/aspnetcore/mvc/models/file-uploads/sample/FileUploadSample/MultipartRequestHelper.cs
	public static class MultipartRequestHelper
	{
		public static bool IsMultipartContentType(string contentType)
		{
			return !string.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}