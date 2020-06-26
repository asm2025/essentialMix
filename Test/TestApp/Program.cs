using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using asm.Collections;
using asm.Collections.Concurrent.ProducerConsumer;
using asm.Comparers;
using asm.Exceptions;
using asm.Extensions;
using asm.Helpers;
using asm.Other.Microsoft.Collections;
using Bogus;
using Bogus.DataSets;
using Crayon;
using EasyConsole;
using JetBrains.Annotations;

namespace TestApp
{
	internal class Program
	{
		private static readonly Lazy<Faker> __fakeGenerator = new Lazy<Faker>(() => new Faker(), LazyThreadSafetyMode.PublicationOnly);

		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;

			//TestDomainName();

			//TestThreadQueue();

			//TestSortAlgorithm();
			//TestSortAlgorithms();

			//TestLinkedQueue();
			//TestMinMaxQueue();

			//TestSinglyLinkedList();
			//TestLinkedList();

			//TestDeque();
			//TestLinkedDeque();

			//TestBinaryTreeFromTraversal();
			
			//TestBinarySearchTreeAdd();
			//TestBinarySearchTreeRemove();
			//TestBinarySearchTreeBalance();
			
			//TestAVLTreeAdd();
			//TestAVLTreeRemove();
			
			//TestRedBlackTreeAdd();
			//TestRedBlackTreeRemove();

			//TestAllBinaryTrees();
			//TestAllBinaryTreesFunctionality();
			//TestAllBinaryTreesPerformance();

			//TestSortedSetPerformance();

			//TestTreeEquality();
			
			//TestHeapAdd();
			//TestHeapRemove();
			
			//TestPriorityQueue();

			//TestHeapElementAt();

			//TestTrie();
			//TestTrieSimilarWordsRemoval();

			TestGraph();

			//TestSkipList();

			//TestDisjointSet();

			ConsoleHelper.Pause();
		}

		private static void TestDomainName()
		{
			string[] domains =
			{
				"https://stackoverflow.com/questions/4643227/top-level-domain-from-url-in-c-sharp",
				"https://stackoverflow.com/questions/3121957/how-can-i-do-a-case-insensitive-string-comparison",
				"https://github.com/nager/Nager.PublicSuffix",
				"https://docs.microsoft.com/en-us/dotnet/csharp/how-to/compare-strings",
				"https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators"
			};
			List<(string, string)> matchingDomains = new List<(string, string)>();
			Title("Testing domain names...");

			for (int i = 0; i < domains.Length - 1; i++)
			{
				string x = domains[i];

				for (int j = i + 1; j < domains.Length; j++)
				{
					string y = domains[j];
					Console.WriteLine("Testing:".BrightBlack());
					Console.WriteLine(x);
					Console.WriteLine(y);
					bool matching = DomainNameComparer.Default.Equals(x, y);
					Console.WriteLine(matching.ToYesNo());
					if (!matching) continue;
					matchingDomains.Add((x, y));
				}
			}

			if (matchingDomains.Count == 0)
			{
				Console.WriteLine("No matching entries..!".BrightRed());
				return;
			}

			Console.WriteLine($"Found {matchingDomains.Count.ToString().BrightGreen()} entries:");

			foreach ((string, string) tuple in matchingDomains)
			{
				Console.WriteLine(tuple.Item1);
				Console.WriteLine(tuple.Item2);
				Console.WriteLine();
			}
		}

		private static void TestSortAlgorithm()
		{
			const string algorithm = nameof(IListExtension.SortHeap);

			Action<IList<int>, int, int, IComparer<int>, bool> sortNumbers = GetAlgorithm<int>(algorithm);
			Action<IList<string>, int, int, IComparer<string>, bool> sortStrings = GetAlgorithm<string>(algorithm);
			Console.WriteLine($"Testing {algorithm.BrightCyan()} algorithm: ");

			Stopwatch watch = new Stopwatch();
			IComparer<int> numbersComparer = Comparer<int>.Default;
			IComparer<string> stringComparer = StringComparer.Ordinal;
			bool more;

			do
			{
				Console.Clear();
				int[] numbers = GetRandomIntegers(RNGRandomHelper.Next(5, 20));
				string[] strings = GetRandomStrings(RNGRandomHelper.Next(3, 10)).ToArray();
				Console.WriteLine("Numbers: ".BrightCyan() + string.Join(", ", numbers));
				Console.WriteLine("String: ".BrightCyan() + string.Join(", ", strings.Select(e => e.SingleQuote())));

				Console.Write("Numbers");
				watch.Restart();
				sortNumbers(numbers, 0, -1, numbersComparer, false);
				long numericResults = watch.ElapsedMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {numericResults.ToString().BrightGreen()}");
				Console.WriteLine("Result: " + string.Join(", ", numbers));
				Console.WriteLine();

				Console.Write("Strings");
				watch.Restart();
				sortStrings(strings, 0, -1, stringComparer, false);
				long stringResults = watch.ElapsedMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {stringResults.ToString().BrightGreen()}");
				Console.WriteLine("Result: " + string.Join(", ", strings.Select(e => e.SingleQuote())));
				Console.WriteLine();

				Console.WriteLine("Finished".BrightYellow());
				Console.WriteLine();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static Action<IList<T>, int, int, IComparer<T>, bool> GetAlgorithm<T>(string name)
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
		}

		private static void TestSortAlgorithms()
		{
			const int TRIES = 100;
			const int RESULT_COUNT = 5;

			Title("Testing Sort Algorithms...");

			string[] sortAlgorithms = 
			{
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
			};

			Stopwatch watch = new Stopwatch();
			IComparer<int> numbersComparer = Comparer<int>.Default;
			IComparer<string> stringComparer = StringComparer.Ordinal;
			IDictionary<string, double> numericResults = new Dictionary<string, double>();
			IDictionary<string, double> stringResults = new Dictionary<string, double>();
			long[] time = new long[TRIES];
			string sectionSeparator = new string('*', 80).BrightMagenta();
			bool more;

			do
			{
				Console.Clear();
				int[] numbers = GetRandomIntegers(RNGRandomHelper.Next(5, 20));
				string[] strings = GetRandomStrings(RNGRandomHelper.Next(3, 10)).ToArray();
				Console.WriteLine("Numbers: ".BrightCyan() + string.Join(", ", numbers));
				Console.WriteLine("String: ".BrightCyan() + string.Join(", ", strings.Select(e => e.SingleQuote())));
				Console.WriteLine($"Taking an average of {TRIES.ToString().BrightCyan()} times for each algorithm.");

				foreach (string algorithm in sortAlgorithms)
				{
					Action<IList<int>, int, int, IComparer<int>, bool> sortNumbers = GetAlgorithm<int>(algorithm);
					Action<IList<string>, int, int, IComparer<string>, bool> sortStrings = GetAlgorithm<string>(algorithm);
					Console.WriteLine(sectionSeparator);
					Console.WriteLine($"Testing {algorithm.BrightCyan()} algorithm: ");

					Console.Write("Numbers");
					int[] ints = (int[])numbers.Clone();

					for (int i = 0; i < time.Length; i++)
					{
						watch.Restart();
						sortNumbers(ints, 0, -1, numbersComparer, false);
						time[i] = watch.ElapsedMilliseconds;
						watch.Stop();
						if (i < time.Length - 1) ints = (int[])numbers.Clone();
					}

					numericResults[algorithm] = time.Average();
					Console.WriteLine($" => {numericResults[algorithm].ToString("F6").BrightGreen()}");
					Console.WriteLine("Result: " + string.Join(", ", ints));
					Console.WriteLine();

					Console.Write("Strings");

					string[] str = (string[])strings.Clone();

					for (int i = 0; i < time.Length; i++)
					{
						watch.Restart();
						sortStrings(str, 0, -1, stringComparer, false);
						time[i] = watch.ElapsedMilliseconds;
						watch.Stop();
					}

					stringResults[algorithm] = time.Average();
					Console.WriteLine($" => {stringResults[algorithm].ToString("F6").BrightGreen()}");
					Console.WriteLine("Result: " + string.Join(", ", str.Select(e => e.SingleQuote())));
					Console.WriteLine();
				}

				Console.WriteLine(sectionSeparator);
				Console.WriteLine("Finished".BrightYellow());
				Console.WriteLine();
				Console.WriteLine($"Fastest {RESULT_COUNT} numeric sort:".BrightGreen());
			
				foreach (KeyValuePair<string, double> pair in numericResults
															.OrderBy(e => e.Value)
															.Take(RESULT_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value:F6}");
				}

				Console.WriteLine();
				Console.WriteLine($"Fastest {RESULT_COUNT} string sort:".BrightGreen());

				foreach (KeyValuePair<string, double> pair in stringResults
															.OrderBy(e => e.Value)
															.Take(RESULT_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value:F6}");
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static Action<IList<T>, int, int, IComparer<T>, bool> GetAlgorithm<T>(string name)
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
		}

		private static void TestLinkedQueue()
		{
			Title("Testing LinkedQueue...");
			
			int len = RNGRandomHelper.Next(5, 20);
			int[] values = GetRandomIntegers(len);
			Console.WriteLine("Array: " + string.Join(", ", values));

			Console.WriteLine("As Queue:");
			LinkedQueue<int> queue = new LinkedQueue<int>(DequeuePriority.FIFO);
	
			foreach (int value in values)
			{
				queue.Enqueue(value);
			}
	
			while (queue.Count > 0)
			{
				Console.WriteLine(queue.Dequeue());
			}

			Console.WriteLine("As Stack:");
			queue = new LinkedQueue<int>(DequeuePriority.LIFO);

			foreach (int value in values)
			{
				queue.Enqueue(value);
			}

			while (queue.Count > 0)
			{
				Console.WriteLine(queue.Dequeue());
			}
		}

		private static void TestMinMaxQueue()
		{
			Title("Testing MinMaxQueue...");

			int len = RNGRandomHelper.Next(5, 20);
			int[] values = GetRandomIntegers(len);
			Console.WriteLine("Array: " + string.Join(", ", values));

			Console.WriteLine("As Queue:");
			MinMaxQueue<int> queue = new MinMaxQueue<int>(DequeuePriority.FIFO);

			foreach (int value in values)
			{
				queue.Enqueue(value);
				Console.WriteLine($"Adding Value: {value}, Min: {queue.Minimum}, Max: {queue.Maximum}");
			}
	
			Console.WriteLine();

			while (queue.Count > 0)
			{
				Console.WriteLine($"Dequeue Value: {queue.Dequeue()}, Min: {queue.Minimum}, Max: {queue.Maximum}");
			}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("As Stack:");
			queue = new MinMaxQueue<int>(DequeuePriority.LIFO);

			foreach (int value in values)
			{
				queue.Enqueue(value);
				Console.WriteLine($"Adding Value: {value}, Min: {queue.Minimum}, Max: {queue.Maximum}");
			}

			Console.WriteLine();

			while (queue.Count > 0)
			{
				Console.WriteLine($"Dequeue Value: {queue.Dequeue()}, Min: {queue.Minimum}, Max: {queue.Maximum}");
			}
		}

		private static void TestSinglyLinkedList()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, 200000);
			SinglyLinkedList<int> list = new SinglyLinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing SingleLinkedList...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test adding...");

				list.Clear();
				int count = list.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				clock.Restart();

				foreach (int v in values)
				{
					list.AddLast(v);
					count++;
				}

				Console.WriteLine($"Added {count} items of {values.Length} in {clock.ElapsedMilliseconds} ms.");

				if (list.Count != values.Length)
				{
					Console.WriteLine("Something went wrong, Count isn't right...!".BrightRed());
					return;
				}

				Console.WriteLine("Test find a random value...".BrightYellow());
				int x = values.PickRandom();
				SinglyLinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				int value = values.PickRandom();
				Console.WriteLine($"Found. Now will add {value.ToString().BrightCyan().Underline()} after {x.ToString().BrightCyan().Underline()}...");
				list.AddAfter(node, value);
				Console.WriteLine("Node's next: " + node.Next.Value);
				list.Remove(node.Next);

				Console.WriteLine($"Test adding {value.ToString().BrightCyan().Underline()} before {x.ToString().BrightCyan().Underline()}...");
				SinglyLinkedListNode<int> previous = list.AddBefore(node, value);
				list.Remove(previous);
	
				Console.WriteLine($"Test adding {value.ToString().BrightCyan().Underline()} to the beginning of the list...");
				list.AddFirst(value);
				list.RemoveFirst();

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (list.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (list.Remove(v))
					{
						removed++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Remove missed a value: {v} :((".BrightRed()
										: "REMOVE MISSED A LOT. :((".BrightRed());
					Console.WriteLine("Does it contain the value? " + list.Contains(v).ToYesNo());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();
		}

		private static void TestLinkedList()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true,200000);
			LinkedList<int> list = new LinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing LinkedList...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test adding...");

				list.Clear();
				int count = list.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				clock.Restart();

				foreach (int v in values)
				{
					list.AddLast(v);
					count++;
				}

				Console.WriteLine($"Added {count} items of {values.Length} in {clock.ElapsedMilliseconds} ms.");

				if (list.Count != values.Length)
				{
					Console.WriteLine("Something went wrong, Count isn't right...!".BrightRed());
					return;
				}

				Console.WriteLine("Test find a random value...".BrightYellow());
				int x = values.PickRandom();
				LinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				int value = values.PickRandom();
				Console.WriteLine($"Found. Now will add {value.ToString().BrightCyan().Underline()} after {x.ToString().BrightCyan().Underline()}...");
				list.AddAfter(node, value);
				Console.WriteLine("Node's next: " + node.Next?.Value);

				Console.WriteLine($"Test adding {value.ToString().BrightCyan().Underline()} before {x.ToString().BrightCyan().Underline()}...");
				list.AddBefore(node, value);
				Console.WriteLine("Node's previous: " + node.Previous?.Value);
				
				Console.WriteLine($"Test adding {value.ToString().BrightCyan().Underline()} to the beginning of the list...");
				list.AddFirst(value);

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (list.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (list.Remove(v))
					{
						removed++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Remove missed a value: {v} :((".BrightRed()
										: "REMOVE MISSED A LOT. :((".BrightRed());
					Console.WriteLine("Does it contain the value? " + list.Contains(v).ToYesNo());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();
		}

		private static void TestDeque()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, 100000/*200000*/);
			Deque<int> deque = new Deque<int>();

			do
			{
				Console.Clear();
				Title("Testing Deque...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test queue functionality...");

				Console.Write($"Would you like to print the results? {"[Y]".BrightGreen()} or {"any other key".Dim()}: ");
				bool print = Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();

				// Queue test
				Title("Testing Deque as a Queue...");
				DoTheTest(deque, values, deque.Enqueue, deque.Dequeue, print, clock);
				Title("End testing Deque as a Queue...");
				ConsoleHelper.Pause();

				// Stack test
				Title("Testing Deque as a Stack...");
				DoTheTest(deque, values, deque.Push, deque.Pop, print, clock);
				Title("End testing Deque as a Stack...");

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(Deque<int> deque, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				deque.Clear();
				int count = deque.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (deque.Count != values.Length)
				{
					Console.WriteLine("Something went wrong, Count isn't right...!".BrightRed());
					return;
				}

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				count = deque.Count / 4;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					int v = values[i];

					if (deque.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
		
				int removed = 0;
				count = deque.Count;
				clock.Restart();

				if (print)
				{
					while (deque.Count > 0 && count > 0)
					{
						Console.Write(remove());
						count--;
						removed++;
						if (deque.Count > 0) Console.Write(", ");
					}
				}
				else
				{
					while (deque.Count > 0 && count > 0)
					{
						remove();
						count--;
						removed++;
					}
				}

				Debug.Assert(count == 0 && deque.Count == 0, $"Values are not cleared correctly! {count} != {deque.Count}.");
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestLinkedDeque()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, 100000/*200000*/);
			LinkedDeque<int> deque = new LinkedDeque<int>();

			do
			{
				Console.Clear();
				Title("Testing Deque...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test queue functionality...");

				Console.Write($"Would you like to print the results? {"[Y]".BrightGreen()} or {"any other key".Dim()}: ");
				bool print = Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();

				// Queue test
				Title("Testing Deque as a Queue...");
				DoTheTest(deque, values, deque.Enqueue, deque.Dequeue, print, clock);
				Title("End testing Deque as a Queue...");
				ConsoleHelper.Pause();

				// Stack test
				Title("Testing Deque as a Stack...");
				DoTheTest(deque, values, deque.Push, deque.Pop, print, clock);
				Title("End testing Deque as a Stack...");

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(LinkedDeque<int> deque, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				deque.Clear();
				int count = deque.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (deque.Count != values.Length)
				{
					Console.WriteLine("Something went wrong, Count isn't right...!".BrightRed());
					return;
				}

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				count = deque.Count / 4;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					int v = values[i];

					if (deque.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				count = deque.Count;
				clock.Restart();

				if (print)
				{
					while (deque.Count > 0 && count > 0)
					{
						Console.Write(remove());
						count--;
						removed++;
						if (deque.Count > 0) Console.Write(", ");
					}
				}
				else
				{
					while (deque.Count > 0 && count > 0)
					{
						remove();
						count--;
						removed++;
					}
				}

				Debug.Assert(count == 0 && deque.Count == 0, $"Values are not cleared correctly! {count} != {deque.Count}.");
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestBinaryTreeFromTraversal()
		{
			const string TREE_DATA_LEVEL = "FCIADGJBEHK";
			const string TREE_DATA_PRE = "FCABDEIGHJK";
			const string TREE_DATA_IN = "ABCDEFGHIJK";
			const string TREE_DATA_POST = "BAEDCHGKJIF";
			const int NUM_TESTS = 7;

			BinarySearchTree<char> tree = new BinarySearchTree<char>();

			bool more;
			int i = 0;

			do
			{
				Console.Clear();
				Title("Testing BinaryTree from traversal values...");

				switch (i)
				{
					case 0:
						Console.WriteLine($@"Data from {"LevelOrder".BrightCyan()} traversal:
{TREE_DATA_LEVEL}");
						tree.FromLevelOrder(TREE_DATA_LEVEL);
						break;
					case 1:
						Console.WriteLine($@"Data from {"PreOrder".BrightCyan()} traversal:
{TREE_DATA_PRE}");
						tree.FromPreOrder(TREE_DATA_PRE);
						break;
					case 2:
						Console.WriteLine($@"Data from {"InOrder".BrightCyan()} traversal:
{TREE_DATA_IN}");
						tree.FromInOrder(TREE_DATA_IN);
						break;
					case 3:
						Console.WriteLine($@"Data from {"PostOrder".BrightCyan()} traversal:
{TREE_DATA_POST}");
						tree.FromPostOrder(TREE_DATA_POST);
						break;
					case 4:
						Console.WriteLine($@"Data from {"InOrder".BrightCyan()} and {"LevelOrder".BrightCyan()} traversals:
{TREE_DATA_IN}
{TREE_DATA_LEVEL}");
						tree.FromInOrderAndLevelOrder(TREE_DATA_IN, TREE_DATA_LEVEL);
						break;
					case 5:
						Console.WriteLine($@"Data from {"InOrder".BrightCyan()} and {"PreOrder".BrightCyan()} traversals:
{TREE_DATA_IN}
{TREE_DATA_PRE}");
						tree.FromInOrderAndPreOrder(TREE_DATA_IN, TREE_DATA_PRE);
						break;
					case 6:
						Console.WriteLine($@"Data from {"InOrder".BrightCyan()} and {"PostOrder".BrightCyan()} traversals:
{TREE_DATA_IN}
{TREE_DATA_POST}");
						tree.FromInOrderAndPostOrder(TREE_DATA_IN, TREE_DATA_POST);
						break;
				}

				tree.PrintWithProps();
				i++;

				if (i >= NUM_TESTS)
				{
					more = false;
					continue;
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestBinarySearchTreeAdd()
		{
			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Title("Testing BinarySearchTree.Add()...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestBinarySearchTreeRemove()
		{
			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Title("Testing BinarySearchTree.Remove()...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Length, $"Values are not added correctly! {values.Length} != {tree.Count}.");
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");
				int value = values.PickRandom();
				Console.WriteLine($"will look for {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				Console.WriteLine("Found.");
				
				int found = 0;
				Console.WriteLine("Test finding all values...");

				foreach (int v in values)
				{
					if (tree.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Length} items.");

				Console.WriteLine();
				Console.WriteLine("Test removing a random value...");
				value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.PrintWithProps();

				int removed = 1;
				Console.WriteLine();
				Console.WriteLine("Test removing all values...");

				foreach (int v in values)
				{
					if (v == value) continue;

					if (tree.Remove(v))
					{
						removed++;
						Debug.Assert(values.Length - removed == tree.Count, $"Values are not removed correctly! {values.Length - removed} != {tree.Count}.");
						continue;
					}

					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestBinarySearchTreeBalance()
		{
			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Title("Testing BinarySearchTree.Balance()...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test removing...".BrightRed());
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestAVLTreeAdd()
		{
			bool more;
			AVLTree<int> tree = new AVLTree<int>();

			do
			{
				Console.Clear();
				Title("Testing AVLTree.Add()...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestAVLTreeRemove()
		{
			bool more;
			AVLTree<int> tree = new AVLTree<int>();

			do
			{
				Console.Clear();
				Title("Testing AVLTree.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Length, $"Values are not added correctly! {values.Length} != {tree.Count}.");
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");
				int value = values.PickRandom();
				Console.WriteLine($"will look for {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				Console.WriteLine("Found.");

				int found = 0;
				Console.WriteLine("Test finding all values...");

				foreach (int v in values)
				{
					if (tree.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Length} items.");

				Console.WriteLine("Test removing...".BrightRed());
				value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.PrintWithProps();

				int removed = 1;
				Console.WriteLine();
				Console.WriteLine("Test removing all values...");

				foreach (int v in values)
				{
					if (v == value) continue;

					if (tree.Remove(v))
					{
						removed++;
						Debug.Assert(values.Length - removed == tree.Count, $"Values are not removed correctly! {values.Length - removed} != {tree.Count}.");
						continue;
					}
					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestRedBlackTreeAdd()
		{
			bool more;
			RedBlackTree<int> tree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Title("Testing RedBlackTree.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestRedBlackTreeRemove()
		{
			bool more;
			RedBlackTree<int> tree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Title("Testing RedBlackTree.Remove()...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Length, $"Values are not added correctly! {values.Length} != {tree.Count}.");
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");

				int value = values.PickRandom();
				Console.WriteLine($"will look for {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				Console.WriteLine("Found.");

				int found = 0;
				Console.WriteLine("Test finding all values...");

				foreach (int v in values)
				{
					if (tree.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Length} items.");

				Console.WriteLine();
				Console.WriteLine("Test removing...".BrightRed());
				value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.PrintWithProps();

				int removed = 1;
				Console.WriteLine();
				Console.WriteLine("Test removing all values...");

				foreach (int v in values)
				{
					if (v == value) continue;

					if (tree.Remove(v))
					{
						removed++;
						Debug.Assert(values.Length - removed == tree.Count, $"Values are not removed correctly! {values.Length - removed} != {tree.Count}.");
						continue;
					}
					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestAllBinaryTrees()
		{
			bool more;
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Title("Testing all BinaryTrees...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				DoTheTest(binarySearchTree, values);

				DoTheTest(avlTree, values);

				DoTheTest(redBlackTree, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] array)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {tree.GetType().Name}...".BrightGreen());
				tree.Clear();
				tree.Add(array);

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test removing...".BrightRed());
				int value = array.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.PrintWithProps();
			}
		}

		private static void TestAllBinaryTreesFunctionality()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();
			int[] values = GetRandomIntegers(true, 30);

			do
			{
				Console.Clear();
				Title("Testing all BinaryTrees performance...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());

				DoTheTest(binarySearchTree, values, clock);
				clock.Stop();
				ConsoleHelper.Pause();

				DoTheTest(avlTree, values, clock);
				clock.Stop();
				ConsoleHelper.Pause();

				DoTheTest(redBlackTree, values, clock);
				clock.Stop();
				ConsoleHelper.Pause();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] values, Stopwatch clock)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {tree.GetType().Name}...".BrightGreen());
				tree.Clear();

				Debug.Assert(tree.Count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				Console.WriteLine();
				Console.WriteLine($"Array: {string.Join(", ", values)}");
				
				clock.Restart();
				tree.Add(values);
				Console.WriteLine($"Added {tree.Count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
				tree.PrintProps();

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (tree.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				tree.Print();

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					Console.WriteLine($"Removing {v}:");

					if (tree.Remove(v))
					{
						removed++;
						tree.Print();
						Console.WriteLine("Tree Count = " + tree.Count);
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Remove missed a value: {v} :((".BrightRed()
										: "REMOVE MISSED A LOT. :((".BrightRed());
					Console.WriteLine("Does it contain the value? " + tree.Contains(v).ToYesNo());
					tree.Print();
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestAllBinaryTreesPerformance()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();
			int[] values = GetRandomIntegers(true, 200000);

			do
			{
				Console.Clear();
				Title("Testing all BinaryTrees performance...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");

				DoTheTest(binarySearchTree, values, clock);
				clock.Stop();

				DoTheTest(avlTree, values, clock);
				clock.Stop();

				DoTheTest(redBlackTree, values, clock);
				clock.Stop();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			clock.Stop();

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] values, Stopwatch clock)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {tree.GetType().Name}...".BrightGreen());
				tree.Clear();
				Debug.Assert(tree.Count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");

				clock.Restart();
				tree.Add(values);
				Console.WriteLine($"Added {tree.Count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
				tree.PrintProps();

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (tree.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Find missed a value: {v} :((".BrightRed()
										: "FIND MISSED A LOT :((".BrightRed());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				missed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (tree.Remove(v))
					{
						removed++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? $"Remove missed a value: {v} :((".BrightRed()
										: "REMOVE MISSED A LOT. :((".BrightRed());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestSortedSetPerformance()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			// this is a RedBlackTree implementation by Microsoft, just testing it.
			SortedSet<int> sortedSet = new SortedSet<int>();
			int[] values = GetRandomIntegers(true, 200000);

			do
			{
				Console.Clear();
				Title("Testing SortedSet performance...");
				Console.WriteLine("This is C#, so the test needs to run at least once before considering results in order for the code to be compiled and run at full speed.".Yellow());
				Console.WriteLine();
				Console.WriteLine($"Array has {values.Length} items.");

				DoTheTest(sortedSet, values, clock);
				clock.Stop();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
			
			clock.Stop();

			static void DoTheTest<T>(SortedSet<T> sortedSet, T[] values, Stopwatch clock)
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {sortedSet.GetType().Name}...".BrightGreen());
				sortedSet.Clear();

				int count = sortedSet.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				clock.Restart();

				foreach (T v in values)
				{
					sortedSet.Add(v);
					count++;
					Debug.Assert(count == sortedSet.Count, $"Values are not added correctly! {count} != {sortedSet.Count}.");
				}

				Console.WriteLine($"Added {count} items of {values.Length} in {clock.ElapsedMilliseconds} ms.");
				Console.WriteLine();
				Console.WriteLine($"{"Count:".Yellow()} {sortedSet.Count.ToString().Underline()}.");
				Console.WriteLine($"{"Minimum:".Yellow()} {sortedSet.Min} {"Maximum:".Yellow()} {sortedSet.Max}");

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				clock.Restart();

				foreach (T v in values)
				{
					if (sortedSet.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				clock.Restart();

				foreach (T v in values)
				{
					if (sortedSet.Remove(v))
					{
						removed++;
						Debug.Assert(count - removed == sortedSet.Count, $"Values are not removed correctly! {count} != {sortedSet.Count}.");
						continue;
					}
					
					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		//private static void TestTreeEquality()
		//{
		//	bool more;

		//	do
		//	{
		//		Console.Clear();
		//		Title("Testing tree equality...");
		
		//		int len = RNGRandomHelper.Next(1, 12);
		//		int[] values = GetRandomIntegers(true, len);
		//		Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

		//		Console.WriteLine();
		//		Console.WriteLine("Testing BinarySearchTree: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine();
		//		LinkedBinaryTree<int> tree1 = new BinarySearchTree<int>();
		//		LinkedBinaryTree<int> tree2 = new BinarySearchTree<int>();
		//		DoTheTest(tree1, tree2, values);

		//		Console.WriteLine();
		//		Console.WriteLine("Testing BinarySearchTree and AVLTree: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine();
		//		tree1.Clear();
		//		tree2 = new AVLTree<int>();
		//		DoTheTest(tree1, tree2, values);

		//		Console.WriteLine();
		//		Console.WriteLine("Testing AVLTree: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine();
		//		tree1 = new AVLTree<int>();
		//		tree2 = new AVLTree<int>();
		//		DoTheTest(tree1, tree2, values);

		//		Console.WriteLine();
		//		Console.WriteLine("Testing RedBlackTree: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine();
		//		RedBlackTree<int> rbTree1 = new RedBlackTree<int>();
		//		RedBlackTree<int> rbTree2 = new RedBlackTree<int>();
		//		DoTheTest(rbTree1, rbTree2, values);

		//		Console.WriteLine();
		//		Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
		//		ConsoleKeyInfo response = Console.ReadKey(true);
		//		Console.WriteLine();
		//		more = response.Key == ConsoleKey.Y;
		//	}
		//	while (more);

		//	static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree1, LinkedBinaryTree<TNode, int> tree2, int[] array)
		//		where TNode : LinkedBinaryNode<TNode, int>
		//	{
		//		Console.WriteLine();
		//		Console.WriteLine($"Testing {tree1.GetType().Name} and {tree1.GetType().Name}...".BrightGreen());
		//		tree1.Add(array);
		//		tree2.Add(array);

		//		Console.WriteLine("InOrder1: ".BrightBlack() + string.Join(", ", tree1));
		//		Console.WriteLine("InOrder2: ".BrightBlack() + string.Join(", ", tree2));
		//		tree1.PrintWithProps();
		//		tree2.PrintWithProps();
		//		Console.WriteLine($"tree1 == tree2? {tree1.Equals(tree2).ToYesNo()}");
		//	}
		//}

		private static void TestHeapAdd()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing Heap.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Heap<int> heap = new MaxHeap<int>();
				DoTheTest(heap, values);

				heap = new MinHeap<int>();
				DoTheTest(heap, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());

				foreach (T value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestHeapRemove()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing Heap.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Heap<int> heap = new MaxHeap<int>();
				DoTheTest(heap, values);

				heap = new MinHeap<int>();
				DoTheTest(heap, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(heap.Remove());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestPriorityQueue()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing PriorityQueue...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				PriorityQueue<int, int> intQueue = new MinPriorityQueue<int>();
				DoTheTest(intQueue, values);

				intQueue = new MaxPriorityQueue<int>();
				DoTheTest(intQueue, values);

				Student[] students = GetRandomStudents(len);
				PriorityQueue<double, Student> studentQueue = new MinPriorityQueue<double, Student>(e => e.Grade);
				DoTheTest(studentQueue, students);

				studentQueue = new MaxPriorityQueue<double, Student>(e => e.Grade);
				DoTheTest(studentQueue, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TPriority, T>(PriorityQueue<TPriority, T> queue, T[] array)
				where TPriority : struct, IComparable
			{
				Console.WriteLine($"Test adding ({queue.GetType()})...".BrightGreen());
				queue.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", queue));
				queue.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (queue.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(queue.Remove());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestHeapElementAt()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing Heap ElementAt...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Heap<int> heap = new MaxHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				PriorityQueue<double, Student> studentQueue = new MaxPriorityQueue<double, Student>(e => e.Grade);
				DoTheTest2(studentQueue, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array, int k)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test get Kth element...");
				Console.WriteLine($"heap {k} kth element = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}

			static void DoTheTest2<TPriority, T>(PriorityQueue<TPriority, T> queue, T[] array, int k)
				where TPriority : struct, IComparable
			{
				Console.WriteLine($"Test adding ({queue.GetType()})...".BrightGreen());
				queue.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", queue));
				queue.Print();
				Console.WriteLine("Test get Kth element...");
				Console.WriteLine($"heap {k} kth element = {queue.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestThreadQueue()
		{
			// change this to true to use 1 thread only for debugging
			const bool LIMIT_THREADS = false;

			int len = RNGRandomHelper.Next(100, 1000);
			int[] values = GetRandomIntegers(len);
			int timeout = RNGRandomHelper.Next(0, 1);
			string timeoutString = timeout > 0
										? $"{timeout} minute(s)"
										: "None";
			int maximumThreads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			Func<int, TaskResult> exec = e =>
			{
				Console.Write(", {0}", e);
				return TaskResult.Success;
			};
			Queue<ThreadQueueMode> modes = new Queue<ThreadQueueMode>(EnumHelper<ThreadQueueMode>.GetValues());
			Stopwatch clock = new Stopwatch();

#if DEBUG
			// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
			int threads = LIMIT_THREADS
							? 1
							: maximumThreads;
#else
					// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
					int threads = maximumThreads;
#endif

			if (threads < 1 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;

			while (modes.Count > 0)
			{
				Console.Clear();
				Console.WriteLine();
				ThreadQueueMode mode = modes.Dequeue();
				Title($"Testing multi-thread queue in '{mode.ToString().BrightCyan()}' mode...");

				// if there is a timeout, will use a CancellationTokenSource.
				using (CancellationTokenSource cts = timeout > 0
														? new CancellationTokenSource(TimeSpan.FromMinutes(timeout))
														: null)
				{
					CancellationToken token = cts?.Token ?? CancellationToken.None;
					ProducerConsumerQueueOptions<int> options = mode == ThreadQueueMode.ThresholdTaskGroup
																	? new ProducerConsumerThresholdQueueOptions<int>(threads, exec)
																	{
																		// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
																		Threshold = TimeSpan.FromSeconds(1)
																	}
																	: new ProducerConsumerQueueOptions<int>(threads, exec);
			
					// Create a generic queue producer
					using (IProducerConsumer<int> queue = ProducerConsumerQueue.Create(mode, options, token))
					{
						queue.WorkStarted += (sender, args) =>
						{
							Console.WriteLine();
							Console.WriteLine($"Starting multi-thread test. mode: '{mode.ToString().BrightCyan()}', values: {values.Length.ToString().BrightCyan()}, threads: {options.Threads.ToString().BrightCyan()}, timeout: {timeoutString.BrightCyan()}...");
							if (mode == ThreadQueueMode.ThresholdTaskGroup) Console.WriteLine($"in {mode} mode, {threads.ToString().BrightCyan()} tasks will be issued every {((ProducerConsumerThresholdQueueOptions<int>)options).Threshold.TotalSeconds.ToString("N0").BrightCyan()} second(s).");
							Console.WriteLine();
							Console.WriteLine();
							clock.Restart();
						};

						queue.WorkCompleted += (sender, args) =>
						{
							long elapsed = clock.ElapsedMilliseconds;
							Console.WriteLine();
							Console.WriteLine();
							Console.WriteLine($"Finished test. mode: '{mode.ToString().BrightCyan()}', values: {values.Length.ToString().BrightCyan()}, threads: {options.Threads.ToString().BrightCyan()}, timeout: {timeoutString.BrightCyan()}, elapsed: {elapsed.ToString().BrightCyan()} ms.");
							Console.WriteLine();
						};

						foreach (int value in values)
						{
							queue.Enqueue(value);
						}

						/*
						 * when the queue is being disposed, it will wait until the queued items are processed.
						 * this works when queue.WaitOnDispose is true , which it is by default.
						 * alternatively, the following can be done to wait for all items to be processed:
						 *
						 * // Important: marks the completion of queued items, no further items can be queued
						 * // after this point. the queue will not to wait for more items other than the already queued.
						 * queue.Complete();
						 * // wait for the queue to finish
						 * queue.Wait();
						 *
						 * another way to go about it, is not to call queue.Complete(); if this queue will
						 * wait indefinitely and maybe use a CancellationTokenSource.
						 *
						 * for now, the queue will wait for the items to be finished once reached the next
						 * dispose curly bracket.
						 */
					}
				}

				if (modes.Count == 0) continue;
				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				if (response.Key != ConsoleKey.Y) modes.Clear();
			}

			clock.Stop();
		}

		private static void TestTrie()
		{
			const int MAX_LIST = 100;

			bool more;
			Trie<char> trie = new Trie<char>(CharComparer.InvariantCultureIgnoreCase);
			ISet<string> values = new HashSet<string>();

			do
			{
				Console.Clear();
				Title("Testing Trie...");
				if (values.Count == 0) AddWords(trie, values);
				Console.WriteLine("Words list: ".BrightBlack() + string.Join(", ", values));

				string word = values.PickRandom();
				DoTheTest(trie, word, values);
				
				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || values.Count >= MAX_LIST) continue;

				Console.Write($"Would you like to add more words? {"[Y]".BrightGreen()} / {"any key".Dim()} ");
				response = Console.ReadKey(true);
				Console.WriteLine();
				if (response.Key != ConsoleKey.Y) continue;
				Console.WriteLine();
				AddWords(trie, values);
			}
			while (more);

			static void AddWords(Trie<char> trie, ISet<string> set)
			{
				int len = RNGRandomHelper.Next(10, 20);
				Console.WriteLine($"Generating {len} words: ".BrightGreen());
				ICollection<string> newValues = GetRandomStrings(true, len);

				foreach (string value in newValues)
				{
					if (!set.Add(value)) continue;
					trie.Add(value);
				}
			}

			static void DoTheTest(Trie<char> trie, string token, ISet<string> values)
			{
				Console.WriteLine($"Test find '{token.BrightCyan().Underline()}'...");

				if (!trie.Contains(token))
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}
				
				Console.WriteLine("Found...!".BrightGreen() + " Let's try all caps...");

				if (!trie.Contains(token.ToUpperInvariant()))
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				Console.WriteLine("Found...!".BrightGreen() + " Let's try words with a common prefix...");

				string prefix = token;

				if (prefix.Length > 1)
				{
					Match match = Regex.Match(prefix, @"^([\w\-]+)");
					prefix = !match.Success || match.Value.Length == prefix.Length
								? prefix.Left(prefix.Length / 2)
								: match.Value;
				}

				Console.WriteLine($"Prefix: '{prefix.BrightCyan().Underline()}'");
				int results = 0;

				foreach (IEnumerable<char> enumerable in trie.Find(prefix))
				{
					Console.WriteLine($"{++results}: " + string.Concat(enumerable));
				}

				if (results == 0)
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				int tries = 3;

				while (prefix.Length > 1 && results < 2 && --tries > 0)
				{
					results = 0;
					prefix = prefix.Left(prefix.Length / 2);
					Console.WriteLine();
					Console.WriteLine($"Results were too few, let's try another prefix: '{prefix.BrightCyan().Underline()}'");

					foreach (IEnumerable<char> enumerable in trie.Find(prefix))
					{
						Console.WriteLine($"{++results}: " + string.Concat(enumerable));
					}
				}

				Console.WriteLine();
				Console.WriteLine($"Test remove '{token.BrightRed().Underline()}'");

				if (!trie.Remove(token))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				values.Remove(token);
				results = 0;
				Console.WriteLine();
				Console.WriteLine($"Cool {"removed".BrightGreen()}, let's try to find the last prefix again: '{prefix.BrightCyan().Underline()}'");

				foreach (IEnumerable<char> enumerable in trie.Find(prefix))
				{
					Console.WriteLine($"{++results}: " + string.Concat(enumerable));
				}

				Console.WriteLine();
				Console.WriteLine("Isn't that cool? :))");
			}
		}

		private static void TestTrieSimilarWordsRemoval()
		{
			string[] values = {
				"Car",
				"Care",
				"calcification",
				"campylobacter",
				"cartilaginous",
				"catecholamine",
				"carpetbagging",
				"carbonization",
				"catastrophism",
				"carboxylation",
				"cardiovascular",
				"cardiomyopathy",
				"capitalization",
				"carcinomatosis",
				"cardiothoracic",
				"carcinosarcoma",
				"cartelizations",
				"caprifications",
				"candlesnuffers",
				"canthaxanthins",
				"Can",
				"Canvas"
			};
			Trie<char> trie = new Trie<char>(CharComparer.InvariantCultureIgnoreCase);

			Console.Clear();
			Console.WriteLine("Adding similar words...");
			Console.WriteLine("Words list: ".BrightBlack() + string.Join(", ", values));

			foreach (string value in values) 
				trie.Add(value);

			int results = 0;
			string prefix = "car";
			Console.WriteLine();
			Console.WriteLine($"Test find '{prefix.BrightCyan().Underline()}'...");

			foreach (IEnumerable<char> enumerable in trie.Find(prefix))
			{
				Console.WriteLine($"{++results}: " + string.Concat(enumerable));
			}

			if (results == 0)
			{
				Console.WriteLine("Didn't find a shit...!".BrightRed());
				return;
			}

			string word = values[0];
			Console.WriteLine();
			Console.WriteLine($"Test remove '{word.BrightRed().Underline()}'");

			if (!trie.Remove(word))
			{
				Console.WriteLine("Didn't remove a shit...!".BrightRed());
				return;
			}

			results = 0;
			Console.WriteLine($"Cool {"removed".BrightGreen()}.");
			Console.WriteLine();
			Console.WriteLine($"let's try to find the last prefix again: '{prefix.BrightCyan().Underline()}'");

			foreach (IEnumerable<char> enumerable in trie.Find(prefix))
			{
				Console.WriteLine($"{++results}: " + string.Concat(enumerable));
			}

			word = values[values.Length - 1];
			Console.WriteLine();
			Console.WriteLine($"Test remove '{word.BrightRed().Underline()}'");

			if (!trie.Remove(word))
			{
				Console.WriteLine("Didn't remove a shit...!".BrightRed());
				return;
			}

			prefix = "ca";
			results = 0;
			Console.WriteLine($"Cool {"removed".BrightGreen()}.");
			Console.WriteLine();
			Console.WriteLine($"Test find '{prefix.BrightCyan().Underline()}'...");

			foreach (IEnumerable<char> enumerable in trie.Find(prefix))
			{
				Console.WriteLine($"{++results}: " + string.Concat(enumerable));
			}
		}

		private static void TestGraph()
		{
			const int MAX_LIST = 26;

			bool more;
			GraphList<GraphVertex<char>, GraphEdge<char>, char> graph;
			WeightedGraphList<GraphWeightedEdge<char>, int, char> weightedGraph;
			List<char> values = new List<char>();
			Menu menu = new Menu()
				.Add("Undirected graph", () =>
				{
					Console.WriteLine();
					graph = new UndirectedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(graph, values);
				})
				.Add("Directed graph", () =>
				{
					Console.WriteLine();
					graph = new DirectedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(graph, values);
				})
				.Add("Mixed graph", () =>
				{
					Console.WriteLine();
					graph = new MixedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(graph, values);
				})
				.Add("Weighted undirected graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedUndirectedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(weightedGraph, values);
				})
				.Add("Weighted directed graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedDirectedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(weightedGraph, values);
				})
				.Add("Weighted mixed graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedMixedGraphList<char>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(weightedGraph, values);
				});

			do
			{
				Console.Clear();
				Title("Testing graph add()");
				menu.Display();
				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || values.Count >= MAX_LIST) continue;

				Console.Write($"Would you like to add more character? {"[Y]".BrightGreen()} / {"any key".Dim()} ");
				response = Console.ReadKey(true);
				Console.WriteLine();
				if (response.Key != ConsoleKey.Y) continue;
				Console.WriteLine();
				AddValues(values);
			}
			while (more);

			static void AddValues(List<char> list)
			{
				int len = RNGRandomHelper.Next(1, 12);
				Console.WriteLine("Generating new characters: ".BrightGreen());
				char[] newValues = GetRandomChar(true, len);
				int count = 0;

				foreach (char value in newValues)
				{
					if (list.Contains(value)) continue;
					list.Add(value);
					count++;
				}

				Console.WriteLine($"Added {count} characters to the set".BrightGreen());
			}

			static bool DoTheTest<TEdge>(GraphList<GraphVertex<char>, TEdge, char> graph, List<char> values)
				where TEdge : GraphEdge<GraphVertex<char>, TEdge, char>
			{
				Console.WriteLine("Test adding nodes...");
				Console.WriteLine("characters list: ".BrightBlack() + string.Join(", ", values));
				graph.Clear();
				graph.Add(values);

				if (graph.Count != values.Count)
				{
					Console.WriteLine("Something went wrong, not all nodes were added...!".BrightRed());
					return false;
				}

				if (graph.Count == 1)
				{
					Console.WriteLine("Huh, must add more nodes...!".BrightRed());
					return true;
				}
				
				Console.WriteLine("All nodes are added...!".BrightGreen() + " Let's try adding some relationships...");
				Console.Write($@"Would you like to add a bit of randomization? {"[Y]".BrightGreen()} / {"any key".Dim()}.
This may cause cycles but also will make it much more fun for weighted shortest paths. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				int threshold = response.Key != ConsoleKey.Y ? 0 : (int)Math.Floor(values.Count * 0.5d);
				
				Queue<char> queue = new Queue<char>(values);
				char from = queue.Dequeue();
				Action<char, char> addEdge = graph switch
				{
					MixedGraphList<TEdge, char> mGraph => (f, t) => mGraph.AddEdge(f, t, __fakeGenerator.Value.Random.Bool()),
					WeightedMixedGraphList<GraphWeightedEdge<char>, int, char> wmGraph => (f, t) => wmGraph.AddEdge(f, t, RNGRandomHelper.Next(byte.MaxValue), __fakeGenerator.Value.Random.Bool()),
					WeightedGraphList<GraphWeightedEdge<char>, int, char> wGraph => (f, t) => wGraph.AddEdge(f, t, RNGRandomHelper.Next(byte.MaxValue)),
					_ => graph.AddEdge
				};

				while (queue.Count > 0)
				{
					char to = queue.Dequeue();
					if (graph.ContainsEdge(from, to)) continue;
					Console.WriteLine($"Adding {from.ToString().BrightCyan().Underline()} to {to.ToString().BrightCyan().Underline()}...");
					addEdge(from, to);

					if (threshold > 0 && __fakeGenerator.Value.Random.Bool())
					{
						queue.Enqueue(from);
						queue.Enqueue(values.PickRandom());
						threshold--;
					}

					from = to;
				}

				graph.Print();
				Console.WriteLine();
				Console.WriteLine("Cool, let's try enumerating it.");
				char value = graph.Top().First();
				Console.WriteLine($"Picking a value with maximum connections: '{value.ToString().BrightCyan().Underline()}'...");
				if (!DoTheTestWithValue(graph, values, value)) return false;

				do
				{
					Console.WriteLine();
					Console.Write($@"Type in {"a character".BrightGreen()} to traverse from,
or press {"ESCAPE".BrightRed()} key to exit this test. ");
					response = Console.ReadKey();
					Console.WriteLine();
					if (response.Key == ConsoleKey.Escape) continue;

					if (!char.IsLetter(response.KeyChar) || !graph.ContainsEdge(response.KeyChar))
					{
						Console.WriteLine($"Character '{value}' is not found or not connected!");
						continue;
					}

					value = response.KeyChar;
					if (!DoTheTestWithValue(graph, values, value)) return false;
				}
				while (response.Key != ConsoleKey.Escape);

				Console.WriteLine();
				return true;
			}

			static bool DoTheTestWithValue<TEdge>(GraphList<GraphVertex<char>, TEdge, char> graph, List<char> values, char value)
				where TEdge : GraphEdge<GraphVertex<char>, TEdge, char>
			{
				Console.WriteLine("Breadth First: ".Yellow() + string.Join(", ", graph.Enumerate(value, GraphTraverseMethod.BreadthFirst)));
				Console.WriteLine("Depth First: ".Yellow() + string.Join(", ", graph.Enumerate(value, GraphTraverseMethod.DepthFirst)));
				Console.WriteLine("Degree: ".Yellow() + graph.Degree(value));

				switch (graph)
				{
					case DirectedGraphList<TEdge, char> directedGraph:
						Console.WriteLine("InDegree: ".Yellow() + directedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", directedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case MixedGraphList<TEdge, char> mixedGraph:
						Console.WriteLine("InDegree: ".Yellow() + mixedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", mixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case WeightedDirectedGraphList<GraphWeightedEdge<char>, int, char> weightedDirectedGraph:
						Console.WriteLine("InDegree: ".Yellow() + weightedDirectedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", weightedDirectedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case WeightedMixedGraphList<GraphWeightedEdge<char>, int, char> weightedMixedGraph:
						Console.WriteLine("InDegree: ".Yellow() + weightedMixedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", weightedMixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
				}

				if (graph is WeightedGraphList<GraphWeightedEdge<char>, int, char> wGraph)
				{
					char to = values.PickRandom();
					ConsoleKeyInfo response;

					do
					{
						Console.Write($@"Type in {"a character".BrightGreen()} to find the shortest path from '{value.ToString().BrightGreen()}' to it,
press the {"RETURN".BrightGreen()} key to accept the current random value '{to.ToString().BrightGreen()}'
or press {"ESCAPE".BrightRed()} key to exit this test. ");
						response = Console.ReadKey();
						Console.WriteLine();
						if (response.Key == ConsoleKey.Escape) return false;
						if (response.Key == ConsoleKey.Enter) continue;
						if (!char.IsLetter(response.KeyChar) || !wGraph.ContainsEdge(response.KeyChar)) Console.WriteLine($"Character '{value}' is not found or not connected!");
						to = response.KeyChar;
						break;
					}
					while (response.Key != ConsoleKey.Enter);

					Console.WriteLine($"{"Shortest Path:".Yellow()} from {value.ToString().BrightCyan()} to {to.ToString().BrightCyan()}");
					Console.WriteLine(string.Join(" -> ", wGraph.GetShortestPath(value, to, ShortestPathAlgorithm.Dijkstra)));
				}

				return true;
			}
		}

		private static void TestSkipList()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			SkipList<int> skipList = new SkipList<int>();
			int[] values = Enumerable.Range(1, 200_000).ToArray();

			do
			{
				Console.Clear();
				Title("Testing SkipList...");
				Console.WriteLine($"Array has {values.Length} items.");
				skipList.Clear();

				int count = skipList.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				clock.Restart();

				foreach (int v in values)
				{
					skipList.Add(v);
					count++;
					Debug.Assert(count == skipList.Count, $"Values are not added correctly! {count} != {skipList.Count}.");
				}

				Console.WriteLine();
				Console.WriteLine($"Added {count} items of {values.Length} in {clock.ElapsedMilliseconds} ms. Count = {skipList.Count}, Level = {skipList.Level}.");
				//skipList.WriteTo(Console.Out);

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (skipList.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (skipList.Remove(v))
					{
						removed++;
						Debug.Assert(count - removed == skipList.Count, $"Values are not removed correctly! {count} != {skipList.Count}.");
						continue;
					}
					
					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.WriteLine("Test to clear the list...");
				skipList.Clear();

				if (skipList.Count != 0)
				{
					Console.WriteLine($"Something went wrong, the count is {skipList.Count}...!".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
			
			clock.Stop();
		}

		private static void Title(string title)
		{
			Console.WriteLine();
			Console.WriteLine(title.Bold().BrightBlack());
			Console.WriteLine();
		}

		[NotNull]
		private static int[] GetRandomIntegers(int len = 0) { return GetRandomIntegers(false, len); }
		[NotNull]
		private static int[] GetRandomIntegers(bool unique, int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);

			int[] values = new int[len];

			if (unique)
			{
				values = Enumerable.Range(1, len).ToArray();
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
		private static char[] GetRandomChar(int len = 0) { return GetRandomChar(false, len); }
		[NotNull]
		private static char[] GetRandomChar(bool unique, int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
			
			char[] values = new char[len];

			if (unique)
			{
				int i = 0;
				HashSet<char> set = new HashSet<char>();

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
		private static ICollection<string> GetRandomStrings(int len = 0) { return GetRandomStrings(false, len); }
		[NotNull]
		private static ICollection<string> GetRandomStrings(bool unique, int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
			if (!unique) return __fakeGenerator.Value.Random.WordsArray(len);

			HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			while (set.Count < len) 
				set.Add(__fakeGenerator.Value.Random.Word());

			return set;
		}

		private class Student
		{
			public string Name { get; set; }

			public double Grade { get; set; }

			/// <inheritdoc />
			public override string ToString() { return $"{Name} > {Grade:F2}"; }
		}

		[NotNull]
		private static Student[] GetRandomStudents(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
			
			Student[] students = new Student[len];

			for (int i = 0; i < len; i++)
			{
				students[i] = new Student
				{
					Name = __fakeGenerator.Value.Name.FirstName(__fakeGenerator.Value.PickRandom<Name.Gender>()),
					Grade = __fakeGenerator.Value.Random.Double(0.0d, 100.0d)
				};
			}

			return students;
		}
	}
}

public static class Extension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	[NotNull]
	public static string ToYesNo(this bool thisValue)
	{
		return thisValue
					? "Yes".BrightGreen()
					: "No".BrightRed();
	}

	public static void PrintProps<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
		where TNode : LinkedBinaryNode<TNode, T>
	{
		Console.WriteLine();
		Console.WriteLine($"{"Dimensions:".Yellow()} {thisValue.Count.ToString().Underline()} x {thisValue.GetHeight().ToString().Underline()}.");
		Console.WriteLine($"{"Balanced:".Yellow()} {thisValue.IsBalanced().ToYesNo()}");
		Console.WriteLine($"{"Valid:".Yellow()} {thisValue.Validate().ToYesNo()}");
		Console.WriteLine($"{"Minimum:".Yellow()} {thisValue.Minimum()} {"Maximum:".Yellow()} {thisValue.Maximum()}");
	}

	public static void PrintWithProps<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
		where TNode : LinkedBinaryNode<TNode, T>
	{
		PrintProps(thisValue);
		thisValue.Print();
	}

	public static void Print<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
		where TNode : LinkedBinaryNode<TNode, T>
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void PrintProps<T>([NotNull] this ArrayBinaryTree<T> thisValue)
	{
		Console.WriteLine();
		Console.WriteLine($"{"Dimensions:".Yellow()} {thisValue.Count.ToString().Underline()} x {thisValue.GetHeight().ToString().Underline()}.");
		Console.WriteLine($"{"Valid:".Yellow()} {thisValue.Validate().ToYesNo()}");
		Console.WriteLine($"{"Minimum:".Yellow()} {thisValue.Minimum()} {"Maximum:".Yellow()} {thisValue.Maximum()}");
	}

	public static void PrintWithProps<T>([NotNull] this ArrayBinaryTree<T> thisValue)
	{
		PrintProps(thisValue);
		thisValue.Print();
	}

	public static void Print<T>([NotNull] this ArrayBinaryTree<T> thisValue)
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void PrintProps<TVertex, TEdge, T>([NotNull] this GraphList<TVertex, TEdge, T> thisValue)
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		Console.WriteLine();
		Console.WriteLine($"{"Order:".Yellow()} {thisValue.Count.ToString().Underline()}.");
		Console.WriteLine($"{"Size:".Yellow()} {thisValue.GetSize().ToString().Underline()}.");
	}

	public static void PrintWithProps<TVertex, TEdge, T>([NotNull] this GraphList<TVertex, TEdge, T> thisValue)
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		PrintProps(thisValue);
		thisValue.Print();
	}

	public static void Print<TVertex, TEdge, T>([NotNull] this GraphList<TVertex, TEdge, T> thisValue)
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}
}
