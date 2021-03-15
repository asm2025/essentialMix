using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class NameValueCollectionExtension
	{
		public static bool ContainsKey([NotNull] this NameValueCollection thisValue, [NotNull] string key) { return thisValue.Get(key) != null || thisValue.AllKeys.Contains(key); }
	}
}