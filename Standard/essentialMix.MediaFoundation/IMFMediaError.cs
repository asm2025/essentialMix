using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("fc0e10d2-ab2a-4501-a951-06bb1075184c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaError
    {
        [PreserveSig]
        MF_MEDIA_ENGINE_ERR GetErrorCode();

        [PreserveSig]
        int GetExtendedErrorCode();

        [PreserveSig]
        int SetErrorCode(
            MF_MEDIA_ENGINE_ERR error
            );

        [PreserveSig]
        int SetExtendedErrorCode(
            int error
            );
    }
}
