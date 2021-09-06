using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("52150b82-ab39-4467-980f-e48bf0822ecd")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureEngineOnSampleCallback
	{
		[PreserveSig]
		int OnSample(
			IMFSample pSample
		);
	}
}