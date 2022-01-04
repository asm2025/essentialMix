using essentialMix.Web;
using Microsoft.Net.Http.Headers;

namespace essentialMix.Core.Web.Mvc.Formatters.Text.Internal;

internal static class MediaTypeHeaderValues
{
	public static readonly MediaTypeHeaderValue TextPlain = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Plain).CopyAsReadOnly();
}