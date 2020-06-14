using System.Runtime.InteropServices;

namespace asm.Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct DetectEncodingInfo
	{
		public uint nLangID;
		public uint nCodePage;
		public int nDocPercent;
		public int nConfidence;
	}
}