using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("3DE21209-8BA6-4f2a-A577-2819B56FF14D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAdvancedMediaCaptureInitializationSettings
    {
        [PreserveSig]
        int SetDirectxDeviceManager(
            IMFDXGIDeviceManager value
            );
    }
}
