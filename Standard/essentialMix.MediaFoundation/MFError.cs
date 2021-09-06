using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using essentialMix.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	public class MFError
	{
		private const string MESSAGE_FILE = "mferror.dll";

		#region externs

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", ExactSpelling = true, SetLastError = true)]
		[SecurityCritical]
		private static extern int FormatMessage(
			FormatMessageFlags dwFlags,
			IntPtr lpSource,
			int dwMessageId,
			int dwLanguageId,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder lpBuffer,
			int nSize,
			IntPtr[] Arguments);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryExW", SetLastError = true, ExactSpelling = true)]
		[SecurityCritical]
		private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

		#endregion

		#region Declarations

		[Flags]
		[UnmanagedName("#defines in WinBase.h")]
		internal enum LoadLibraryExFlags
		{
			DontResolveDllReferences = 0x00000001,
			LoadLibraryAsDataFile = 0x00000002,
			LoadWithAlteredSearchPath = 0x00000008,
			LoadIgnoreCodeAuthzLevel = 0x00000010,
			LoadLibrarySearchSystem32 = 0x00000800, // search system32 dir
		}

		[Flags]
		[UnmanagedName("FORMAT_MESSAGE_* defines")]
		internal enum FormatMessageFlags
		{
			AllocateBuffer = 0x00000100,
			IgnoreInserts = 0x00000200,
			FromString = 0x00000400,
			FromHmodule = 0x00000800,
			FromSystem = 0x00001000,
			ArgumentArray = 0x00002000,
			MaxWidthMask = 0x000000FF
		}

		#endregion

		private static IntPtr s_hModule = IntPtr.Zero;

		private readonly int _lastError;

		/// <summary>
		/// Construct an empty MFError
		/// </summary>
		public MFError()
			: this(ResultCom.S_OK)
		{
		}

		/// <summary>
		/// Construct an MFError from an int
		/// </summary>
		/// <param name="hr">an int to turn into an MFError</param>
		public MFError(int hr)
		{
			_lastError = hr;
		}

		[NotNull]
		private static MFError MakeOne(int hr)
		{
			MFError m = hr switch
			{
				0 => new MFError(),
				< 0 => throw new MFException(hr),
				_ => new MFError(hr)
			};

			return m;
		}

		/// <summary>
		/// If hr is a fatal error, an exception is thrown, otherwise an MFError is returned.  This
		/// allows you to check for status codes (like MF_S_PROTECTION_NOT_REQUIRED).
		/// </summary>
		/// <example>
		/// // Throws an exception if the create fails.
		/// MFError hrthrow = new MFError();
		/// hrthrow = MFAPI.MFCreateAttributes(out ia, 6);
		/// </example>
		/// <param name="hr">The value from which to construct the MFError.</param>
		/// <returns>The new MFError</returns>
		[NotNull]
		public static implicit operator MFError(int hr)
		{
			return MakeOne(hr);
		}

		/// <summary>
		/// Convert an MFError back to an integer
		/// </summary>
		/// <param name="hr">The MFError from which to retrieve the integer</param>
		/// <returns>The MFError as an integer</returns>
		public static implicit operator int([NotNull] MFError hr)
		{
			return hr._lastError;
		}

		/// <summary>
		/// Convert the MFError to the error string
		/// </summary>
		/// <returns>The string</returns>
		[NotNull]
		public override string ToString()
		{
			return GetErrorText(_lastError);
		}

		#region static methods

		public bool Failed()
		{
			return Failed(_lastError);
		}
		public static bool Failed(int hr)
		{
			return hr < 0;
		}

		public bool Succeeded()
		{
			return Succeeded(_lastError);
		}

		public static bool Succeeded(int hr)
		{
			return hr >= 0;
		}

		[NotNull]
		public string GetErrorText()
		{
			return GetErrorText(_lastError);
		}

		[SecurityCritical]
		[NotNull]
		public static string GetErrorText(int hr)
		{
			// Make sure the resource dll is loaded.
			if (s_hModule == IntPtr.Zero)
			{
				// Deal with possible multi-threading problems
				lock (MESSAGE_FILE)
				{
					// Make sure it didn't get set while we waited for the lock
					if (s_hModule == IntPtr.Zero)
					{
						LoadLibraryExFlags f = LoadLibraryExFlags.LoadLibraryAsDataFile | LoadLibraryExFlags.LoadLibrarySearchSystem32;
						// Load the Media Foundation error message dll
						s_hModule = LoadLibraryEx(MESSAGE_FILE, IntPtr.Zero, f);

						// LoadLibraryExFlags.LoadLibrarySearchSystem32 may not be supported
						if (s_hModule == IntPtr.Zero && Marshal.GetLastWin32Error() == 87)
						{
							// Perhaps KB2533623 is not installed.  Try again.
							s_hModule = LoadLibraryEx(MESSAGE_FILE, IntPtr.Zero, LoadLibraryExFlags.LoadLibraryAsDataFile);
						}
					}
				}
			}

			// Get the text (always returns something)
			string sText = GetErrorTextFromDll(hr, s_hModule);

			return sText;
		}

		[SecurityCritical]
		[NotNull]
		private static string GetErrorTextFromDll(int hr, IntPtr hLib)
		{
			// Most strings fit in this, but we'll resize if needed.
			StringBuilder sb = new StringBuilder(128);

			FormatMessageFlags dwFormatFlags =
				FormatMessageFlags.IgnoreInserts |
				FormatMessageFlags.MaxWidthMask |
				FormatMessageFlags.FromSystem;

			if (hLib != IntPtr.Zero)
			{
				dwFormatFlags |= FormatMessageFlags.FromHmodule;
			}

			while (true)
			{
				int dwBufferLength = FormatMessage(dwFormatFlags,
													hLib, // module to get message from (NULL == system)
													hr, // error number to get message for
													0, // default language
													sb,
													sb.Capacity,
													null
												);

				// Got the message.
				if (dwBufferLength > 0)
					return sb.ToString();

				// Error of some kind (possibly 'not found').
				if (Marshal.GetLastWin32Error() != 122) // 122 = buffer too small
					return string.Format("Error 0x{0:X}", hr);

				// Need a bigger buffer.
				sb = new StringBuilder(sb.Capacity * 2);
			}
		}

		public void ThrowExceptionForHR()
		{
			ThrowExceptionForHR(_lastError);
		}

		/// <summary>
		/// If hr has a "failed" status code (E_*), throw an exception.  Note that status
		/// messages (S_*) are not considered failure codes.  If MediaFoundation error text
		/// is available, it is used to build the exception, otherwise a generic com error
		/// is thrown.
		/// </summary>
		/// <param name="hr">The HRESULT to check</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.ForwardRef)]
		public static void ThrowExceptionForHR(int hr)
		{
			// If a severe error has occurred
			if (hr >= 0) return;
			throw new MFException(hr);
		}

		#endregion
	}
}