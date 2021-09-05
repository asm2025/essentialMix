using System;
using System.Runtime.InteropServices;
using essentialMix.MediaFoundation.MFPlayer;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
	internal class EHMarshaler : ICustomMarshaler
	{
		public IntPtr MarshalManagedToNative(object managedObj)
		{
			MFP_EVENT_HEADER eh = (MFP_EVENT_HEADER)managedObj;
			IntPtr ip = eh.GetPtr();
			return ip;
		}

		// Called just after invoking the COM method.  The IntPtr is the same one that just got returned
		// from MarshalManagedToNative.  The return value is unused.
		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			// We should never get here.
			MFP_EVENT_HEADER eh = MFP_EVENT_HEADER.PtrToEH(pNativeData);
			return eh;
		}

		public void CleanUpManagedData(object ManagedObj)
		{
			// Never called.
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

		// This method is called by interop to create the custom marshaler.  The (optional)
		// cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
		[NotNull]
		private static ICustomMarshaler GetInstance(string cookie)
		{
			return new EHMarshaler();
		}
	}
}