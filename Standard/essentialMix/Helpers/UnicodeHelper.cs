using System.Collections.Generic;
using System.Linq;
using essentialMix.Collections;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

/// <summary>
/// Utility class providing a number of singleton instances of
/// Range&lt;char&gt; to indicate the various ranges of unicode characters,
/// as documented at http://msdn.microsoft.com/en-us/library/20bw873z.aspx.
/// Note that this does not indicate the Unicode category of a character,
/// merely which range it's in.
/// TODO: Work out how to include names. Can't derive from Range[char].
/// </summary>
public static class UnicodeHelper
{
	private static readonly ISet<LambdaRange<char>> __allRanges = new HashSet<LambdaRange<char>>();

	public static ReadOnlySet<LambdaRange<char>> AllRanges { get; } = new ReadOnlySet<LambdaRange<char>>(__allRanges);

	[NotNull]
	public static LambdaRange<char> BasicLatin => CreateRange('\u0000', '\u007f');

	[NotNull]
	public static LambdaRange<char> Latin1Supplement => CreateRange('\u0080', '\u00ff');

	[NotNull]
	public static LambdaRange<char> LatinExtendedA => CreateRange('\u0100', '\u017f');

	[NotNull]
	public static LambdaRange<char> LatinExtendedB => CreateRange('\u0180', '\u024f');

	[NotNull]
	public static LambdaRange<char> IpaExtensions => CreateRange('\u0250', '\u02af');

	[NotNull]
	public static LambdaRange<char> SpacingModifierLetters => CreateRange('\u02b0', '\u02ff');

	[NotNull]
	public static LambdaRange<char> CombiningDiacriticalMarks => CreateRange('\u0300', '\u036f');

	[NotNull]
	public static LambdaRange<char> GreekAndCoptic => CreateRange('\u0370', '\u03ff');

	[NotNull]
	public static LambdaRange<char> Cyrillic => CreateRange('\u0400', '\u04ff');

	[NotNull]
	public static LambdaRange<char> CyrillicSupplement => CreateRange('\u0500', '\u052f');

	[NotNull]
	public static LambdaRange<char> Armenian => CreateRange('\u0530', '\u058f');

	[NotNull]
	public static LambdaRange<char> Hebrew => CreateRange('\u0590', '\u05FF');

	[NotNull]
	public static LambdaRange<char> Arabic => CreateRange('\u0600', '\u06ff');

	[NotNull]
	public static LambdaRange<char> Syriac => CreateRange('\u0700', '\u074f');

	[NotNull]
	public static LambdaRange<char> Thaana => CreateRange('\u0780', '\u07bf');

	[NotNull]
	public static LambdaRange<char> Devangari => CreateRange('\u0900', '\u097f');

	[NotNull]
	public static LambdaRange<char> Bengali => CreateRange('\u0980', '\u09ff');

	[NotNull]
	public static LambdaRange<char> Gurmukhi => CreateRange('\u0a00', '\u0a7f');

	[NotNull]
	public static LambdaRange<char> Gujarati => CreateRange('\u0a80', '\u0aff');

	[NotNull]
	public static LambdaRange<char> Oriya => CreateRange('\u0b00', '\u0b7f');

	[NotNull]
	public static LambdaRange<char> Tamil => CreateRange('\u0b80', '\u0bff');

	[NotNull]
	public static LambdaRange<char> Telugu => CreateRange('\u0c00', '\u0c7f');

	[NotNull]
	public static LambdaRange<char> Kannada => CreateRange('\u0c80', '\u0cff');

	[NotNull]
	public static LambdaRange<char> Malayalam => CreateRange('\u0d00', '\u0d7f');

	[NotNull]
	public static LambdaRange<char> Sinhala => CreateRange('\u0d80', '\u0dff');

	[NotNull]
	public static LambdaRange<char> Thai => CreateRange('\u0e00', '\u0e7f');

	[NotNull]
	public static LambdaRange<char> Lao => CreateRange('\u0e80', '\u0eff');

	[NotNull]
	public static LambdaRange<char> Tibetan => CreateRange('\u0f00', '\u0fff');

	[NotNull]
	public static LambdaRange<char> Myanmar => CreateRange('\u1000', '\u109f');

	[NotNull]
	public static LambdaRange<char> Georgian => CreateRange('\u10a0', '\u10ff');

	[NotNull]
	public static LambdaRange<char> HangulJamo => CreateRange('\u1100', '\u11ff');

	[NotNull]
	public static LambdaRange<char> Ethiopic => CreateRange('\u1200', '\u137f');

	[NotNull]
	public static LambdaRange<char> Cherokee => CreateRange('\u13a0', '\u13ff');

	[NotNull]
	public static LambdaRange<char> UnifiedCanadianAboriginalSyllabics => CreateRange('\u1400', '\u167f');

	[NotNull]
	public static LambdaRange<char> Ogham => CreateRange('\u1680', '\u169f');

	[NotNull]
	public static LambdaRange<char> Runic => CreateRange('\u16a0', '\u16ff');

	[NotNull]
	public static LambdaRange<char> Tagalog => CreateRange('\u1700', '\u171f');

	[NotNull]
	public static LambdaRange<char> Hanunoo => CreateRange('\u1720', '\u173f');

	[NotNull]
	public static LambdaRange<char> Buhid => CreateRange('\u1740', '\u175f');

	[NotNull]
	public static LambdaRange<char> Tagbanwa => CreateRange('\u1760', '\u177f');

	[NotNull]
	public static LambdaRange<char> Khmer => CreateRange('\u1780', '\u17ff');

	[NotNull]
	public static LambdaRange<char> Mongolian => CreateRange('\u1800', '\u18af');

	[NotNull]
	public static LambdaRange<char> Limbu => CreateRange('\u1900', '\u194f');

	[NotNull]
	public static LambdaRange<char> TaiLe => CreateRange('\u1950', '\u197f');

	[NotNull]
	public static LambdaRange<char> KhmerSymbols => CreateRange('\u19e0', '\u19ff');

	[NotNull]
	public static LambdaRange<char> PhoneticExtensions => CreateRange('\u1d00', '\u1d7f');

	[NotNull]
	public static LambdaRange<char> LatinExtendedAdditional => CreateRange('\u1e00', '\u1eff');

	[NotNull]
	public static LambdaRange<char> GreekExtended => CreateRange('\u1f00', '\u1fff');

	[NotNull]
	public static LambdaRange<char> GeneralPunctuation => CreateRange('\u2000', '\u206f');

	[NotNull]
	public static LambdaRange<char> SuperscriptsandSubscripts => CreateRange('\u2070', '\u209f');

	[NotNull]
	public static LambdaRange<char> CurrencySymbols => CreateRange('\u20a0', '\u20cf');

	[NotNull]
	public static LambdaRange<char> CombiningDiacriticalMarksforSymbols => CreateRange('\u20d0', '\u20ff');

	[NotNull]
	public static LambdaRange<char> LetterlikeSymbols => CreateRange('\u2100', '\u214f');

	[NotNull]
	public static LambdaRange<char> NumberForms => CreateRange('\u2150', '\u218f');

	[NotNull]
	public static LambdaRange<char> Arrows => CreateRange('\u2190', '\u21ff');

	[NotNull]
	public static LambdaRange<char> MathematicalOperators => CreateRange('\u2200', '\u22ff');

	[NotNull]
	public static LambdaRange<char> MiscellaneousTechnical => CreateRange('\u2300', '\u23ff');

	[NotNull]
	public static LambdaRange<char> ControlPictures => CreateRange('\u2400', '\u243f');

	[NotNull]
	public static LambdaRange<char> OpticalCharacterRecognition => CreateRange('\u2440', '\u245f');

	[NotNull]
	public static LambdaRange<char> EnclosedAlphanumerics => CreateRange('\u2460', '\u24ff');

	[NotNull]
	public static LambdaRange<char> BoxDrawing => CreateRange('\u2500', '\u257f');

	[NotNull]
	public static LambdaRange<char> BlockElements => CreateRange('\u2580', '\u259f');

	[NotNull]
	public static LambdaRange<char> GeometricShapes => CreateRange('\u25a0', '\u25ff');

	[NotNull]
	public static LambdaRange<char> MiscellaneousSymbols => CreateRange('\u2600', '\u26ff');

	[NotNull]
	public static LambdaRange<char> Dingbats => CreateRange('\u2700', '\u27bf');

	[NotNull]
	public static LambdaRange<char> MiscellaneousMathematicalSymbolsA => CreateRange('\u27c0', '\u27ef');

	[NotNull]
	public static LambdaRange<char> SupplementalArrowsA => CreateRange('\u27f0', '\u27ff');

	[NotNull]
	public static LambdaRange<char> BraillePatterns => CreateRange('\u2800', '\u28ff');

	[NotNull]
	public static LambdaRange<char> SupplementalArrowsB => CreateRange('\u2900', '\u297f');

	[NotNull]
	public static LambdaRange<char> MiscellaneousMathematicalSymbolsB => CreateRange('\u2980', '\u29ff');

	[NotNull]
	public static LambdaRange<char> SupplementalMathematicalOperators => CreateRange('\u2a00', '\u2aff');

	[NotNull]
	public static LambdaRange<char> MiscellaneousSymbolsandArrows => CreateRange('\u2b00', '\u2bff');

	[NotNull]
	public static LambdaRange<char> CjkRadicalsSupplement => CreateRange('\u2e80', '\u2eff');

	[NotNull]
	public static LambdaRange<char> KangxiRadicals => CreateRange('\u2f00', '\u2fdf');

	[NotNull]
	public static LambdaRange<char> IdeographicDescriptionCharacters => CreateRange('\u2ff0', '\u2fff');

	[NotNull]
	public static LambdaRange<char> CjkSymbolsandPunctuation => CreateRange('\u3000', '\u303f');

	[NotNull]
	public static LambdaRange<char> Hiragana => CreateRange('\u3040', '\u309f');

	[NotNull]
	public static LambdaRange<char> Katakana => CreateRange('\u30a0', '\u30ff');

	[NotNull]
	public static LambdaRange<char> Bopomofo => CreateRange('\u3100', '\u312f');

	[NotNull]
	public static LambdaRange<char> HangulCompatibilityJamo => CreateRange('\u3130', '\u318f');

	[NotNull]
	public static LambdaRange<char> Kanbun => CreateRange('\u3190', '\u319f');

	[NotNull]
	public static LambdaRange<char> BopomofoExtended => CreateRange('\u31a0', '\u31bf');

	[NotNull]
	public static LambdaRange<char> KatakanaPhoneticExtensions => CreateRange('\u31f0', '\u31ff');

	[NotNull]
	public static LambdaRange<char> EnclosedCjkLettersandMonths => CreateRange('\u3200', '\u32ff');

	[NotNull]
	public static LambdaRange<char> CjkCompatibility => CreateRange('\u3300', '\u33ff');

	[NotNull]
	public static LambdaRange<char> CjkUnifiedIdeographsExtensionA => CreateRange('\u3400', '\u4dbf');

	[NotNull]
	public static LambdaRange<char> YijingHexagramSymbols => CreateRange('\u4dc0', '\u4dff');

	[NotNull]
	public static LambdaRange<char> CjkUnifiedIdeographs => CreateRange('\u4e00', '\u9fff');

	[NotNull]
	public static LambdaRange<char> YiSyllables => CreateRange('\ua000', '\ua48f');

	[NotNull]
	public static LambdaRange<char> YiRadicals => CreateRange('\ua490', '\ua4cf');

	[NotNull]
	public static LambdaRange<char> HangulSyllables => CreateRange('\uac00', '\ud7af');

	[NotNull]
	public static LambdaRange<char> HighSurrogates => CreateRange('\ud800', '\udb7f');

	[NotNull]
	public static LambdaRange<char> HighPrivateUseSurrogates => CreateRange('\udb80', '\udbff');

	[NotNull]
	public static LambdaRange<char> LowSurrogates => CreateRange('\udc00', '\udfff');

	[NotNull]
	public static LambdaRange<char> PrivateUse => CreateRange('\ue000', '\uf8ff');

	[NotNull]
	public static LambdaRange<char> PrivateUseArea => CreateRange('\uf900', '\ufaff');

	[NotNull]
	public static LambdaRange<char> CjkCompatibilityIdeographs => CreateRange('\ufb00', '\ufb4f');

	[NotNull]
	public static LambdaRange<char> AlphabeticPresentationForms => CreateRange('\ufb50', '\ufdff');

	[NotNull]
	public static LambdaRange<char> ArabicPresentationFormsA => CreateRange('\ufe00', '\ufe0f');

	[NotNull]
	public static LambdaRange<char> VariationSelectors => CreateRange('\ufe20', '\ufe2f');

	[NotNull]
	public static LambdaRange<char> CombiningHalfMarks => CreateRange('\ufe30', '\ufe4f');

	[NotNull]
	public static LambdaRange<char> CjkCompatibilityForms => CreateRange('\ufe50', '\ufe6f');

	[NotNull]
	public static LambdaRange<char> SmallFormVariants => CreateRange('\ufe70', '\ufeff');

	[NotNull]
	public static LambdaRange<char> ArabicPresentationFormsB => CreateRange('\uff00', '\uffef');

	[NotNull]
	public static LambdaRange<char> HalfwidthandFullwidthForms => CreateRange('\ufff0', '\uffff');

	/// <summary>
	/// Returns the unicode range containing the specified character.
	/// </summary>
	/// <param name="c">Character to look for</param>
	/// <returns>The unicode range containing the specified character, or null if the character
	/// is not in a unicode range.</returns>
	public static LambdaRange<char> GetRange(char c)
	{
		// TODO: Make this efficient. SortedList should do it with a binary search, but it
		// doesn't give us quite what we want
		return __allRanges.FirstOrDefault(range => range.Contains(c));
	}

	[NotNull]
	private static LambdaRange<char> CreateRange(char from, char to)
	{
		// TODO: Check for overlaps
		LambdaRange<char> ret = new LambdaRange<char>(from, to);
		__allRanges.Add(ret);
		return ret;
	}
}