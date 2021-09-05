using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("17C3779E-3CDE-4EDE-8C60-3899F5F53AD6")]
	public interface IMFSinkWriterEncoderConfig
	{
		[PreserveSig]
		int SetTargetMediaType(
			int dwStreamIndex,
			IMFMediaType pTargetMediaType,
			IMFAttributes pEncodingParameters
		);

		[PreserveSig]
		int PlaceEncodingParameters(
			int dwStreamIndex,
			IMFAttributes pEncodingParameters
		);
	}
}