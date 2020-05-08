using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class MacAddressHelper
	{
		static MacAddressHelper()
		{
			List<Regex> allFormatRegularExpressions = new List<Regex>(FormatRegularExpressions.Count * 2);

			foreach (KeyValuePair<string, Regex[]> pair in FormatRegularExpressions)
			{
				allFormatRegularExpressions.Add(pair.Value);
			}

			AllFormatRegularExpressions = allFormatRegularExpressions.ToArray();
			DefaultFormat = Formats.First();
		}

		public static string DefaultFormat { get; }

		public static IReadOnlyList<string> Formats { get; } = new[]
		{
			"XX-XX-XX-XX-XX-XX",
			"XX:XX:XX:XX:XX:XX",
			"XXXX.XXXX.XXXX",
			"XXXXXXXXXXXX"
		};

		public static IReadOnlyDictionary<string, string> Masks { get; } =
			new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				new KeyValuePair<string, string>(Formats[0], ">aa-aa-aa-aa-aa-aa"),
				new KeyValuePair<string, string>(Formats[1], ">aa':'aa':'aa':'aa':'aa':'aa"),
				new KeyValuePair<string, string>(Formats[2], ">aaaa.aaaa.aaaa"),
				new KeyValuePair<string, string>(Formats[3], ">aaaaaaaaaaaa")
			});

		public static IReadOnlyCollection<Regex> AllFormatRegularExpressions { get; }

		public static IReadOnlyDictionary<string, Regex[]> FormatRegularExpressions { get; } =
			new ReadOnlyDictionary<string, Regex[]>(new Dictionary<string, Regex[]>(StringComparer.OrdinalIgnoreCase)
			{
				new KeyValuePair<string, Regex[]>(Formats[0], new[]
				{
					new Regex(@"\A(?<mac>[\dA-F]{2}(?<delimiter>-)(?:(?:[\dA-F]{2}-){1,3})(?:[\dA-F]{2}-?))(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I),
					new Regex(@"\A(?<mac>[\dA-F]{2}(?<delimiter>-)(?:[\dA-F](?:[\dA-F]-?)?)?)(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I)
				}),
				new KeyValuePair<string, Regex[]>(Formats[1], new[]
				{
					new Regex(@"\A(?<mac>[\dA-F]{2}(?<delimiter>:)(?:(?:[\dA-F]{2}:){1,3})(?:[\dA-F]{2}:?))(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I),
					new Regex(@"\A(?<mac>[\dA-F]{2}(?<delimiter>:)(?:[\dA-F](?:[\dA-F]:?)?)?)(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I)
				}),
				new KeyValuePair<string, Regex[]>(Formats[2], new[]
				{
					new Regex(@"\A(?<mac>(?:[\dA-F]{4}(?<delimiter>\.)){1,2}[\dA-F]{0,4})(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I)
				}),
				new KeyValuePair<string, Regex[]>(Formats[3], new[]
				{
					new Regex(@"\A(?<mac>(?:[\dA-F]{1,12}(?<delimiter>)))(?:\t(?<count>\d+))?\z", RegexHelper.OPTIONS_I)
				})
			});

		public static string GetFormatMask(string format)
		{
			if (!IsFormatSupported(format)) throw new NotSupportedException("Format is not supported.");
			return Masks[format];
		}

		public static Regex[] GetFormatRegEx([NotNull] string format)
		{
			if (format == null) throw new ArgumentNullException(nameof(format));
			if (!IsFormatSupported(format)) throw new NotSupportedException("Format is not supported");
			return FormatRegularExpressions[format];
		}

		public static bool IsFormatSupported(string format) { return !string.IsNullOrEmpty(format) && Formats.Contains(format, StringComparer.OrdinalIgnoreCase); }

		public static bool IsAllowed(char c) { return c.IsHexadecimal(); }

		[NotNull]
		public static IEnumerable<PhysicalAddress> GetMacAddress()
		{
			return NetworkInterface.GetAllNetworkInterfaces()
				.Where(e => e.OperationalStatus == OperationalStatus.Up)
				.Select(e => e.GetPhysicalAddress());
		}

		[NotNull]
		public static IEnumerable<PhysicalAddress> GetMacAddressWMI()
		{
			Func<ManagementObject, PhysicalAddress> converter = mo => PhysicalAddress.Parse(Convert.ToString(mo["MacAddress"]));
			SystemInfoRequest<PhysicalAddress> request = new SystemInfoRequest<PhysicalAddress>(SystemInfoType.Win32_NetworkAdapterConfiguration, converter)
			{
				Filter = mo => Convert.ToBoolean(mo["IPEnabled"])
			};

			return SystemInfo.Get(request);
		}
	}
}