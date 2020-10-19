using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct _FILETIME
	{
		public uint dwLowDateTime;
		public uint dwHighDateTime;
	}
}