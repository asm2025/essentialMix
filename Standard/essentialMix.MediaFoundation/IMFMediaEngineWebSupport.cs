using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("ba2743a1-07e0-48ef-84b6-9a2ed023ca6c")]
	public interface IMFMediaEngineWebSupport
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool ShouldDelayTheLoadEvent();

		[PreserveSig]
		int ConnectWebAudio(
			int dwSampleRate,
			out IAudioSourceProvider ppSourceProvider
		);

		[PreserveSig]
		int DisconnectWebAudio();
	}
}