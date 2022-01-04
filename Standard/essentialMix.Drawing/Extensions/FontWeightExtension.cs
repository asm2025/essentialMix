// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class FontWeightExtension
{
	public static bool IsBold(this FontWeight value) { return value >= FontWeight.FW_MEDIUM; }
}