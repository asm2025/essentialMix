using System.Net.Http;
using System.Reflection;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class HttpMessageInvokerExtension
	{
		private static readonly FieldInfo __handlerField = typeof(HttpMessageInvoker).GetField("_handler", Constants.BF_NON_PUBLIC_INSTANCE);

		public static HttpMessageHandler GetHandler([NotNull] this HttpMessageInvoker thisValue) { return (HttpMessageHandler)__handlerField.GetValue(thisValue); }
		public static void SetHandler([NotNull] this HttpMessageInvoker thisValue, [NotNull] HttpMessageHandler handler) { __handlerField.SetValue(thisValue, handler); }
	}
}