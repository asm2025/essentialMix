using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class MultipartSectionExtension
{
	public static Encoding GetEncoding([NotNull] this MultipartSection thisValue)
	{
		bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(thisValue.ContentType, out MediaTypeHeaderValue mediaType);
		// UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
		return !hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType.Encoding)
					? Encoding.UTF8
					: mediaType.Encoding;
	}
}