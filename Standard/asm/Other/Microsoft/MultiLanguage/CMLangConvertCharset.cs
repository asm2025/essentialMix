using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage
{
	[ComImport]
	[Guid("D66D6F98-CDAA-11D0-B822-00C04FC9B31F")]
	[CoClass(typeof(CMLangConvertCharsetClass))]
	internal interface CMLangConvertCharset : IMLangConvertCharset
	{
	}
}