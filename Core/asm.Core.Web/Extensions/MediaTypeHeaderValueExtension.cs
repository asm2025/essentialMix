using System.IO;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class MediaTypeHeaderValueExtension
	{
		/// <summary>
		/// Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
		/// The spec says 70 characters is a reasonable limit.
		/// The length should be validated.
		/// </summary>
		public static string GetBoundary(this MediaTypeHeaderValue thisValue, int lengthLimit = 0)
		{
			if (thisValue == null || !thisValue.Boundary.HasValue) return null;

			string boundary = thisValue.Boundary.Value?.Trim()
									.UnQuote()
									.ToNullIfEmpty() ?? throw new InvalidDataException("Missing content-type boundary.");
			if (lengthLimit > 0 && boundary.Length > lengthLimit) throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
			return boundary;
		}
	}
}