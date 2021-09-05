using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D267BF6A-028B-4e0d-903D-43F0EF82D0D4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFProfile : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFASFProfile.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
		);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
		);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
		);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
		);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
		);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
		);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
		);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
		);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
		);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
		);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
		);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
		);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
		);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
		);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
		);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
		);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
		);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
		);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
		);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
		);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
		);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
		);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
		);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
		);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFASFProfile.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
		);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
		);

		#endregion

		[PreserveSig]
		int GetStreamCount(
			out int pcStreams);

		[PreserveSig]
		int GetStream(
			[In] int dwStreamIndex,
			out short pwStreamNumber,
			out IMFASFStreamConfig ppIStream);

		[PreserveSig]
		int GetStreamByNumber(
			[In] short wStreamNumber,
			out IMFASFStreamConfig ppIStream);

		[PreserveSig]
		int SetStream(
			[In] IMFASFStreamConfig pIStream);

		[PreserveSig]
		int RemoveStream(
			[In] short wStreamNumber);

		[PreserveSig]
		int CreateStream(
			[In] IMFMediaType pIMediaType,
			out IMFASFStreamConfig ppIStream);

		[PreserveSig]
		int GetMutualExclusionCount(
			out int pcMutexs);

		[PreserveSig]
		int GetMutualExclusion(
			[In] int dwMutexIndex,
			out IMFASFMutualExclusion ppIMutex);

		[PreserveSig]
		int AddMutualExclusion(
			[In] IMFASFMutualExclusion pIMutex);

		[PreserveSig]
		int RemoveMutualExclusion(
			[In] int dwMutexIndex);

		[PreserveSig]
		int CreateMutualExclusion(
			out IMFASFMutualExclusion ppIMutex);

		[PreserveSig]
		int GetStreamPrioritization([MarshalAs(UnmanagedType.IUnknown)] out object ppIStreamPrioritization);
		[PreserveSig]
		int AddStreamPrioritization([MarshalAs(UnmanagedType.IUnknown)] object pIStreamPrioritization);

		[PreserveSig]
		int RemoveStreamPrioritization();

		[PreserveSig]
		int CreateStreamPrioritization([MarshalAs(UnmanagedType.IUnknown)] out object ppIStreamPrioritization);

		[PreserveSig]
		int Clone(
			out IMFASFProfile ppIProfile);
	}
}