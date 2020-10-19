using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct _ULARGE_INTEGER
	{
		public ulong QuadPart;
	}
}