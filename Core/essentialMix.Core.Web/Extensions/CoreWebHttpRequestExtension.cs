using System;
using System.Collections.Generic;
using System.Net;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class CoreWebHttpRequestExtension
	{
		public static string GetIPAddress([NotNull] this HttpRequest thisValue)
		{
			string ipStr = thisValue.Headers["HTTP_CF_CONNECTING_IP"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.Headers["HTTP_X_CLUSTER_CLIENT_IP"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.Headers["HTTP_X_FORWARDED_FOR"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.Headers["REMOTE_ADDR"];
			if (string.IsNullOrEmpty(ipStr)) ipStr = thisValue.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

			if (!string.IsNullOrEmpty(ipStr))
			{
				int n = ipStr.IndexOf(',');
				
				if (n > -1)
				{
					ipStr = n > 0
								? ipStr.Substring(0, n)
								: null;
				}
			}

			if (IPAddressHelper.IsLoopback(ipStr))
			{
				IPAddress ip = IPAddressHelper.GetLocalIP();
				if (ip != null) ipStr = ip.ToString();
			}

			return ipStr.ToNullIfEmpty();
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] string name, string value)
		{
			thisValue.Headers[name] = new StringValues(value);
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] params (string, string)[] values)
		{
			foreach ((string key, string value) in values)
			{
				thisValue.Headers[key] = new StringValues(value);
			}
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] IReadOnlyCollection<(string, object)> values)
		{
			foreach ((string key, object value) in values)
			{
				thisValue.Headers[key] = new StringValues(Convert.ToString(value));
			}
		}

		public static void AddHeader([NotNull] this HttpRequest thisValue, [NotNull] IReadOnlyCollection<KeyValuePair<string, object>> values)
		{
			foreach ((string key, object value) in values)
			{
				thisValue.Headers[key] = new StringValues(Convert.ToString(value));
			}
		}

		public static void RemoveHeader([NotNull] this HttpRequest thisValue, [NotNull] params string[] names)
		{
			foreach (string name in names)
			{
				if (!thisValue.Headers.ContainsKey(name)) continue;
				thisValue.Headers.Remove(name);
			}
		}
	}
}