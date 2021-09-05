using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Internal;
using essentialMix.MediaFoundation.Transform;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("7b981cf0-560e-4116-9875-b099895f23d7")]
	public interface IMFSourceReaderEx : IMFSourceReader
	{
		#region IMFSourceReader Methods

		[PreserveSig]
		new int GetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] out bool pfSelected
		);

		[PreserveSig]
		new int SetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] bool fSelected
		);

		[PreserveSig]
		new int GetNativeMediaType(
			int dwStreamIndex,
			int dwMediaTypeIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		new int GetCurrentMediaType(
			int dwStreamIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		new int SetCurrentMediaType(
			int dwStreamIndex,
			[In][Out] MFInt pdwReserved,
			IMFMediaType pMediaType
		);

		[PreserveSig]
		new int SetCurrentPosition([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant varPosition
		);

		[PreserveSig]
		new int ReadSample(
			int dwStreamIndex,
			MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
			out int pdwActualStreamIndex,
			out  MF_SOURCE_READER_FLAG pdwStreamFlags,
			out long pllTimestamp,
			out IMFSample ppSample
		);

		[PreserveSig]
		new int Flush(
			int dwStreamIndex
		);

		[PreserveSig]
		new int GetServiceForStream(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		new int GetPresentationAttribute(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidAttribute,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSourceReaderEx.GetPresentationAttribute", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvarAttribute
		);

		#endregion

		[PreserveSig]
		int SetNativeMediaType(
			int dwStreamIndex,
			IMFMediaType pMediaType,
			out MF_SOURCE_READER_FLAG pdwStreamFlags);

		[PreserveSig]
		int AddTransformForStream(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.IUnknown)] object pTransformOrActivate);

		[PreserveSig]
		int RemoveAllTransformsForStream(
			int dwStreamIndex);

		[PreserveSig]
		int GetTransformForStream(
			int dwStreamIndex,
			int dwTransformIndex,
			out Guid pGuidCategory,
			out IMFTransform ppTransform);
	}
}