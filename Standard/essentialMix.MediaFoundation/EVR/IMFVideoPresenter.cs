using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("29AFF080-182A-4A5D-AF3B-448F3A6346CB")]
	public interface IMFVideoPresenter : IMFClockStateSink
	{
		#region IMFClockStateSink

		[PreserveSig]
		new int OnClockStart([In] long hnsSystemTime,
			[In] long llClockStartOffset);

		[PreserveSig]
		new int OnClockStop([In] long hnsSystemTime);

		[PreserveSig]
		new int OnClockPause([In] long hnsSystemTime);

		[PreserveSig]
		new int OnClockRestart([In] long hnsSystemTime);

		[PreserveSig]
		new int OnClockSetRate([In] long hnsSystemTime,
			[In] float flRate);

		#endregion

		[PreserveSig]
		int ProcessMessage(MFVPMessageType eMessage,
			IntPtr ulParam);

		[PreserveSig]
		int GetCurrentMediaType([MarshalAs(UnmanagedType.Interface)] out IMFVideoMediaType ppMediaType);
	}
}