using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("9E8AE8D2-DBBD-4200-9ACA-06E6DF484913")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFStreamConfig : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFASFStreamConfig.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
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
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFASFStreamConfig.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
		);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
		);

		#endregion

		[PreserveSig]
		int GetStreamType(
			out Guid pguidStreamType);

		[PreserveSig]
		short GetStreamNumber();

		[PreserveSig]
		int SetStreamNumber(
			[In] short wStreamNum);

		[PreserveSig]
		int GetMediaType(
			out IMFMediaType ppIMediaType);

		[PreserveSig]
		int SetMediaType(
			[In] IMFMediaType pIMediaType);

		[PreserveSig]
		int GetPayloadExtensionCount(
			out short pcPayloadExtensions);

		[PreserveSig]
		int GetPayloadExtension(
			[In] short wPayloadExtensionNumber,
			out Guid pguidExtensionSystemID,
			out short pcbExtensionDataSize,
			IntPtr pbExtensionSystemInfo,
			ref int pcbExtensionSystemInfo);

		[PreserveSig]
		int AddPayloadExtension([In][MarshalAs(UnmanagedType.Struct)] Guid guidExtensionSystemID,
			[In] short cbExtensionDataSize,
			IntPtr pbExtensionSystemInfo,
			[In] int cbExtensionSystemInfo);

		[PreserveSig]
		int RemoveAllPayloadExtensions();

		[PreserveSig]
		int Clone(
			out IMFASFStreamConfig ppIStreamConfig);
	}
}