using System;
using essentialMix.Extensions;

namespace Test.Common;

public static class Constants
{
	public const int START = 10;
	public const int SMALL = 10_000;
	public const int MEDIUM = 100_000;
	public const int HEAVY = 1_000_000;

	public const int MAX_ITERATION_INC = 3;
	public const int TOP_COUNT = 10;

	private static readonly Lazy<string[]> __sortAlgorithms = new Lazy<string[]>(() =>
	[
		nameof(IListExtension.SortBubble),
		nameof(IListExtension.SortSelection),
		nameof(IListExtension.SortInsertion),
		nameof(IListExtension.SortHeap),
		nameof(IListExtension.SortMerge),
		nameof(IListExtension.SortQuick),
		nameof(IListExtension.SortShell),
		nameof(IListExtension.SortComb),
		nameof(IListExtension.SortTim),
		nameof(IListExtension.SortCocktail),
		nameof(IListExtension.SortBitonic),
		nameof(IListExtension.SortPancake),
		nameof(IListExtension.SortBinary),
		nameof(IListExtension.SortGnome),
		nameof(IListExtension.SortBrick)
	]);

	public static string[] SortAlgorithms => __sortAlgorithms.Value;

	public static bool LimitThreads()
	{
		// change this to true to use 1 thread only for debugging
		return false;
	}
}