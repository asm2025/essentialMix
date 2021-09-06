using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("a6bba433-176b-48b2-b375-53aa03473207")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureEngine
	{
		[PreserveSig]
		int Initialize(
			IMFCaptureEngineOnEventCallback pEventCallback,
			IMFAttributes pAttributes,
			[MarshalAs(UnmanagedType.IUnknown)] object pAudioSource,
			[MarshalAs(UnmanagedType.IUnknown)] object pVideoSource
		);

		[PreserveSig]
		int StartPreview();

		[PreserveSig]
		int StopPreview();

		[PreserveSig]
		int StartRecord();

		[PreserveSig]
		int StopRecord(
			[MarshalAs(UnmanagedType.Bool)] bool bFinalize,
			[MarshalAs(UnmanagedType.Bool)] bool bFlushUnprocessedSamples
		);

		[PreserveSig]
		int TakePhoto();

		[PreserveSig]
		int GetSink(
			MF_CAPTURE_ENGINE_SINK_TYPE mfCaptureEngineSinkType,
			out IMFCaptureSink ppSink
		);

		[PreserveSig]
		int GetSource(
			out IMFCaptureSource ppSource
		);
	}
}