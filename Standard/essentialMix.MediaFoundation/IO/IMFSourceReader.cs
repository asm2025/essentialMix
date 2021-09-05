using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
	public interface IMFSourceReader
	{
		[PreserveSig]
		int GetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] out bool pfSelected
		);

		[PreserveSig]
		int SetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] bool fSelected
		);

		[PreserveSig]
		int GetNativeMediaType(
			int dwStreamIndex,
			int dwMediaTypeIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		int GetCurrentMediaType(
			int dwStreamIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		int SetCurrentMediaType(
			int dwStreamIndex,
			[In][Out] MFInt pdwReserved,
			IMFMediaType pMediaType
		);

		[PreserveSig]
		int SetCurrentPosition([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant varPosition
		);

		[PreserveSig]
		int ReadSample(
			int dwStreamIndex,
			MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
			out int pdwActualStreamIndex,
			out  MF_SOURCE_READER_FLAG pdwStreamFlags,
			out long pllTimestamp,
			out IMFSample ppSample
		);

		[PreserveSig]
		int Flush(
			int dwStreamIndex
		);

		[PreserveSig]
		int GetServiceForStream(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		int GetPresentationAttribute(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidAttribute,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSourceReader.GetPresentationAttribute", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvarAttribute
		);
	}
}