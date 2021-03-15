using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace essentialMix.Extensions
{
	public static class IConfigurationExtension
	{
		public static T GetAnyValue<T>([NotNull] this IConfiguration thisValue, [NotNull] params string[] keys) { return GetAnyValue(thisValue, default(T), keys); }
		public static T GetAnyValue<T>([NotNull] this IConfiguration thisValue, T defaultValue, [NotNull] params string[] keys)
		{
			string result = keys.SkipNullOrEmpty()
								.Select(key => thisValue.GetSection(key)?.Value)
								.FirstNotNullOrWhiteSpaceOrDefault();
			return result.To(defaultValue);
		}
	}
}