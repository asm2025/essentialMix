using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("AEA36028-796D-454F-BEEE-B48071E24304")]
	public interface IEVRFilterConfigEx : IEVRFilterConfig
	{
		#region IEVRFilterConfig methods

		[PreserveSig]
		new int SetNumberOfStreams(int dwMaxStreams);

		[PreserveSig]
		new int GetNumberOfStreams(out int pdwMaxStreams);

		#endregion

		[PreserveSig]
		int SetConfigPrefs([In] EVRFilterConfigPrefs dwConfigFlags);

		[PreserveSig]
		int GetConfigPrefs(out EVRFilterConfigPrefs pdwConfigFlags);
	}
}