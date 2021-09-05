using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("72d6135b-35e9-412c-b926-fd5265f2a885")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureSink
	{
		[PreserveSig]
		int GetOutputMediaType(
			int dwSinkStreamIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		int GetService(
			int dwSinkStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid rguidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnknown
		);

		[PreserveSig]
		int AddStream(
			int dwSourceStreamIndex,
			IMFMediaType pMediaType,
			IMFAttributes pAttributes,
			out int pdwSinkStreamIndex
		);

		[PreserveSig]
		int Prepare();

		[PreserveSig]
		int RemoveAllStreams();
	}
}