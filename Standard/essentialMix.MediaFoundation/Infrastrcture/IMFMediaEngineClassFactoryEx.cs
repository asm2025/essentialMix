using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("c56156c6-ea5b-48a5-9df8-fbe035d0929e")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineClassFactoryEx : IMFMediaEngineClassFactory
	{
		#region IMFMediaEngineClassFactory methods

		[PreserveSig]
		new int CreateInstance(
			MF_MEDIA_ENGINE_CREATEFLAGS dwFlags,
			IMFAttributes pAttr,
			out IMFMediaEngine ppPlayer
		);

		[PreserveSig]
		new int CreateTimeRange(
			out IMFMediaTimeRange ppTimeRange
		);

		[PreserveSig]
		new int CreateError(
			out IMFMediaError ppError
		);

		#endregion

		[PreserveSig]
		int CreateMediaSourceExtension(
			int dwFlags,
			IMFAttributes pAttr,
			out IMFMediaSourceExtension ppMSE
		);

		[PreserveSig]
		int CreateMediaKeys(
			[MarshalAs(UnmanagedType.BStr)] string keySystem,
			[MarshalAs(UnmanagedType.BStr)] string cdmStorePath,
			out IMFMediaKeys ppKeys
		);

		[PreserveSig]
		int IsTypeSupported(
			[MarshalAs(UnmanagedType.BStr)] string type,
			[MarshalAs(UnmanagedType.BStr)] string keySystem,
			[MarshalAs(UnmanagedType.Bool)] out bool isSupported
		);
	}
}