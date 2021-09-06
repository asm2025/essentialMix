using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("4D645ACE-26AA-4688-9BE1-DF3516990B93")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineClassFactory
	{
		[PreserveSig]
		int CreateInstance(
			MF_MEDIA_ENGINE_CREATEFLAGS dwFlags,
			IMFAttributes pAttr,
			out IMFMediaEngine ppPlayer
		);

		[PreserveSig]
		int CreateTimeRange(
			out IMFMediaTimeRange ppTimeRange
		);

		[PreserveSig]
		int CreateError(
			out IMFMediaError ppError
		);
	}
}