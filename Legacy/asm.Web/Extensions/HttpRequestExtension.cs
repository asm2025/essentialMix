using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using JetBrains.Annotations;
using asm.Helpers;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class HttpRequestExtension
	{
		public static string GetIPAddress([NotNull] this HttpRequest thisValue)
		{
			string ipStr = thisValue.Headers["HTTP_CF_CONNECTING_IP"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.ServerVariables["HTTP_X_CLUSTER_CLIENT_IP"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.ServerVariables["REMOTE_ADDR"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.UserHostAddress;

			if (!string.IsNullOrEmpty(ipStr))
			{
				int n = ipStr.IndexOf(',');
				if (n > -1) ipStr = n > 0 ? ipStr.Substring(0, n) : null;

				if (ipStr == "::1" || ipStr == "127.0.0.1")
				{
					IPAddress ip = IPAddressHelper.GetLocalIP();
					if (ip != null) ipStr = ip.ToString();
				}
			}

			if (string.IsNullOrEmpty(ipStr)) ipStr = null;
			return ipStr;
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers.UnlockAndAdd(name, new ArrayList { value });
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] params (string, string)[] values)
		{
			thisValue.Headers.UnlockAndAdd(values.Select(tuple => new KeyValuePair<string, object>(tuple.Item1, new ArrayList
																												{
																													tuple.Item2
																												})).ToArray());
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] IReadOnlyCollection<(string, object)> values)
		{
			thisValue.Headers.UnlockAndAdd(values.Select(tuple => new KeyValuePair<string, object>(tuple.Item1, new ArrayList
																												{
																													tuple.Item2
																												})).ToArray());
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] IReadOnlyCollection<KeyValuePair<string, object>> values)
		{
			thisValue.Headers.UnlockAndAdd(values);
		}

		public static void RemoveHeader([NotNull] this HttpRequest thisValue, [NotNull] params string[] names)
		{
			thisValue.Headers.UnlockAndRemove(names);
		}
	}
}