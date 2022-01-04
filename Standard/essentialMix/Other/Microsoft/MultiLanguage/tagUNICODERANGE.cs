using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct tagUNICODERANGE
{
	public ushort wcFrom;
	public ushort wcTo;
}