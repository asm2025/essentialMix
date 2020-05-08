using JetBrains.Annotations;

namespace asm.Web
{
	public static class RequestParameterNames
	{
		private const string CULTURE = "culture";
		
		private static string _culture = CULTURE;

		[NotNull]
		public static string Culture
		{
			get => _culture;
			set
			{
				value = value.Trim();
				if (string.IsNullOrEmpty(value)) value = CULTURE;
				_culture = value;
			}
		}
	}
}