using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("db71a2fc-078a-414e-9df9-8c2531b0aa6c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaTimeRange
	{
		[PreserveSig]
		int GetLength();

		[PreserveSig]
		int GetStart(
			int index,
			out double pStart
		);

		[PreserveSig]
		int GetEnd(
			int index,
			out  double pEnd
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool ContainsTime(
			double time
		);

		[PreserveSig]
		int AddRange(
			double startTime,
			double endTime
		);

		[PreserveSig]
		int Clear();
	}
}