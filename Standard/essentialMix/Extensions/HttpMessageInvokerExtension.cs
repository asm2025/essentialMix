using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class HttpMessageInvokerExtension
{
	private static readonly Lazy<FieldInfo> __handlerField = new Lazy<FieldInfo>(() => typeof(HttpMessageInvoker).GetField("_handler", Constants.BF_NON_PUBLIC_INSTANCE), LazyThreadSafetyMode.PublicationOnly);

	public static HttpMessageHandler GetHandler([NotNull] this HttpMessageInvoker thisValue) { return (HttpMessageHandler)__handlerField.Value.GetValue(thisValue); }
	public static void SetHandler([NotNull] this HttpMessageInvoker thisValue, [NotNull] HttpMessageHandler handler) { __handlerField.Value.SetValue(thisValue, handler); }
}