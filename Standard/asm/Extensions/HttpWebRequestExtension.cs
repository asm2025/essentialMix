using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class HttpWebRequestExtension
	{
		public static void AddHeader([NotNull] this HttpWebRequest thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.UnlockAndAdd(name, new ArrayList { value });
		}

		public static void AddHeader([NotNull] this HttpWebRequest thisValue, [NotNull] params (string, string)[] values)
		{
			thisValue.Headers.UnlockAndAdd(values.Select(tuple => new KeyValuePair<string, object>(tuple.Item1, new ArrayList
																												{
																													tuple.Item2
																												})).ToArray());
		}

		public static void AddHeader([NotNull] this HttpWebRequest thisValue, [NotNull] IReadOnlyCollection<(string, object)> values)
		{
			thisValue.Headers.UnlockAndAdd(values.Select(tuple => new KeyValuePair<string, object>(tuple.Item1, new ArrayList
																												{
																													tuple.Item2
																												})).ToArray());
		}

		public static void AddHeader([NotNull] this HttpWebRequest thisValue, [NotNull] IReadOnlyCollection<KeyValuePair<string, object>> values)
		{
			thisValue.Headers.UnlockAndAdd(values);
		}
	}
}