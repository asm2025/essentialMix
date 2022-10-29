using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using JetBrains.Annotations;

namespace essentialMix.Windows.Helpers;

public static class MacAddressHelper
{
	[NotNull]
	public static IEnumerable<PhysicalAddress> GetMacAddress()
	{
		Func<ManagementObject, PhysicalAddress> converter = mo => PhysicalAddress.Parse(Convert.ToString(mo["MacAddress"]));
		SystemInfoRequest<PhysicalAddress> request = new SystemInfoRequest<PhysicalAddress>(SystemInfoType.Win32_NetworkAdapterConfiguration, converter)
		{
			Filter = mo => Convert.ToBoolean(mo["IPEnabled"])
		};

		return SystemInfo.Get(request);
	}
}