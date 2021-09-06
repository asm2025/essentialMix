using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFMPEG2DLNASINKSTATS")]
	public struct MFMPEG2DLNASINKSTATS
	{
		long cBytesWritten;
		bool fPAL;
		int fccVideo;
		int dwVideoWidth;
		int dwVideoHeight;
		long cVideoFramesReceived;
		long cVideoFramesEncoded;
		long cVideoFramesSkipped;
		long cBlackVideoFramesEncoded;
		long cVideoFramesDuplicated;
		int cAudioSamplesPerSec;
		int cAudioChannels;
		long cAudioBytesReceived;
		long cAudioFramesEncoded;
	}
}