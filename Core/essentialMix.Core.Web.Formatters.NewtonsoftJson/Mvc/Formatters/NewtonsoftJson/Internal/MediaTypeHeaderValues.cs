using essentialMix.Web;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.NewtonsoftJson
{
	internal static class MediaTypeHeaderValues
	{
		public static readonly MediaTypeHeaderValue ApplicationJson = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json).CopyAsReadOnly();
		public static readonly MediaTypeHeaderValue TextJson = MediaTypeHeaderValue.Parse(MediaTypeNames.Text.Json).CopyAsReadOnly();
		public static readonly MediaTypeHeaderValue ApplicationJsonPatch = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.JsonPatchJson).CopyAsReadOnly();
		public static readonly MediaTypeHeaderValue ApplicationAnyJsonSyntax = MediaTypeHeaderValue.Parse("application/*+json").CopyAsReadOnly();
	}
}