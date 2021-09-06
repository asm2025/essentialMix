using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("EBBAF249-AFC2-4582-91C6-B60DF2E84954")]
	public interface IAudioSourceProvider
	{
		[PreserveSig]
		int ProvideInput(
			int dwSampleCount,
			ref int pdwChannelCount,
			out float[] pInterleavedAudioData
		);
	}
}