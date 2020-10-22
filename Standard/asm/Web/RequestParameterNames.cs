using JetBrains.Annotations;

namespace asm.Web
{
	public static class RequestParameterNames
	{
		private const string CULTURE = "culture";
		
		private static string __culture = CULTURE;

		[NotNull]
		public static string Culture
		{
			get => __culture;
			set
			{
				value = value.Trim();
				if (string.IsNullOrEmpty(value)) value = CULTURE;
				__culture = value;
			}
		}
	}
}