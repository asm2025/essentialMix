using System.Runtime.InteropServices;

namespace asm.Internal.MultiLanguage
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