using System.Runtime.InteropServices;
using essentialMix.MediaFoundation.EVR;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("8D3CE1BF-2367-40E0-9EEE-40D377CC1B46")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSharingEngine : IMFMediaEngine
	{
		#region IMFMediaEngine methods

		[PreserveSig]
		new int GetError(
			out IMFMediaError ppError
		);

		[PreserveSig]
		new int SetErrorCode(
			MF_MEDIA_ENGINE_ERR error
		);

		[PreserveSig]
		new int SetSourceElements(
			IMFMediaEngineSrcElements pSrcElements
		);

		[PreserveSig]
		new int SetSource(
			[MarshalAs(UnmanagedType.BStr)] string pUrl
		);

		[PreserveSig]
		new int GetCurrentSource(
			[MarshalAs(UnmanagedType.BStr)] out string ppUrl
		);

		[PreserveSig]
		new MF_MEDIA_ENGINE_NETWORK GetNetworkState();

		[PreserveSig]
		new MF_MEDIA_ENGINE_PRELOAD GetPreload();

		[PreserveSig]
		new int SetPreload(
			MF_MEDIA_ENGINE_PRELOAD Preload
		);

		[PreserveSig]
		new int GetBuffered(
			out IMFMediaTimeRange ppBuffered
		);

		[PreserveSig]
		new int Load();

		[PreserveSig]
		new int CanPlayType(
			[MarshalAs(UnmanagedType.BStr)] string type,
			out MF_MEDIA_ENGINE_CANPLAY pAnswer
		);

		[PreserveSig]
		new MF_MEDIA_ENGINE_READY GetReadyState();

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool IsSeeking();

		[PreserveSig]
		new double GetCurrentTime();

		[PreserveSig]
		new int SetCurrentTime(
			double seekTime
		);

		[PreserveSig]
		new double GetStartTime();

		[PreserveSig]
		new double GetDuration();

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool IsPaused();

		[PreserveSig]
		new double GetDefaultPlaybackRate();

		[PreserveSig]
		new int SetDefaultPlaybackRate(
			double Rate
		);

		[PreserveSig]
		new double GetPlaybackRate();

		[PreserveSig]
		new int SetPlaybackRate(
			double Rate
		);

		[PreserveSig]
		new int GetPlayed(
			out IMFMediaTimeRange ppPlayed
		);

		[PreserveSig]
		new int GetSeekable(
			out IMFMediaTimeRange ppSeekable
		);

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool IsEnded();

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool GetAutoPlay();

		[PreserveSig]
		new int SetAutoPlay(
			[MarshalAs(UnmanagedType.Bool)] bool AutoPlay
		);

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool GetLoop();

		[PreserveSig]
		new int SetLoop(
			[MarshalAs(UnmanagedType.Bool)] bool Loop
		);

		[PreserveSig]
		new int Play();

		[PreserveSig]
		new int Pause();

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool GetMuted();

		[PreserveSig]
		new int SetMuted(
			[MarshalAs(UnmanagedType.Bool)] bool Muted
		);

		[PreserveSig]
		new double GetVolume();

		[PreserveSig]
		new int SetVolume(
			double Volume
		);

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool HasVideo();

		[return: MarshalAs(UnmanagedType.Bool)]
		new bool HasAudio();

		[PreserveSig]
		new int GetNativeVideoSize(
			out int cx,
			out int cy
		);

		[PreserveSig]
		new int GetVideoAspectRatio(
			out int cx,
			out int cy
		);

		[PreserveSig]
		new int Shutdown();

		[PreserveSig]
		new int TransferVideoFrame([In][MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
			[In] MFVideoNormalizedRect pSrc,
			[In] MFRect pDst,
			[In] MFARGB pBorderClr
		);

		[PreserveSig]
		new int OnVideoStreamTick(
			out long pPts
		);

		#endregion

		[PreserveSig]
		int GetDevice(
			out DEVICE_INFO pDevice
		);
	}
}