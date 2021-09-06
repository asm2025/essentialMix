using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DEVICE_INFO")]
	public struct DEVICE_INFO
    {
        [MarshalAs(UnmanagedType.BStr)]
        string pFriendlyDeviceName;

        [MarshalAs(UnmanagedType.BStr)]
        string pUniqueDeviceName;

        [MarshalAs(UnmanagedType.BStr)]
        string pManufacturerName;

        [MarshalAs(UnmanagedType.BStr)]
        string pModelName;

        [MarshalAs(UnmanagedType.BStr)]
        string pIconURL;
    }
}
