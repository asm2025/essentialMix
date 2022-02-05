using System;
using System.Text;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Text;

namespace essentialMix.Extensions;

public static class EncodingExtension
{
	public static bool IsBigEndian([NotNull] this Encoding thisValue) { return thisValue.WebName.IsSame("UTF-16BE"); }

	public static bool IsBOM([NotNull] this Encoding thisValue) { return thisValue.WebName.IsSame("UTF-16"); }

	[NotNull]
	public static Encoding GetWebEncoding([NotNull] this Encoding thisValue) { return GetWebEncoding(thisValue, IsBigEndian(thisValue), IsBOM(thisValue)); }

	[NotNull]
	public static Encoding GetWebEncoding([NotNull] this Encoding thisValue, bool bigEndian, bool bom)
	{
		int unisize = GetUnicodeSize(thisValue);

		switch (unisize)
		{
			case 1:
				return new UTF8Encoding(bom);
			case 2:
				return new UTF16Encoding(bigEndian, bom);
			default:
				return thisValue;
		}
	}

	public static int GetUnicodeSize([NotNull] this Encoding thisValue)
	{
		if (thisValue.WebName.StartsWith("UTF-8", StringComparison.OrdinalIgnoreCase)) return 1;

		if (thisValue.WebName.StartsWith("UTF-16", StringComparison.OrdinalIgnoreCase) ||
			thisValue.WebName.Contains("UCS-2", StringComparison.OrdinalIgnoreCase)) return 2;

		return 0;
	}

	public static EncodingInfo EncodingInfo([NotNull] this Encoding thisValue)
	{
		return GenericsHelper.CreateInstance<EncodingInfo>(thisValue.CodePage, thisValue.WebName, thisValue.EncodingName);
	}
}