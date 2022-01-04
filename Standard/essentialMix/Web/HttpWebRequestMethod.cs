using System;
using System.Net;
using System.Net.Http;
using JetBrains.Annotations;

namespace essentialMix.Web;

public enum HttpWebRequestMethod
{
	Unknown,
	Get,
	Connect,
	Head,
	Put,
	Post,
	Delete,
	Options,
	Trace,
	Patch,
	MakeCollection
}

internal static class MethodConstants
{
	public const string DELETE = "DELETE";
	public const string OPTIONS = "OPTIONS";
	public const string TRACE = "TRACE";
	public const string PATCH = "PATCH";
}

public static class HttpWebRequestMethodHelper
{
	public static HttpWebRequestMethod ToHttpWebRequestMethod([NotNull] string value)
	{
		switch (value.ToUpper())
		{
			case WebRequestMethods.Http.Get:
				return HttpWebRequestMethod.Get;
			case WebRequestMethods.Http.Connect:
				return HttpWebRequestMethod.Connect;
			case WebRequestMethods.Http.Head:
				return HttpWebRequestMethod.Head;
			case WebRequestMethods.Http.Put:
				return HttpWebRequestMethod.Put;
			case WebRequestMethods.Http.Post:
				return HttpWebRequestMethod.Post;
			case MethodConstants.DELETE:
				return HttpWebRequestMethod.Delete;
			case MethodConstants.OPTIONS:
				return HttpWebRequestMethod.Options;
			case MethodConstants.TRACE:
				return HttpWebRequestMethod.Trace;
			case MethodConstants.PATCH:
				return HttpWebRequestMethod.Patch;
			case WebRequestMethods.Http.MkCol:
				return HttpWebRequestMethod.MakeCollection;
			default:
				return HttpWebRequestMethod.Unknown;
		}
	}
}

public static class HttpWebRequestMethodExtension
{
	[NotNull]
	public static string ToWebMethod(this HttpWebRequestMethod thisValue)
	{
		switch (thisValue)
		{
			case HttpWebRequestMethod.Get:
				return WebRequestMethods.Http.Get;
			case HttpWebRequestMethod.Connect:
				return WebRequestMethods.Http.Connect;
			case HttpWebRequestMethod.Head:
				return WebRequestMethods.Http.Head;
			case HttpWebRequestMethod.Put:
				return WebRequestMethods.Http.Put;
			case HttpWebRequestMethod.Post:
				return WebRequestMethods.Http.Post;
			case HttpWebRequestMethod.Delete:
				return MethodConstants.DELETE;
			case HttpWebRequestMethod.Options:
				return MethodConstants.OPTIONS;
			case HttpWebRequestMethod.Trace:
				return MethodConstants.TRACE;
			case HttpWebRequestMethod.Patch:
				return MethodConstants.PATCH;
			case HttpWebRequestMethod.MakeCollection:
				return WebRequestMethods.Http.MkCol;
			default:
				return string.Empty;
		}
	}
}

public static class HttpMethodExtension
{
	private static readonly HttpMethod __connectMethod = new HttpMethod(WebRequestMethods.Http.Connect);
	private static readonly HttpMethod __patchMethod = new HttpMethod(MethodConstants.PATCH);
	private static readonly HttpMethod __mkColMethod = new HttpMethod(WebRequestMethods.Http.MkCol);

	public static HttpMethod ToHttpMethod(this HttpWebRequestMethod thisValue)
	{
		switch (thisValue)
		{
			case HttpWebRequestMethod.Get:
				return HttpMethod.Get;
			case HttpWebRequestMethod.Connect:
				return __connectMethod;
			case HttpWebRequestMethod.Head:
				return HttpMethod.Head;
			case HttpWebRequestMethod.Put:
				return HttpMethod.Put;
			case HttpWebRequestMethod.Post:
				return HttpMethod.Post;
			case HttpWebRequestMethod.Delete:
				return HttpMethod.Delete;
			case HttpWebRequestMethod.Options:
				return HttpMethod.Options;
			case HttpWebRequestMethod.Trace:
				return HttpMethod.Trace;
			case HttpWebRequestMethod.Patch:
				return __patchMethod;
			case HttpWebRequestMethod.MakeCollection:
				return __mkColMethod;
			default:
				throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null);
		}
	}
}