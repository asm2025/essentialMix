using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("53590F48-DC3B-4297-813F-787761AD7B3E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFIndexer
	{
		[PreserveSig]
		int SetFlags(
			[In] MFASFIndexerFlags dwFlags);

		[PreserveSig]
		int GetFlags(
			out MFASFIndexerFlags pdwFlags);

		[PreserveSig]
		int Initialize(
			[In] IMFASFContentInfo pIContentInfo);

		[PreserveSig]
		int GetIndexPosition(
			[In] IMFASFContentInfo pIContentInfo,
			out long pcbIndexOffset);

		[PreserveSig]
		int SetIndexByteStreams([In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] IMFByteStream[] ppIByteStreams,
			[In] int cByteStreams);

		[PreserveSig]
		int GetIndexByteStreamCount(
			out int pcByteStreams);

		[PreserveSig]
		int GetIndexStatus([In][MarshalAs(UnmanagedType.LPStruct)] ASFIndexIdentifier pIndexIdentifier,
			[Out][MarshalAs(UnmanagedType.Bool)]
			out bool pfIsIndexed,
			IntPtr pbIndexDescriptor,
			ref int pcbIndexDescriptor);

		[PreserveSig]
		int SetIndexStatus(
			[In] IntPtr pbIndexDescriptor,
			[In] int cbIndexDescriptor,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fGenerateIndex);

		[PreserveSig]
		int GetSeekPositionForValue([In][MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarValue,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ASFIndexIdentifier pIndexIdentifier,
			out long pcbOffsetWithinData,
			IntPtr phnsApproxTime,
			out int pdwPayloadNumberOfStreamWithinPacket);

		[PreserveSig]
		int GenerateIndexEntries(
			[In] IMFSample pIASFPacketSample);

		[PreserveSig]
		int CommitIndex(
			[In] IMFASFContentInfo pIContentInfo);

		[PreserveSig]
		int GetIndexWriteSpace(
			out long pcbIndexWriteSpace);

		[PreserveSig]
		int GetCompletedIndex(
			[In] IMFMediaBuffer pIIndexBuffer,
			[In] long cbOffsetWithinIndex);
	}
}