using System;
using System.Runtime.InteropServices;
using System.Text;

namespace asm.Text
{
	[ComVisible(true)]
	[Serializable]
	public class UTF16Encoding : UnicodeEncoding
	{
		private const string ENCODING_NAME = "UTF-16";

		public UTF16Encoding(bool bBigEndian, bool bBOM)
			: base(bBigEndian, bBOM) { }

		public override string BodyName => ENCODING_NAME;

		public override string EncodingName => ENCODING_NAME;

		public override string WebName => ENCODING_NAME;
	}
}