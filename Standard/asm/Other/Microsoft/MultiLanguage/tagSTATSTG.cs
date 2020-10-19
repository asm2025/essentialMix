using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct tagSTATSTG
	{
		[MarshalAs(UnmanagedType.LPWStr)] public string pwcsName;

		public uint type;
		public _ULARGE_INTEGER cbSize;
		public _FILETIME mtime;
		public _FILETIME ctime;
		public _FILETIME atime;
		public uint grfMode;
		public uint grfLocksSupported;
		public Guid clsid;
		public uint grfStateBits;
		public uint reserved;
	}
}