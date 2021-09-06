using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.EVR;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("98a1b0bb-03eb-4935-ae7c-93c1fa0e1c93")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngine
	{
		[PreserveSig]
		int GetError(
			out IMFMediaError ppError
		);

		[PreserveSig]
		int SetErrorCode(
			MF_MEDIA_ENGINE_ERR error
		);

		[PreserveSig]
		int SetSourceElements(
			IMFMediaEngineSrcElements pSrcElements
		);

		[PreserveSig]
		int SetSource(
			[MarshalAs(UnmanagedType.BStr)] string pUrl
		);

		[PreserveSig]
		int GetCurrentSource(
			[MarshalAs(UnmanagedType.BStr)] out string ppUrl
		);

		[PreserveSig]
		MF_MEDIA_ENGINE_NETWORK GetNetworkState();

		[PreserveSig]
		MF_MEDIA_ENGINE_PRELOAD GetPreload();

		[PreserveSig]
		int SetPreload(
			MF_MEDIA_ENGINE_PRELOAD Preload
		);

		[PreserveSig]
		int GetBuffered(
			out IMFMediaTimeRange ppBuffered
		);

		[PreserveSig]
		int Load();

		[PreserveSig]
		int CanPlayType(
			[MarshalAs(UnmanagedType.BStr)] string type,
			out MF_MEDIA_ENGINE_CANPLAY pAnswer
		);

		[PreserveSig]
		MF_MEDIA_ENGINE_READY GetReadyState();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsSeeking();

		[PreserveSig]
		double GetCurrentTime();

		[PreserveSig]
		int SetCurrentTime(
			double seekTime
		);

		[PreserveSig]
		double GetStartTime();

		[PreserveSig]
		double GetDuration();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsPaused();

		[PreserveSig]
		double GetDefaultPlaybackRate();

		[PreserveSig]
		int SetDefaultPlaybackRate(
			double Rate
		);

		[PreserveSig]
		double GetPlaybackRate();

		[PreserveSig]
		int SetPlaybackRate(
			double Rate
		);

		[PreserveSig]
		int GetPlayed(
			out IMFMediaTimeRange ppPlayed
		);

		[PreserveSig]
		int GetSeekable(
			out IMFMediaTimeRange ppSeekable
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsEnded();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool GetAutoPlay();

		[PreserveSig]
		int SetAutoPlay(
			[MarshalAs(UnmanagedType.Bool)] bool AutoPlay
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool GetLoop();

		[PreserveSig]
		int SetLoop(
			[MarshalAs(UnmanagedType.Bool)] bool Loop
		);

		[PreserveSig]
		int Play();

		[PreserveSig]
		int Pause();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool GetMuted();

		[PreserveSig]
		int SetMuted(
			[MarshalAs(UnmanagedType.Bool)] bool Muted
		);

		[PreserveSig]
		double GetVolume();

		[PreserveSig]
		int SetVolume(
			double Volume
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool HasVideo();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool HasAudio();

		[PreserveSig]
		int GetNativeVideoSize(
			out int cx,
			out int cy
		);

		[PreserveSig]
		int GetVideoAspectRatio(
			out int cx,
			out int cy
		);

		[PreserveSig]
		int Shutdown();

		[PreserveSig]
		int TransferVideoFrame([In][MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFVideoNormalizedRect pSrc,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFRect pDst,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFARGB pBorderClr
		);

		[PreserveSig]
		int OnVideoStreamTick(
			out long pPts
		);
	}
}