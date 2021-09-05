using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using essentialMix.MediaFoundation.Transform;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
	// Used (only) by MFAPI.MFTGetInfo.  In order to perform the marshaling,
	// we need to have the pointer to the array, and the number of elements. To
	// receive all this information in the marshaler, we are using the same
	// instance of this class for multiple parameters.  So ppInputTypes &
	// pcInputTypes share an instance, and ppOutputTypes & pcOutputTypes share
	// an instance.  To make life interesting, we also need to work correctly
	// if invoked on multiple threads at the same time.
	internal class RTIMarshaler : ICustomMarshaler
	{
		private struct MyProps
		{
			public ArrayList m_array;
			public MFInt m_int;
			public IntPtr m_MFIntPtr;
			public IntPtr m_ArrayPtr;
		}

		// When used with MFAPI.MFTGetInfo, there are 2 parameter pairs
		// (ppInputTypes + pcInputTypes, ppOutputTypes + pcOutputTypes).  Each
		// need their own instance, so s_Props is a 2 element array.
		[ThreadStatic]
		private static MyProps[] s_Props;

		// Used to indicate the index of s_Props we should be using.  It is
		// derived from the MarshalCookie.
		private readonly int m_Cookie;

		private RTIMarshaler([NotNull] string cookie)
		{
			m_Cookie = int.Parse(cookie);
		}

		public IntPtr MarshalManagedToNative(object managedObj)
		{
			IntPtr p;

			// s_Props is thread static, so we don't need to worry about
			// locking.  And since the only method that RTIMarshaler supports
			// is MFAPI.MFTGetInfo, we know that MarshalManagedToNative gets
			// called first.
			s_Props ??= new MyProps[2];

			// We get called twice: Once for the MFInt, and once for the array.
			// Figure out which call this is.
			if (managedObj is MFInt obj)
			{
				// Save off the object.  We'll need to use Assign() on this
				// later.
				s_Props[m_Cookie].m_int = obj;

				// Allocate room for the int and set it to zero;
				p = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MFInt)));
				Marshal.WriteInt32(p, 0);

				s_Props[m_Cookie].m_MFIntPtr = p;
			}
			else // Must be the array.  FYI: Nulls don't get marshaled.
			{
				// Save off the object.  We'll be calling methods on this in
				// MarshalNativeToManaged.
				s_Props[m_Cookie].m_array = managedObj as ArrayList;

				s_Props[m_Cookie].m_array?.Clear();

				// All we need is room for the pointer
				p = Marshal.AllocCoTaskMem(IntPtr.Size);

				// Belt-and-suspenders.  Set this to null.
				Marshal.WriteIntPtr(p, IntPtr.Zero);

				s_Props[m_Cookie].m_ArrayPtr = p;
			}

			return p;
		}

		// We have the MFInt and the array pointer.  Populate the array.
		private static void Parse(MyProps p)
		{
			// If we have an array to return things in (ie MFTGetInfo wasn't
			// passed nulls).  Note that the MFInt doesn't get set in that
			// case.
			if (p.m_array != null)
			{
				// Read the count
				int count = Marshal.ReadInt32(p.m_MFIntPtr);
				p.m_int.Assign(count);

				IntPtr ip2 = Marshal.ReadIntPtr(p.m_ArrayPtr);

				// I don't know why this might happen, but it seems worth the
				// check.
				if (ip2 != IntPtr.Zero)
				{
					try
					{
						int iSize = Marshal.SizeOf(typeof(MFTRegisterTypeInfo));
						IntPtr pos = ip2;

						// Size the array
						p.m_array.Capacity = count;

						// Copy in the values
						for (int x = 0; x < count; x++)
						{
							MFTRegisterTypeInfo rti = new MFTRegisterTypeInfo();
							Marshal.PtrToStructure(pos, rti);
							pos += iSize;
							p.m_array.Add(rti);
						}
					}
					finally
					{
						// Free the array we got back
						Marshal.FreeCoTaskMem(ip2);
					}
				}
			}
		}

		// Called just after invoking the COM method.  The IntPtr is the same
		// one that just got returned from MarshalManagedToNative.  The return
		// value is unused.
		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			Debug.Assert(s_Props != null);

			// Figure out which (if either) of the MFInts this is.
			for (int x = 0; x < 2; x++)
			{
				if (pNativeData == s_Props[x].m_MFIntPtr)
				{
					Parse(s_Props[x]);
					break;
				}
			}

			// This value isn't actually used
			return null;
		}

		public void CleanUpManagedData(object ManagedObj)
		{
			// Never called.
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			if (s_Props[m_Cookie].m_MFIntPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(s_Props[m_Cookie].m_MFIntPtr);

				s_Props[m_Cookie].m_MFIntPtr = IntPtr.Zero;
				s_Props[m_Cookie].m_int = null;
			}
			if (s_Props[m_Cookie].m_ArrayPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(s_Props[m_Cookie].m_ArrayPtr);

				s_Props[m_Cookie].m_ArrayPtr = IntPtr.Zero;
				s_Props[m_Cookie].m_array = null;
			}
		}

		// The number of bytes to marshal out
		public int GetNativeDataSize()
		{
			return -1;
		}

		// This method is called by interop to create the custom marshaler.
		// The (optional) cookie is the value specified in MarshalCookie="xxx",
		// or "" if none is specified.
		[NotNull]
		private static ICustomMarshaler GetInstance([NotNull] string cookie)
		{
			return new RTIMarshaler(cookie);
		}
	}
}