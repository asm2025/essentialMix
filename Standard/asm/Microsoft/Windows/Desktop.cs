using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using asm.Extensions;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace Microsoft.Windows
{
	/// <summary>
	/// Encapsulates the Desktop API.
	/// </summary>
	public class Desktop : Disposable, IDisposable, ICloneable
	{
		public struct Window
		{
			/// <summary>
			/// Creates a new window object.
			/// </summary>
			/// <param name="handle">Window handle.</param>
			/// <param name="text">Window title.</param>
			public Window(IntPtr handle, string text)
			{
				Handle = handle;
				Text = text;
			}

			/// <summary>
			/// Gets the window handle.
			/// </summary>
			public IntPtr Handle { get; }

			/// <summary>
			/// Gets teh window title.
			/// </summary>
			public string Text { get; }
		}

		/// <inheritdoc />
		/// <summary>
		/// A collection for Window objects.
		/// </summary>
		public class WindowCollection : CollectionBase
		{
			/// <summary>
			/// Gets a window from teh collection.
			/// </summary>
			public Window this[int index] => (Window)List[index];

			/// <summary>
			/// Adds a window to the collection.
			/// </summary>
			/// <param name="wnd">Window to add.</param>
			public void Add(Window wnd)
			{
				// adds a widow to the collection.
				List.Add(wnd);
			}
		}

		private static volatile StringCollection _stringCollection;

		/// <summary>
		/// Opens the default desktop.
		/// </summary>
		public static readonly Desktop DEFAULT = OpenDefaultDesktop();

		/// <summary>
		/// Opens the desktop the user if viewing.
		/// </summary>
		public static readonly Desktop INPUT = OpenInputDesktop();

		private readonly ArrayList _windows = new ArrayList();

		private IntPtr _desktop = IntPtr.Zero;

		/// <summary>
		/// Creates a new Desktop object.
		/// </summary>
		public Desktop()
		{
		}

		// constructor is private to prevent invalid handles being passed to it.
		private Desktop(IntPtr desktop)
		{
			DesktopName = GetDesktopName(desktop);
		}

		~Desktop()
		{
			// clean up, close the desktop.
			Close();
		}

		/// <summary>
		/// Gets the desktop name.
		/// </summary>
		/// <returns>The desktop name, or a blank string if no desktop open.</returns>
		public override string ToString() { return DesktopName; }

		protected override void Dispose(bool disposing)
		{
			if (disposing) Close();
			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets if a desktop is open.
		/// </summary>
		public bool IsOpen => !_desktop.IsZero();

		/// <summary>
		/// Gets the name of the desktop, returns null if no desktop is open.
		/// </summary>
		public string DesktopName { get; private set; } = string.Empty;

		/// <summary>
		/// Gets a handle to the desktop, IntPtr.Zero if no desktop open.
		/// </summary>
		public IntPtr DesktopHandle => _desktop;

		/// <summary>
		/// Creates a new desktop.  If a handle is open, it will be closed.
		/// </summary>
		/// <param name="name">The name of the new desktop.  Must be unique, and is case sensitive.</param>
		/// <returns>True if desktop was successfully created, otherwise false.</returns>
		public bool Create(string name)
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// close the open desktop.
			if (_desktop != IntPtr.Zero)
			{
				// attempt to close the desktop.
				if (!Close()) return false;
			}

			// make sure desktop doesn't already exist.
			if (Exists(name))
			{
				// it exists, so open it.
				return Open(name);
			}

			// attempt to create desktop.
			_desktop = asm.Win32.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, asm.Win32.DesktopAccessRightsEnum.DESKTOP_ALL, IntPtr.Zero);

			DesktopName = name;

			// something went wrong.
			return _desktop != IntPtr.Zero;
		}

		/// <summary>
		/// Closes the handle to a desktop.
		/// </summary>
		/// <returns>True if an open handle was successfully closed.</returns>
		public bool Close()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// check there is a desktop open.
			if (_desktop != IntPtr.Zero)
			{
				// close the desktop.
				bool result = asm.Win32.CloseDesktop(_desktop);

				if (result)
				{
					_desktop = IntPtr.Zero;

					DesktopName = string.Empty;
				}

				return result;
			}

			// no desktop was open, so desktop is closed.
			return true;
		}

		/// <summary>
		/// Opens a desktop.
		/// </summary>
		/// <param name="name">The name of the desktop to open.</param>
		/// <returns>True if the desktop was successfully opened.</returns>
		public bool Open(string name)
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// close the open desktop.
			if (_desktop != IntPtr.Zero)
			{
				// attempt to close the desktop.
				if (!Close()) return false;
			}

			// open the desktop.
			_desktop = asm.Win32.OpenDesktop(name, 0, true, asm.Win32.DesktopAccessRightsEnum.DESKTOP_ALL);

			// something went wrong.
			if (_desktop == IntPtr.Zero) return false;

			DesktopName = name;

			return true;
		}

		/// <summary>
		/// Opens the current input desktop.
		/// </summary>
		/// <returns>True if the desktop was successfully opened.</returns>
		public bool OpenInput()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// close the open desktop.
			if (_desktop != IntPtr.Zero)
			{
				// attempt to close the desktop.
				if (!Close()) return false;
			}

			// open the desktop.
			_desktop = asm.Win32.OpenInputDesktop(0, true, asm.Win32.DesktopAccessRightsEnum.DESKTOP_ALL);

			// something went wrong.
			if (_desktop == IntPtr.Zero) return false;

			// get the desktop name.
			DesktopName = GetDesktopName(_desktop);

			return true;
		}

		/// <summary>
		/// Switches input to the currently opened desktop.
		/// </summary>
		/// <returns>True if desktops were successfully switched.</returns>
		public bool Show()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// make sure there is a desktop to open.
			if (_desktop == IntPtr.Zero) return false;

			// attempt to switch desktops.
			bool result = asm.Win32.SwitchDesktop(_desktop);

			return result;
		}

		/// <summary>
		/// Enumerates the windows on a desktop.
		/// </summary>
		/// <returns>A window collection if successful, otherwise null.</returns>
		public WindowCollection GetWindows()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// make sure a desktop is open.
			if (!IsOpen) return null;

			// init the array list.
			_windows.Clear();

			// get windows.
			bool result = asm.Win32.EnumDesktopWindows(_desktop, DesktopWindowsProc, IntPtr.Zero);

			// check for error.
			if (!result) return null;

			// get window names.
			WindowCollection windows = new WindowCollection();
			StringBuilder sb = new StringBuilder(asm.Win32.MAX_WINDOW_NAME_LENGTH);

			foreach (IntPtr wnd in _windows)
			{
				asm.Win32.GetWindowText(wnd, sb, sb.Capacity);
				windows.Add(new Window(wnd, sb.ToString()));
			}

			return windows;
		}

		/// <summary>
		/// Creates a new process in a desktop.
		/// </summary>
		/// <param name="path">Path to application.</param>
		/// <returns>The process object for the newly created process.</returns>
		public Process CreateProcess(string path)
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// make sure a desktop is open.
			if (!IsOpen) return null;

			asm.Win32.SECURITY_ATTRIBUTES pa = new asm.Win32.SECURITY_ATTRIBUTES();
			asm.Win32.SECURITY_ATTRIBUTES ta = new asm.Win32.SECURITY_ATTRIBUTES();
			asm.Win32.STARTUPINFO si = new asm.Win32.STARTUPINFO {lpDesktop = DesktopName};

			// start the process.
			bool result = asm.Win32.CreateProcess(null, path, ref pa, ref ta, true, asm.Win32.CreateProcessFlagsEnum.NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref si,
												out asm.Win32.PROCESS_INFORMATION pi);
			return !result ? null : Process.GetProcessById(pi.dwProcessId);
		}

		/// <summary>
		/// Prepares a desktop for use.  For use only on newly created desktops, call straight after CreateDesktop.
		/// </summary>
		public void Prepare()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			// make sure a desktop is open.
			if (IsOpen)
			{
				// load explorer.
				CreateProcess("explorer.exe");
			}
		}

		/// <summary>
		/// Creates a new Desktop object with the same desktop open.
		/// </summary>
		/// <returns>Cloned desktop object.</returns>
		public object Clone()
		{
			// make sure object isn't disposed.
			ThrowIfDisposed();

			Desktop desktop = new Desktop();

			// if a desktop is open, make the clone open it.
			if (IsOpen) desktop.Open(DesktopName);

			return desktop;
		}

		private bool DesktopWindowsProc(IntPtr wndHandle, IntPtr lParam)
		{
			// add window handle to collection.
			_windows.Add(wndHandle);

			return true;
		}

		/// <summary>
		/// Enumerates all of the desktops.
		/// </summary>
		/// <returns>True if desktop names were successfully enumerated.</returns>
		[NotNull]
		public static string[] GetDesktops()
		{
			// attempt to enum desktops.
			IntPtr windowStation = asm.Win32.GetProcessWindowStation();

			// check we got a valid handle.
			if (windowStation == IntPtr.Zero) return new string[0];

			string[] desktops;

			// lock the object. thread safety and all.
			lock(_stringCollection = new StringCollection())
			{
				bool result = asm.Win32.EnumDesktops(windowStation, DesktopProc, IntPtr.Zero);

				// something went wrong.
				if (!result) return new string[0];

				//	// turn the collection into an array.
				desktops = new string[_stringCollection.Count];
				for (int i = 0; i < desktops.Length; i++) desktops[i] = _stringCollection[i];
			}

			return desktops;
		}

		/// <summary>
		/// Switches to the specified desktop.
		/// </summary>
		/// <param name="name">Name of desktop to switch input to.</param>
		/// <returns>True if desktops were successfully switched.</returns>
		public static bool Show(string name)
		{
			// attempt to open desktop.
			bool result;

			using (Desktop d = new Desktop())
			{
				result = d.Open(name);

				// something went wrong.
				if (!result) return false;

				// attempt to switch desktops.
				result = d.Show();
			}

			return result;
		}

		/// <summary>
		/// Gets the desktop of the calling thread.
		/// </summary>
		/// <returns>Returns a Desktop object for the calling thread.</returns>
		[NotNull]
		public static Desktop GetCurrent() { return new Desktop(asm.Win32.GetThreadDesktop(Thread.CurrentThread.ManagedThreadId)); }

		/// <summary>
		/// Sets the desktop of the calling thread.
		/// NOTE: Function will fail if thread has hooks or windows in the current desktop.
		/// </summary>
		/// <param name="desktop">Desktop to put the thread in.</param>
		/// <returns>True if the threads desktop was successfully changed.</returns>
		public static bool SetCurrent([NotNull] Desktop desktop) { return desktop.IsOpen && asm.Win32.SetThreadDesktop(desktop.DesktopHandle); }

		/// <summary>
		/// Opens a desktop.
		/// </summary>
		/// <param name="name">The name of the desktop to open.</param>
		/// <returns>If successful, a Desktop object, otherwise, null.</returns>
		public static Desktop OpenDesktop(string name)
		{
			// open the desktop.
			Desktop desktop = new Desktop();
			bool result = desktop.Open(name);

			// something went wrong.
			return !result ? null : desktop;
		}

		/// <summary>
		/// Opens the current input desktop.
		/// </summary>
		/// <returns>If successful, a Desktop object, otherwise, null.</returns>
		public static Desktop OpenInputDesktop()
		{
			// open the desktop.
			Desktop desktop = new Desktop();
			bool result = desktop.OpenInput();

			// something went wrong.
			return !result ? null : desktop;
		}

		/// <summary>
		/// Opens the default desktop.
		/// </summary>
		/// <returns>If successful, a Desktop object, otherwise, null.</returns>
		public static Desktop OpenDefaultDesktop() { return OpenDesktop("Default"); }

		/// <summary>
		/// Creates a new desktop.
		/// </summary>
		/// <param name="name">The name of the desktop to create.  Names are case sensitive.</param>
		/// <returns>If successful, a Desktop object, otherwise, null.</returns>
		public static Desktop CreateDesktop(string name)
		{
			// open the desktop.
			Desktop desktop = new Desktop();
			bool result = desktop.Create(name);

			// something went wrong.
			return !result ? null : desktop;
		}

		/// <summary>
		/// Gets the name of a given desktop.
		/// </summary>
		/// <param name="desktop">Desktop object whose name is to be found.</param>
		/// <returns>If successful, the desktop name, otherwise, null.</returns>
		public static string GetDesktopName([NotNull] Desktop desktop)
		{
			return desktop.IsOpen
						? null
						: GetDesktopName(desktop.DesktopHandle);
		}

		/// <summary>
		/// Gets the name of a desktop from a desktop handle.
		/// </summary>
		/// <param name="desktopHandle"></param>
		/// <returns>If successful, the desktop name, otherwise, null.</returns>
		public static string GetDesktopName(IntPtr desktopHandle)
		{
			// check its not a null pointer.
			// null pointers wont work.
			if (desktopHandle == IntPtr.Zero) return null;

			// get the length of the name.
			int needed = 0;
			asm.Win32.GetUserObjectInformation(desktopHandle, asm.Win32.UOI_NAME, IntPtr.Zero, 0, ref needed);

			// get the name.
			IntPtr ptr = Marshal.AllocHGlobal(needed);
			bool result = asm.Win32.GetUserObjectInformation(desktopHandle, asm.Win32.UOI_NAME, ptr, needed, ref needed);
			string name = Marshal.PtrToStringAnsi(ptr);
			Marshal.FreeHGlobal(ptr);

			// something went wrong.
			return !result ? null : name;
		}

		/// <summary>
		/// Checks if the specified desktop exists (using a case sensitive search).
		/// </summary>
		/// <param name="name">The name of the desktop.</param>
		/// <returns>True if the desktop exists, otherwise false.</returns>
		public static bool Exists(string name) { return Exists(name, false); }

		/// <summary>
		/// Checks if the specified desktop exists.
		/// </summary>
		/// <param name="name">The name of the desktop.</param>
		/// <param name="caseInsensitive">If the search is case Insensitive.</param>
		/// <returns>True if the desktop exists, otherwise false.</returns>
		public static bool Exists(string name, bool caseInsensitive)
		{
			// enumerate desktops.
			string[] desktops = GetDesktops();

			// return true if desktop exists.
			foreach (string desktop in desktops)
			{
				if (caseInsensitive)
				{
					// case insensitive, compare all in lower case.
					if (desktop.ToLower() == name.ToLower()) return true;
				}
				else
				{
					if (desktop == name) return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a new process on the specified desktop.
		/// </summary>
		/// <param name="path">Path to application.</param>
		/// <param name="desktop">Desktop name.</param>
		/// <returns>A Process object for the newly created process, otherwise, null.</returns>
		public static Process CreateProcess(string path, string desktop)
		{
			if (!Exists(desktop)) return null;

			// create the process.
			Desktop d = OpenDesktop(desktop);
			return d.CreateProcess(path);
		}

		/// <summary>
		/// Gets an array of all the processes running on the Input desktop.
		/// </summary>
		/// <returns>An array of the processes.</returns>
		[NotNull]
		public static Process[] GetInputProcesses()
		{
			// get all processes.
			Process[] processes = Process.GetProcesses();

			ArrayList mProc = new ArrayList();

			// get the current desktop name.
			string currentDesktop = GetDesktopName(INPUT.DesktopHandle);

			// cycle through the processes.
			foreach (Process process in processes)
			{
				// check the threads of the process - are they in this one?
				foreach (ProcessThread pt in process.Threads)
				{
					// check for a desktop name match.
					if (GetDesktopName(asm.Win32.GetThreadDesktop(pt.Id)) == currentDesktop)
					{
						// found a match, add to list, and bail.
						mProc.Add(process);
						break;
					}
				}
			}

			// put ArrayList into array.
			Process[] inputProcesses = new Process[mProc.Count];

			for (int i = 0; i < inputProcesses.Length; i++)
				inputProcesses[i] = (Process)mProc[i];

			return inputProcesses;
		}

		private static bool DesktopProc(string lpszDesktop, IntPtr lParam)
		{
			// add the desktop to the collection.
			_stringCollection.Add(lpszDesktop);

			return true;
		}
	}
}