using System;
using System.Runtime.InteropServices;
using essentialMix.MediaFoundation.Transform;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
	// Used by MFTRegister.  Note that since it only marshals [In],
	// the class has no members, which makes life much simpler.
	internal class RTAMarshaler : ICustomMarshaler
	{
		public IntPtr MarshalManagedToNative(object managedObj)
		{
			int iSize = Marshal.SizeOf(typeof(MFTRegisterTypeInfo));

			MFTRegisterTypeInfo[] array = managedObj as MFTRegisterTypeInfo[];

			IntPtr p = Marshal.AllocCoTaskMem(array.Length * iSize);
			IntPtr t = p;

			for (int x = 0; x < array.Length; x++)
			{
				Marshal.StructureToPtr(array[x], t, false);
				t += iSize;
			}

			return p;
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			// This value isn't actually used
			return null;
		}

		public void CleanUpManagedData(object ManagedObj)
		{
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			Marshal.FreeCoTaskMem(pNativeData);
		}

		// The number of bytes to marshal out
		public int GetNativeDataSize()
		{
			return -1;
		}

		// This method is called by interop to create the custom marshaler.
		// The (optional) cookie is the value specified in MarshalCookie="xxx"
		// or "" if none is specified.
		[NotNull]
		private static ICustomMarshaler GetInstance(string cookie)
		{
			return new RTAMarshaler();
		}
	}
}