using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("699bdc27-bbaf-49ff-8e38-9c39c9b5e088")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFStreamPrioritization
	{
		[PreserveSig]
		int GetStreamCount(
			out int pdwStreamCount);

		[PreserveSig]
		int GetStream(
			[In] int dwStreamIndex,
			out short pwStreamNumber,
			out short pwStreamFlags); // bool

		[PreserveSig]
		int AddStream(
			[In] short wStreamNumber,
			[In] short wStreamFlags); // bool

		[PreserveSig]
		int RemoveStream(
			[In] int dwStreamIndex);

		[PreserveSig]
		int Clone(
			out IMFASFStreamPrioritization ppIStreamPrioritization);
	}
}