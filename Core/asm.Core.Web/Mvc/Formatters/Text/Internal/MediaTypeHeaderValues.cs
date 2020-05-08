using asm.Web;
using Microsoft.Net.Http.Headers;

namespace asm.Core.Web.Mvc.Formatters.Text.Internal
{
	internal static class MediaTypeHeaderValues
	{
		public static readonly MediaTypeHeaderValue TextPlain = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Plain).CopyAsReadOnly();
	}
}