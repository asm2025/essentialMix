using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	public class BMMarshaler : ICustomMarshaler
	{
		private class MyProps
		{
			#region Data members

			public BitmapInfoHeader m_obj;
			public IntPtr m_ptr;

			private int m_InProcess;
			private bool m_IAllocated;
			private MyProps m_Parent;

			[ThreadStatic]
			private static MyProps m_CurrentProps;

			#endregion

			public int GetStage()
			{
				return m_InProcess;
			}

			public void StageComplete()
			{
				m_InProcess++;
			}

			public IntPtr Allocate()
			{
				IntPtr ip = m_obj.GetPtr();
				m_IAllocated = true;
				return ip;
			}

			[NotNull]
			public static MyProps AddLayer()
			{
				MyProps p = new MyProps
				{
					m_Parent = m_CurrentProps
				};
				m_CurrentProps = p;

				return p;
			}

			public static void SplitLayer()
			{
				MyProps t = AddLayer();
				MyProps p = t.m_Parent;

				t.m_InProcess = 1;
				t.m_ptr = p.m_ptr;
				t.m_obj = p.m_obj;

				p.m_InProcess = 1;
			}

			[NotNull]
			public static MyProps GetTop()
			{
				// If the member hasn't been initialized, do it now.  And no,
				// we can't do this in the constructor, since the constructor
				// may have been called on a different thread, and
				// m_CurrentProps is unique to each thread.
				return m_CurrentProps ??= new MyProps();
			}

			public void Clear()
			{
				if (m_IAllocated)
				{
					Marshal.FreeCoTaskMem(m_ptr);
					m_IAllocated = false;
				}

				// Never delete the last entry.
				if (m_Parent == null)
				{
					m_InProcess = 0;
					m_obj = null;
					m_ptr = IntPtr.Zero;
				}
				else
				{
					m_obj = null;
					m_CurrentProps = m_Parent;
				}
			}
		}

		public IntPtr MarshalManagedToNative(object managedObj)
		{
			MyProps t = MyProps.GetTop();

			switch (t.GetStage())
			{
				case 0:
				{
					t.m_obj = managedObj as BitmapInfoHeader;

					Debug.Assert(t.m_obj != null);
					Debug.Assert(t.m_obj.Size != 0);

					t.m_ptr = t.Allocate();

					break;
				}
				case 1:
				{
					if (!ReferenceEquals(t.m_obj, managedObj))
					{
						MyProps.AddLayer();

						// Try this call again now that we have fixed
						// m_CurrentProps.
						return MarshalManagedToNative(managedObj);
					}
					Marshal.StructureToPtr(managedObj, t.m_ptr, false);
					break;
				}
				case 2:
				{
					MyProps.SplitLayer();

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

		// Called just after invoking the COM method.  The IntPtr is the same
		// one that just got returned from MarshalManagedToNative.  The return
		// value is unused.
		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			MyProps t = MyProps.GetTop();

			switch (t.GetStage())
			{
				case 0:
				{
					t.m_obj = BitmapInfoHeader.PtrToBMI(pNativeData);
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
						// one. Therefore instead of being the second half
						// of a "Managed calling unmanaged" (as m_InProcess
						// led us to believe) this is really the first half
						// of a nested "Unmanaged calling managed" (see
						// Recursion in the comments at the top of this
						// class).  Add another layer.
						MyProps.AddLayer();

						// Try this call again now that we have fixed
						// m_CurrentProps.
						return MarshalNativeToManaged(pNativeData);
					}
					BitmapInfoHeader bmi = BitmapInfoHeader.PtrToBMI(pNativeData);

					t.m_obj.CopyFrom(bmi);
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
					MyProps.SplitLayer();

					// Try this call again now that we have fixed
					// m_CurrentProps.
					return MarshalNativeToManaged(pNativeData);
				}
				default:
				{
					Environment.FailFast("Something horrible has happened, probably due to marshaling of nested BMMarshaler calls.");
					break;
				}
			}
			t.StageComplete();

			return t.m_obj;
		}

		public void CleanUpManagedData(object ManagedObj)
		{
			MyProps t = MyProps.GetTop();
			t.Clear();
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			MyProps t = MyProps.GetTop();
			t.Clear();
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
			return new BMMarshaler();
		}
	}
}