using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("e467b94e-a713-4562-a802-816a42e9008a")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSourceExtension
	{
		[PreserveSig]
		IMFSourceBufferList GetSourceBuffers();

		[PreserveSig]
		IMFSourceBufferList GetActiveSourceBuffers();

		[PreserveSig]
		MF_MSE_READY GetReadyState();

		[PreserveSig]
		double GetDuration();

		[PreserveSig]
		int SetDuration(
			double duration
		);

		[PreserveSig]
		int AddSourceBuffer(
			[MarshalAs(UnmanagedType.BStr)] string type,
			IMFSourceBufferNotify pNotify,
			out IMFSourceBuffer ppSourceBuffer
		);

		[PreserveSig]
		int RemoveSourceBuffer(
			IMFSourceBuffer pSourceBuffer
		);

		[PreserveSig]
		int SetEndOfStream(
			MF_MSE_ERROR error
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsTypeSupported(
			[MarshalAs(UnmanagedType.BStr)] string type
		);

		[PreserveSig]
		IMFSourceBuffer GetSourceBuffer(
			int dwStreamIndex
		);
	}
}