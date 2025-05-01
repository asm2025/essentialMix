using System;
using System.Text;

namespace essentialMix.Text;

[Serializable]
public class UTF16Encoding(bool bBigEndian, bool bBom) : UnicodeEncoding(bBigEndian, bBom)
{
	private const string ENCODING_NAME = "UTF-16";

	public override string BodyName => ENCODING_NAME;

	public override string EncodingName => ENCODING_NAME;

	public override string WebName => ENCODING_NAME;
}