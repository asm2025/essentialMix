using System;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ContentDispositionHeaderValueExtension
	{
		public static bool HasFormDataContentDisposition(this ContentDispositionHeaderValue thisValue)
		{
			// Content-Disposition: form-data; name="key";
			return thisValue != null
					&& thisValue.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase)
					&& StringSegment.IsNullOrEmpty(thisValue.FileName)
					&& StringSegment.IsNullOrEmpty(thisValue.FileNameStar);
		}

		public static bool HasFileContentDisposition(this ContentDispositionHeaderValue thisValue)
		{
			// Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
			return thisValue != null
					&& thisValue.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase)
					&& (!StringSegment.IsNullOrEmpty(thisValue.FileName) || !StringSegment.IsNullOrEmpty(thisValue.FileNameStar));
		}
	}
}