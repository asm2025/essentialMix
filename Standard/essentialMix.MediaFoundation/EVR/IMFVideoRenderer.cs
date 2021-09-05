using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Transform;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("DFDFD197-A9CA-43D8-B341-6AF3503792CD")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFVideoRenderer
	{
		[PreserveSig]
		int InitializeRenderer([In][MarshalAs(UnmanagedType.Interface)] IMFTransform pVideoMixer,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFVideoPresenter pVideoPresenter);
	}
}