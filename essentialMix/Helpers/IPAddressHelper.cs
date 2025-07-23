using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.IO;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class IPAddressHelper
{
	private const string STR_PUBLIC_IP = "http://icanhazip.com/"; // also "http://ipinfo.io/ip"

	private static readonly Regex __extractIP = new Regex(@"(?s)(?<ip>(?:[0-9]{1,3}\.){3}[0-9]{1,3})", RegexHelper.OPTIONS_I | RegexOptions.Singleline);
	private static readonly Regex __extractIPWithPort = new Regex(@"(?s)(?<ip>\d+\.\d+\.\d+\.\d+)(?<port>:\d{1,5})", RegexHelper.OPTIONS_I | RegexOptions.Singleline);
	private static readonly Regex __isIPv4 = new Regex(@"^\b(?<url>(?<ip>(?:[0-9]{1,3}\.){3}[0-9]{1,3})(?<port>:\d{1,5})?)\b$", RegexHelper.OPTIONS_I);
	private static readonly Regex __isIPv4Url = new Regex(@"^\b(?<url>(?<protocol>https?://)?(?<ip>(?:[0-9]{1,3}\.){3}[0-9]{1,3})(?<port>:\d{1,5})?)\b/?$", RegexHelper.OPTIONS_I);
	private static readonly Regex __isIPv4Simple = new Regex(@"^\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}-(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b$", RegexHelper.OPTIONS_I);
	private static readonly Regex __isIPv4CIDR = new Regex(@"^\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}/\d{1,2}\b$", RegexHelper.OPTIONS_I);

	public static IPAddress ExtractIP(string value)
	{
		if (string.IsNullOrEmpty(value)) return null;

		Match match = __extractIP.Match(value);
		if (!match.Success) return null;
		if (!IPAddress.TryParse(match.Groups["ip"].Value, out IPAddress result)) result = null;
		return result;
	}

	public static string ExtractIPString(string value)
	{
		IPAddress ip = ExtractIPWithPort(value);
		return ip?.ToString();
	}

	public static IPAddress ExtractIPWithPort(string value)
	{
		if (string.IsNullOrEmpty(value)) return null;

		Match match = __extractIPWithPort.Match(value);
		if (!match.Success) return null;

		if (!IPAddress.TryParse(match.Groups["ip"].Value + match.Groups["port"].Value, out IPAddress result)) result = null;
		return result;
	}

	public static IPAddress ExtractIPWithPort(string value, out ushort port)
	{
		port = 0;
		if (string.IsNullOrEmpty(value)) return null;

		Match match = __extractIPWithPort.Match(value);
		if (!match.Success) return null;


		if (!IPAddress.TryParse(match.Groups["ip"].Value + match.Groups["port"].Value, out IPAddress result)) result = null;
		else port = ushort.Parse(match.Groups["port"].Value);

		return result;
	}

	[NotNull]
	public static Match MatchIPv4(string value)
	{
		return string.IsNullOrEmpty(value)
					? Match.Empty
					: __isIPv4.Match(value);
	}

	public static bool IsIPv4(string value)
	{
		if (string.IsNullOrEmpty(value)) return false;

		Match match = __isIPv4.Match(value);
		if (!match.Success) return false;

		try
		{
			IPAddress.Parse(match.Groups["ip"].Value);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsLoopback(string value)
	{
		return string.IsNullOrEmpty(value)
				|| value is "::1" or "0.0.0.1" or "0.0.0.0" or "127.0.0.1" || value.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase) || value.Equals("localhost", StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsIPv4Url(string value)
	{
		if (string.IsNullOrEmpty(value)) return false;

		Match match = __isIPv4Url.Match(value);
		if (!match.Success) return false;

		try
		{
			IPAddress.Parse(match.Groups["ip"].Value);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsPossibleIPRange(string value) { return value != null && (__isIPv4Simple.IsMatch(value) || __isIPv4CIDR.IsMatch(value)); }

	public static bool IsPossibleIP(string value) { return IsPossibleIPRange(value) || value != null && __isIPv4Url.IsMatch(value); }

	public static bool IsPossibleIPv4(string value) { return IsPossibleIPRange(value) || value != null && __isIPv4.IsMatch(value); }

	public static List<string> Parse(string value)
	{
		if (string.IsNullOrEmpty(value)) return null;

		byte[] beginIp = new byte[4];
		byte[] endIp = new byte[4];

		try
		{
			Match match = __isIPv4Url.Match(value);
			if (match.Success) return [match.Groups["url"]?.Value];

			if (value.Contains("/"))
			{
				if (!TryParseCIDRNotation(value, beginIp, endIp)) return null;
			}
			else if (value.Contains("-"))
			{
				if (!TryParseSimpleRange(value, beginIp, endIp)) return null;
			}
			else
			{
				return [IPAddress.Parse(value).ToString()];
			}
		}
		catch
		{
			return null;
		}

		List<string> list = [];

		for (int i = beginIp[0]; i <= endIp[0]; i++)
		{
			for (int j = beginIp[1]; j <= endIp[1]; j++)
			{
				for (int x = beginIp[2]; x <= endIp[2]; x++)
				{
					for (int y = beginIp[3]; y <= endIp[3]; y++)
					{
						list.Add(new IPAddress(new[] { (byte)i, (byte)j, (byte)x, (byte)y }).ToString());
					}
				}
			}
		}

		return list;
	}

	public static IPAddress GetLocalIP()
	{
		return GetLocalIP(nit => nit is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.GenericModem).FirstOrDefault();
	}

	public static IPAddress GetLocalIP([NotNull] params NetworkInterfaceType[] types)
	{
		return types.Length == 0
					? GetLocalIP()
					: GetLocalIP(types.Contains).FirstOrDefault();
	}

	[ItemNotNull]
	public static IEnumerable<IPAddress> GetLocalIP([NotNull] Predicate<NetworkInterfaceType> filter)
	{
		foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
		{
			if (ni.OperationalStatus != OperationalStatus.Up) continue;
			if (!filter(ni.NetworkInterfaceType)) continue;

			IPInterfaceProperties properties = ni.GetIPProperties();
			if (properties.UnicastAddresses.Count == 0) continue;

			foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
			{
				if (address.Address.AddressFamily != AddressFamily.InterNetwork || !address.IsDnsEligible) continue;

				if (address.Address.Equals(IPAddress.Any) ||
					address.Address.Equals(IPAddress.Loopback) ||
					address.Address.Equals(IPAddress.IPv6Any) ||
					address.Address.Equals(IPAddress.IPv6Loopback) ||
					address.Address.Equals(IPAddress.IPv6None))
				{
					continue;
				}

				if (address.DuplicateAddressDetectionState is DuplicateAddressDetectionState.Invalid or DuplicateAddressDetectionState.Duplicate) continue;
				yield return address.Address;
			}
		}
	}

	public static IPAddress GetPublicIP()
	{
		IPAddress result;

		try
		{
			IOHttpRequestSettings settings = new IOHttpRequestSettings
			{
				OnResponseReceived = r => ((HttpWebResponse)r).StatusCode == HttpStatusCode.OK
			};
			WebRequest request = UriHelper.MakeHttpWebRequest(STR_PUBLIC_IP, settings);
			string content = request.ReadToEnd(settings);
			if (string.IsNullOrEmpty(content)) return null;

			Match match = __extractIP.Match(content);
			result = match.Success ? IPAddress.Parse(match.Groups["ip"].Value) : null;
		}
		catch
		{
			result = null;
		}

		return result;
	}

	public static async Task<IPAddress> GetPublicIPAsync(CancellationToken token = default(CancellationToken))
	{
		IPAddress result;

		try
		{
			IOHttpRequestSettings settings = new IOHttpRequestSettings
			{
				OnResponseReceived = r => ((HttpWebResponse)r).StatusCode == HttpStatusCode.OK
			};
			WebRequest request = UriHelper.MakeHttpWebRequest(STR_PUBLIC_IP, settings);
			string content = await request.ReadToEndAsync(settings, token);
			if (string.IsNullOrEmpty(content)) return null;

			Match match = __extractIP.Match(content);
			result = match.Success ? IPAddress.Parse(match.Groups["ip"].Value) : null;
		}
		catch
		{
			result = null;
		}

		return result;
	}

	public static uint ToUInt([NotNull] string value) { return IPAddress.Parse(value).ToUInt(); }

	[NotNull]
	public static IPAddress ToIP(uint value) { return new IPAddress(BitConverter.GetBytes(value)); }

	public static int ToIPNumber(string value)
	{
		if (!IsIPv4(value)) return 0;

		string[] parts = value.Split(4, '.');
		if (parts.Length != 4) return 0;

		byte[] bytes = parts.Select(byte.Parse).ToArray();
		return ToIPNumber(bytes);
	}

	public static int ToIPNumber([NotNull] byte[] value)
	{
		/*
		http://www.ip2location.com/faqs/db1-ip-country
		FAQs for IP2Location� IP-Country DataProvider [DB1]
		How do I add licenses to existing subscription?
		
		IP Number = 16777216 * w + 65536 * x + 256 * y + z
		IP Address = w.x.y.z
		*/
		if (value.Length != 4) throw new ArgumentException("Array length mismatch", nameof(value));

		int i;
		double num = 0;

		for (i = value.Length - 1; i >= 0; i--)
			num += value[i] % 256 * Math.Pow(256, 3 - i);

		return (int)num;
	}

	/// <summary>
	/// Parse IP-range string in CIDR notation.
	/// For example "12.15.0.0/16".
	/// </summary>
	/// <param name="ipRange"></param>
	/// <param name="beginIp"></param>
	/// <param name="endIp"></param>
	/// <returns></returns>
	public static bool TryParseCIDRNotation(string ipRange, [NotNull] byte[] beginIp, [NotNull] byte[] endIp)
	{
		if (beginIp.Length != 4) throw new ArgumentException("byte array size mismatch", nameof(beginIp));
		if (endIp.Length != 4) throw new ArgumentException("byte array size mismatch", nameof(endIp));
		beginIp.FastInitialize(byte.MinValue);
		endIp.FastInitialize(byte.MinValue);
		if (string.IsNullOrWhiteSpace(ipRange)) return false;

		string[] x = ipRange.Split(2, '/');
		if (x.Length != 2) return false;

		byte bits = byte.Parse(x[1]);
		uint ip = 0;
		string[] ipParts = x[0].Split(4, '.');
		if (ipParts.Length != 4) return false;

		for (int i = 0; i < 4; i++)
		{
			ip = ip << 8;
			ip += uint.Parse(ipParts[i]);
		}

		byte shiftBits = (byte)(32 - bits);
		uint ip1 = (ip >> shiftBits) << shiftBits;

		if (ip1 != ip) // Check correct subnet address
			return false;

		uint ip2 = ip1 >> shiftBits;

		for (int k = 0; k < shiftBits; k++)
			ip2 = (ip2 << 1) + 1;

		for (int i = 0; i < 4; i++)
		{
			beginIp[i] = (byte)((ip1 >> (3 - i) * 8) & 255);
			endIp[i] = (byte)((ip2 >> (3 - i) * 8) & 255);
		}

		return true;
	}

	/// <summary>
	/// Parse IP-range string "12.15.16.1-30.10.255"
	/// </summary>
	/// <param name="ipRange"></param>
	/// <param name="beginIp"></param>
	/// <param name="endIp"></param>
	/// <returns></returns>
	public static bool TryParseSimpleRange(string ipRange, [NotNull] byte[] beginIp, [NotNull] byte[] endIp)
	{
		if (beginIp.Length != 4) throw new ArgumentException("byte array size mismatch", nameof(beginIp));
		if (endIp.Length != 4) throw new ArgumentException("byte array size mismatch", nameof(endIp));
		beginIp.FastInitialize(byte.MinValue);
		endIp.FastInitialize(byte.MinValue);
		if (string.IsNullOrWhiteSpace(ipRange)) return false;

		string[] ranges = ipRange.Split(2, '-');
		if (ranges.Length != 2) return false;

		string[] ipParts0 = ranges[0].Split(4, '.');
		if (ipParts0.Length != 4) return false;

		string[] ipParts1 = ranges[1].Split(4, '.');
		if (ipParts1.Length != 4) return false;

		for (int i = 0; i < 4; i++)
		{
			beginIp[i] = byte.Parse(ipParts0[i]);
			endIp[i] = byte.Parse(ipParts1[i]);
		}

		return true;
	}
}