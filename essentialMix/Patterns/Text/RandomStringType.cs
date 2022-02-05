using System;

namespace essentialMix.Patterns.Text;

[Flags]
public enum RandomStringType
{
	None = 0,
	SmallLetters = 1,
	CapitalLetters = 1 << 1,
	Numbers = 1 << 2,
	SpecialCharacters = 1 << 3,
	SafeCharacters = 1 << 4,
	Letters = SmallLetters | CapitalLetters,
	AlphaNumeric = Letters | Numbers,
	Any = AlphaNumeric | SpecialCharacters,
	SafeAny = Any | SafeCharacters
}