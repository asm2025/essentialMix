using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web
{
	public static class CookieNames
	{
		private const string CULTURE = "__culture";

		public static readonly string AntiForgeryToken = "X-XSRF-TOKEN";
	
		private static string __culture = CULTURE;

		[NotNull]
		public static string Culture
		{
			get => __culture;
			set => __culture = value.ToNullIfEmpty() ?? CULTURE;
		}
	}
}