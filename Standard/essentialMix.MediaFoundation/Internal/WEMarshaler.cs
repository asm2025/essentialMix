using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
	// Class to handle WAVEFORMATEXTENSIBLE.  While it is used for both [In]
    // and [Out], it is never used for [In, Out], which means it has no data
    // members (which makes life much simpler).
    internal class WEMarshaler : ICustomMarshaler
    {
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            WaveFormatEx wfe = (WaveFormatEx)managedObj;
            IntPtr ip = wfe.GetPtr();
            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same
        // one that just got returned from MarshalManagedToNative.  The return
        // value is unused.
        [NotNull]
		public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            WaveFormatEx wfe = WaveFormatEx.PtrToWave(pNativeData);

            return wfe;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.
        // The (optional) cookie is the value specified in MarshalCookie="xxx",
        // or "" if none is specified.
        [NotNull]
		private static ICustomMarshaler GetInstance(string cookie)
        {
            return new WEMarshaler();
        }
    }

	// Used only by IMFPMediaPlayerCallback, where it only uses [In].
}
