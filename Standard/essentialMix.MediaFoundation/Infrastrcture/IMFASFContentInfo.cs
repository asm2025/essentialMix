using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("B1DCA5CD-D5DA-4451-8E9E-DB5C59914EAD")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFContentInfo
	{
		[PreserveSig]
		int GetHeaderSize(
			[In] IMFMediaBuffer pIStartOfContent,
			out long cbHeaderSize);

		[PreserveSig]
		int ParseHeader(
			[In] IMFMediaBuffer pIHeaderBuffer,
			[In] long cbOffsetWithinHeader);

		[PreserveSig]
		int GenerateHeader(
			[In] IMFMediaBuffer pIHeader,
			out int pcbHeader);

		[PreserveSig]
		int GetProfile(
			out IMFASFProfile ppIProfile);

		[PreserveSig]
		int SetProfile(
			[In] IMFASFProfile pIProfile);

		[PreserveSig]
		int GeneratePresentationDescriptor(
			out IMFPresentationDescriptor ppIPresentationDescriptor);

		[PreserveSig]
		int GetEncodingConfigurationPropertyStore(
			[In] short wStreamNumber,
			out IPropertyStore ppIStore);
	}
}