using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
    // PVMarshaler - Class to marshal PropVariants on parameters that
    // *output* PropVariants.

    // When defining parameters that use this marshaler, you must always
    // declare them as both [In] and [Out].  This will result in *both*
    // MarshalManagedToNative and MarshalNativeToManaged being called.  Since
    // the order they are called depends on exactly what's happening,
    // m_InProcess lets us know which way things are being called.
    //
    // Managed calling unmanaged: 
    // In this case, MarshalManagedToNative is called first with m_InProcess 
    // == 0.  When MarshalManagedToNative is called, we store the managed
    // object (so we know where to copy it back to), then we clear the variant,
    // allocate some COM memory and pass a pointer to the COM memory to the
    // native code.  When the native code is done, MarshalNativeToManaged gets
    // called (m_InProcess == 1) with the pointer to the COM memory.  At that
    // point, we copy the contents back into the (saved) managed object. After
    // that, CleanUpNativeData gets called and we release the COM memory.
    //
    // Unmanaged calling managed:
    // In this case, MarshalNativeToManaged is called first.  We store the
    // native pointer (so we know where to copy the result back to), then we
    // create a managed PropVariant and copy the native value into it.  When
    // the managed code is done, MarshalManagedToNative gets called with the
    // managed PropVariant we created.  At that point, we copy the contents
    // of the managed PropVariant back into the (saved) native pointer.
    //
    // Multi-threading:
    // When marshaling from managed to native, the first thing that happens
    // is the .Net creates an instance of the PVMarshaler class.  It then
    // calls MarshalManagedToNative to send you the managed object into which
    // the unmanaged data will eventually be stored. However it doesn't pass
    // the managed object again when MarshalNativeToManaged eventually gets
    // called.  No problem, you assume, I'll just store it as a data member
    // and use it when MarshalNativeToManaged get called.  Yeah, about that...
    // First of all, if several threads all start calling a method that uses
    // PVMarshaler, .Net WILL create multiple instances of this class.
    // However, it will then DESTRUCT all of them except 1, which it will use
    // from every thread.  Unless you are aware of this behavior and take
    // precautions, having multiple thread using the same instance results in
    // chaos.
    // Also be aware that if two different methods both use PVMarshaler (say
    // GetItem and GetItemByIndex on IMFAttributes), .Net may use the same
    // instance for both methods.  Unless they each have a unique MarshalCookie
    // string.  Using a unique MarshalCookie doesn't help with multi-threading,
    // but it does help keep the marshaling from one method call from
    // interfering with another.
    //
    // Recursion:
    // If managed code calls unmanaged code thru PVMarshaler, then that
    // unmanaged code in turn calls IMFAttribute::GetItem against a managed
    // object, what happens?  .Net will use a single instance of PVMarshaler to
    // handle both calls, even if the actual PropVariant used for the second
    // call is a different instance of the PropVariant class.  It can also use
    // the same managed thread id all the way thru (so using a simple
    // ThreadStatic is not sufficient to keep track of this).  So if you see a
    // call to MarshalNativeToManaged right after a call to
    // MarshalManagedToNative, it might be the second half of a 'normal' bit of
    // marshaling, or it could be the start of a nested call from unmanaged
    // back into managed.
    // There are 2 ways to detect nesting:
    // 1) If the pNativeData sent to MarshalNativeToManaged *isn't* the one
    // you returned from MarshalManagedToNative, you are nesting.
    // 2) m_InProcess starts at 0.  MarshalManagedToNative increments it to 1.
    // Then MarshalNativeToManaged increments it to 2.  For non-nested, that
    // should be the end.  So if MarshalManagedToNative gets called with
    // m_InProcess == 2, we are nesting.
    //
    // Warning!  You cannot assume that both marshaling routines will always
    // get called.  For example if calling from unmanaged to managed,
    // MarshalNativeToManaged will get called, but if the managed code throws
    // an exception, MarshalManagedToNative will not.  This can be a problem
    // since .Net REUSES instances of the marshaler.  So it is essential that
    // class members always get cleaned up in CleanUpManagedData &
    // CleanUpNativeData.
    //
    // All this helps explain the otherwise inexplicable complexity of this
    // class:  It uses a ThreadStatic variable to keep instance data from one
    // thread from interfering with another thread, nests instances of MyProps,
    // and uses 2 different methods to check for recursion (which in theory
    // could be nested quite deeply).
	internal class PVMarshaler : ICustomMarshaler
	{
		private class MyProps
		{
			public Variant m_obj;
			public IntPtr m_ptr;

			private int m_InProcess;
			private bool m_IAllocated;
			private MyProps m_Parent;

			[ThreadStatic]
			private static MyProps[] m_CurrentProps;

			public int GetStage()
			{
				return m_InProcess;
			}

			public void StageComplete()
			{
				m_InProcess++;
			}

			[NotNull]
			public static MyProps AddLayer(int iIndex)
			{
				MyProps p = new MyProps
				{
					m_Parent = m_CurrentProps[iIndex]
				};
				m_CurrentProps[iIndex] = p;

				return p;
			}

			public static void SplitLayer(int iIndex)
			{
				MyProps t = AddLayer(iIndex);
				MyProps p = t.m_Parent;

				t.m_InProcess = 1;
				t.m_ptr = p.m_ptr;
				t.m_obj = p.m_obj;

				p.m_InProcess = 1;
			}

			public static MyProps GetTop(int iIndex)
			{
				// If the member hasn't been initialized, do it now.  And no, we can't
				// do this in the PVMarshaler constructor, since the constructor may 
				// have been called on a different thread.
				if (m_CurrentProps == null)
				{
					m_CurrentProps = new MyProps[MaxArgs];
					for (int x = 0; x < MaxArgs; x++)
					{
						m_CurrentProps[x] = new MyProps();
					}
				}
				return m_CurrentProps[iIndex];
			}

			public void Clear(int iIndex)
			{
				if (m_IAllocated)
				{
					Marshal.FreeCoTaskMem(m_ptr);
					m_IAllocated = false;
				}
				if (m_Parent == null)
				{
					// Never delete the last entry.
					m_InProcess = 0;
					m_obj = null;
					m_ptr = IntPtr.Zero;
				}
				else
				{
					m_obj = null;
					m_CurrentProps[iIndex] = m_Parent;
				}
			}

			public IntPtr Alloc(int iSize)
			{
				IntPtr ip = Marshal.AllocCoTaskMem(iSize);
				m_IAllocated = true;
				return ip;
			}
		}

		private readonly int m_Index;

		// Max number of arguments in a single method call that can use
		// PVMarshaler.
		private const int MaxArgs = 2;

		private PVMarshaler([NotNull] string cookie)
		{
			int iLen = cookie.Length;

			// On methods that have more than 1 PVMarshaler on a
			// single method, the cookie is in the form:
			// InterfaceName.MethodName.0 & InterfaceName.MethodName.1.
			if (cookie[iLen - 2] != '.')
			{
				m_Index = 0;
			}
			else
			{
				m_Index = int.Parse(cookie.Substring(iLen - 1));
				Debug.Assert(m_Index < MaxArgs);
			}
		}

		public IntPtr MarshalManagedToNative([NotNull] object managedObj)
		{
			// Nulls don't invoke custom marshaling.
			Debug.Assert(managedObj != null);

			MyProps t = MyProps.GetTop(m_Index);

			switch (t.GetStage())
			{
				case 0:
				{
					// We are just starting a "Managed calling unmanaged"
					// call.

					// Cast the object back to a PropVariant and save it
					// for use in MarshalNativeToManaged.
					t.m_obj = managedObj as Variant;

					// This could happen if (somehow) managedObj isn't a
					// PropVariant.  During normal marshaling, the custom
					// marshaler doesn't get called if the parameter is
					// null.
					Debug.Assert(t.m_obj != null);

					// Release any memory currently allocated in the
					// PropVariant.  In theory, the (managed) caller
					// should have done this before making the call that
					// got us here, but .Net programmers don't generally
					// think that way.  To avoid any leaks, do it for them.
					t.m_obj?.Clear();

					// Create an appropriately sized buffer (varies from
					// x86 to x64).
					int iSize = GetNativeDataSize();
					t.m_ptr = t.Alloc(iSize);

					// Copy in the (empty) PropVariant.  In theory we could
					// just zero out the first 2 bytes (the VariantType),
					// but since PropVariantClear wipes the whole struct,
					// that's what we do here to be safe.
					Marshal.StructureToPtr(t.m_obj, t.m_ptr, false);

					break;
				}
				case 1:
				{
					if (!ReferenceEquals(t.m_obj, managedObj))
					{
						// If we get here, we have already received a call
						// to MarshalNativeToManaged where we created a
						// PropVariant and stored it into t.m_obj.  But
						// the object we just got passed here isn't the
						// same one.  Therefore instead of being the second
						// half of an "Unmanaged calling managed" (as
						// m_InProcess led us to believe), this is really
						// the first half of a nested "Managed calling
						// unmanaged" (see Recursion in the comments at the
						// top of this class).  Add another layer.
						MyProps.AddLayer(m_Index);

						// Try this call again now that we have fixed
						// m_CurrentProps.
						return MarshalManagedToNative(managedObj);
					}

					// This is (probably) the second half of "Unmanaged
					// calling managed."  However, it could be the first
					// half of a nested usage of PropVariants.  If it is a
					// nested, we'll eventually figure that out in case 2.

					// Copy the data from the managed object into the
					// native pointer that we received in
					// MarshalNativeToManaged.
					Marshal.StructureToPtr(t.m_obj, t.m_ptr, false);

					break;
				}
				case 2:
				{
					// Apparently this is 'part 3' of a 2 part call.  Which
					// means we are doing a nested call.  Normally we would
					// catch the fact that this is a nested call with the
					// ReferenceEquals check above.  However, if the same
					// PropVariant instance is being passed thru again, we
					// end up here.
					// So, add a layer.
					MyProps.SplitLayer(m_Index);

					// Try this call again now that we have fixed
					// m_CurrentProps.
					return MarshalManagedToNative(managedObj);
				}
				default:
				{
					Environment.FailFast("Something horrible has happened, probably due to marshaling of nested PropVariant calls.");
					break;
				}
			}
			t.StageComplete();

			return t.m_ptr;
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			// Nulls don't invoke custom marshaling.
			Debug.Assert(pNativeData != IntPtr.Zero);

			MyProps t = MyProps.GetTop(m_Index);

			switch (t.GetStage())
			{
				case 0:
				{
					// We are just starting a "Unmanaged calling managed"
					// call.

					// Caller should have cleared variant before calling
					// us.  Might be acceptable for types *other* than
					// IUnknown, String, Blob and StringArray, but it is
					// still bad design.  We're checking for it, but we
					// work around it.

					// Read the 16bit VariantType.
					Debug.Assert(Marshal.ReadInt16(pNativeData) == 0);

					// Create an empty managed PropVariant without using
					// pNativeData.
					t.m_obj = new Variant();

					// Save the pointer for use in MarshalManagedToNative.
					t.m_ptr = pNativeData;

					break;
				}
				case 1:
				{
					if (t.m_ptr != pNativeData)
					{
						// If we get here, we have already received a call
						// to MarshalManagedToNative where we created an
						// IntPtr and stored it into t.m_ptr.  But the
						// value we just got passed here isn't the same
						// one.  Therefore instead of being the second half
						// of a "Managed calling unmanaged" (as m_InProcess
						// led us to believe) this is really the first half
						// of a nested "Unmanaged calling managed" (see
						// Recursion in the comments at the top of this
						// class).  Add another layer.
						MyProps.AddLayer(m_Index);

						// Try this call again now that we have fixed
						// m_CurrentProps.
						return MarshalNativeToManaged(pNativeData);
					}

					// This is (probably) the second half of "Managed
					// calling unmanaged."  However, it could be the first
					// half of a nested usage of PropVariants.  If it is a
					// nested, we'll eventually figure that out in case 2.

					// Copy the data from the native pointer into the
					// managed object that we received in
					// MarshalManagedToNative.
					Marshal.PtrToStructure(pNativeData, t.m_obj);

					break;
				}
				case 2:
				{
					// Apparently this is 'part 3' of a 2 part call.  Which
					// means we are doing a nested call.  Normally we would
					// catch the fact that this is a nested call with the
					// (t.m_ptr != pNativeData) check above.  However, if
					// the same PropVariant instance is being passed thru
					// again, we end up here.  So, add a layer.
					MyProps.SplitLayer(m_Index);

					// Try this call again now that we have fixed
					// m_CurrentProps.
					return MarshalNativeToManaged(pNativeData);
				}
				default:
				{
					Environment.FailFast("Something horrible has happened, probably due to marshaling of nested PropVariant calls.");
					break;
				}
			}
			t.StageComplete();

			return t.m_obj;
		}

		public void CleanUpManagedData(object ManagedObj)
		{
			// Note that if there are nested calls, one of the Cleanup*Data
			// methods will be called at the end of each pair:

			// MarshalNativeToManaged
			// MarshalManagedToNative
			// CleanUpManagedData
			//
			// or for recursion:
			//
			// MarshalManagedToNative 1
			// MarshalNativeToManaged 2
			// MarshalManagedToNative 2
			// CleanUpManagedData     2
			// MarshalNativeToManaged 1
			// CleanUpNativeData      1
            
			// Clear() either pops an entry, or clears
			// the values for the next call.
			MyProps t = MyProps.GetTop(m_Index);
			t.Clear(m_Index);
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			// Clear() either pops an entry, or clears
			// the values for the next call.
			MyProps t = MyProps.GetTop(m_Index);
			t.Clear(m_Index);
		}

		// The number of bytes to marshal.  Size varies between x86 and x64.
		public int GetNativeDataSize()
		{
			return Marshal.SizeOf(typeof(Variant));
		}

		// This method is called by interop to create the custom marshaler.
		// The (optional) cookie is the value specified in
		// MarshalCookie="asdf", or "" if none is specified.
		[NotNull]
		private static ICustomMarshaler GetInstance([NotNull] string cookie)
		{
			return new PVMarshaler(cookie);
		}
	}
}