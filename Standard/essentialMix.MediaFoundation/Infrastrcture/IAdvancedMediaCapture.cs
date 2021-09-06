using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D0751585-D216-4344-B5BF-463B68F977BB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAdvancedMediaCapture
	{
		[PreserveSig]
		int GetAdvancedMediaCaptureSettings(
			out IAdvancedMediaCaptureSettings value
		);
	}
}