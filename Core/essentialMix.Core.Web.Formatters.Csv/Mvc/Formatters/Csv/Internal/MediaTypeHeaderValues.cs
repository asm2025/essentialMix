using essentialMix.Web;
using Microsoft.Net.Http.Headers;

namespace essentialMix.Core.Web.Formatters.Csv.Mvc.Formatters.Csv.Internal
{
	internal static class MediaTypeHeaderValues
	{
		public static readonly MediaTypeHeaderValue ApplicationCsv = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Csv).CopyAsReadOnly();
		public static readonly MediaTypeHeaderValue TextCsv = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Csv).CopyAsReadOnly();
		public static readonly MediaTypeHeaderValue ApplicationAnyCsvSyntax = MediaTypeHeaderValue.Parse("application/*+csv").CopyAsReadOnly();
	}
}