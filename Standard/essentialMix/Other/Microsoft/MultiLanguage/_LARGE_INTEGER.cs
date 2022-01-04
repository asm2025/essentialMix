using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct _LARGE_INTEGER
{
	public long QuadPart;
}