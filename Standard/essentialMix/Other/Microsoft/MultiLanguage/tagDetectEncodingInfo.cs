using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential)]
internal struct DetectEncodingInfo
{
	public uint nLangID;
	public uint nCodePage;
	public int nDocPercent;
	public int nConfidence;
}