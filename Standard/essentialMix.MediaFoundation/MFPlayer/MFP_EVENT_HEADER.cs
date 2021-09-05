using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
    #region Declarations

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_EVENT_HEADER")]
	public class MFP_EVENT_HEADER
    {
        public MFP_EVENT_TYPE eEventType;
        public int hrEvent;
        public IMFPMediaPlayer pMediaPlayer;
        public MFP_MEDIAPLAYER_STATE eState;
        public IPropertyStore pPropertyStore;

        public IntPtr GetPtr()
        {
            IntPtr ip;

            int iSize = Marshal.SizeOf(this);

            ip = Marshal.AllocCoTaskMem(iSize);
            Marshal.StructureToPtr(this, ip, false);

            return ip;
        }

        public static MFP_EVENT_HEADER PtrToEH(IntPtr pNativeData)
        {
            MFP_EVENT_TYPE met = (MFP_EVENT_TYPE)Marshal.ReadInt32(pNativeData);
            object mce;

            switch (met)
            {
                case MFP_EVENT_TYPE.Play:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAY_EVENT));
                    break;
                case MFP_EVENT_TYPE.Pause:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PAUSE_EVENT));
                    break;
                case MFP_EVENT_TYPE.Stop:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_STOP_EVENT));
                    break;
                case MFP_EVENT_TYPE.PositionSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_POSITION_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.RateSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCreated:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CREATED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.FrameStep:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_FRAME_STEP_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCleared:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CLEARED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MF:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MF_EVENT));
                    break;
                case MFP_EVENT_TYPE.Error:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ERROR_EVENT));
                    break;
                case MFP_EVENT_TYPE.PlaybackEnded:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAYBACK_ENDED_EVENT));
                    break;

                case MFP_EVENT_TYPE.AcquireUserCredential:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ACQUIRE_USER_CREDENTIAL_EVENT));
                    break;

                default:
                    // Don't know what it is.  Send back the header.
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_EVENT_HEADER));
                    break;
            }

            return mce as MFP_EVENT_HEADER;
        }
    }

	#endregion

    #region Interfaces

	#endregion
}
