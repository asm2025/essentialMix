using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56C294D0-753E-4260-8D61-A3D8820B1D54")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFDesiredSample
	{
		[PreserveSig]
		int GetDesiredSampleTimeAndDuration(out long phnsSampleTime,
			out long phnsSampleDuration);

		[PreserveSig]
		void SetDesiredSampleTimeAndDuration([In] long hnsSampleTime,
			[In] long hnsSampleDuration);

		[PreserveSig]
		void Clear();
	}
}