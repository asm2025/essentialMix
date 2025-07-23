using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bogus;
using Bogus.DataSets;
using essentialMix.Exceptions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Test.Common.Model;

namespace Test.Common;

public static class Generator
{
	private static readonly Lazy<Faker> __fakeGenerator = new Lazy<Faker>(() => new Faker(), LazyThreadSafetyMode.PublicationOnly);

	public static Faker Faker => __fakeGenerator.Value;

	[NotNull]
	public static Action<IList<T>, int, int, IComparer<T>, bool> GetSortAlgorithm<T>([NotNull] string name)
	{
		return name switch
		{
			nameof(IListExtension.SortBubble) => IListExtension.SortBubble,
			nameof(IListExtension.SortSelection) => IListExtension.SortSelection,
			nameof(IListExtension.SortInsertion) => IListExtension.SortInsertion,
			nameof(IListExtension.SortHeap) => IListExtension.SortHeap,
			nameof(IListExtension.SortMerge) => IListExtension.SortMerge,
			nameof(IListExtension.SortQuick) => IListExtension.SortQuick,
			nameof(IListExtension.SortShell) => IListExtension.SortShell,
			nameof(IListExtension.SortComb) => IListExtension.SortComb,
			nameof(IListExtension.SortTim) => IListExtension.SortTim,
			nameof(IListExtension.SortCocktail) => IListExtension.SortCocktail,
			nameof(IListExtension.SortBitonic) => IListExtension.SortBitonic,
			nameof(IListExtension.SortPancake) => IListExtension.SortPancake,
			nameof(IListExtension.SortBinary) => IListExtension.SortBinary,
			nameof(IListExtension.SortGnome) => IListExtension.SortGnome,
			nameof(IListExtension.SortBrick) => IListExtension.SortBrick,
			_ => throw new NotFoundException()
		};
	}

	[NotNull]
	public static int[] GetRandomIntegers(int len = 0) { return GetRandomIntegers(false, len); }

	[NotNull]
	public static int[] GetRandomIntegers(bool unique, int len = 0)
	{
		const double GAPS_THRESHOLD = 0.25d;

		if (len < 1) len = RNGRandomHelper.Next(1, 12);

		int[] values = new int[len];

		if (unique)
		{
			int gaps = (int)(len * GAPS_THRESHOLD);
			values = Enumerable.Range(1, len).ToArray();

			int min = len + 1, max = min + gaps + 1;

			for (int i = 0; i < gaps; i++)
				values[RNGRandomHelper.Next(0, values.Length - 1)] = RNGRandomHelper.Next(min, max);

			values.Shuffle();
		}
		else
		{
			for (int i = 0; i < len; i++)
				values[i] = RNGRandomHelper.Next(1, short.MaxValue);
		}

		return values;
	}

	[NotNull]
	public static char[] GetRandomChar(int len = 0) { return GetRandomChar(false, len); }

	[NotNull]
	public static char[] GetRandomChar(bool unique, int len = 0)
	{
		if (len < 1) len = RNGRandomHelper.Next(1, 12);

		char[] values = new char[len];

		if (unique)
		{
			int i = 0;
			HashSet<char> set = [];

			while (i < len)
			{
				char value = (char)RNGRandomHelper.Next('a', 'z');
				if (!set.Add(value)) continue;
				values[i++] = value;
			}
		}
		else
		{
			for (int i = 0; i < len; i++)
			{
				values[i] = (char)RNGRandomHelper.Next('a', 'z');
			}
		}

		return values;
	}

	[NotNull]
	public static ICollection<string> GetRandomStrings(int len = 0) { return GetRandomStrings(false, len); }

	[NotNull]
	public static ICollection<string> GetRandomStrings(bool unique, int len = 0)
	{
		if (len < 1) len = RNGRandomHelper.Next(1, 12);
		if (!unique) return __fakeGenerator.Value.Random.WordsArray(len);

		HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		while (set.Count < len)
			set.Add(__fakeGenerator.Value.Random.Word());

		return set;
	}

	[NotNull]
	public static Student[] GetRandomStudents(int len = 0)
	{
		if (len < 1) len = RNGRandomHelper.Next(1, 12);

		Student[] students = new Student[len];

		for (int i = 0; i < len; i++)
		{
			students[i] = new Student
			{
				Id = i + 1,
				Name = __fakeGenerator.Value.Name.FirstName(__fakeGenerator.Value.PickRandom<Name.Gender>()),
				Grade = __fakeGenerator.Value.Random.Double(0.0d, 100.0d)
			};
		}

		return students;
	}
}