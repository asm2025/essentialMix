using System;
using System.Net;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IPAddressExtension
{
	public static uint ToUInt([NotNull] this IPAddress thisValue) { return BitConverter.ToUInt32(thisValue.GetAddressBytes(), 0); }
}