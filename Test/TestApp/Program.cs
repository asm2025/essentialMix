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
		private const int START = 10;
		private const int SMALL = 10_000;
		private const int MEDIUM = 100_000;
		private const int HEAVY = 1_000_000;

		private static readonly string COMPILATION_TEXT = $@"
This is C# (a compiled language), so the test needs to run at least
once before considering results in order for the code to be compiled
and run at full speed. The first time this test run, it will start 
with just {START} items and the next time when you press '{"Y".BrightGreen()}', it will 
work with {HEAVY} items.".Yellow();

		private static readonly Lazy<Faker> __fakeGenerator = new Lazy<Faker>(() => new Faker(), LazyThreadSafetyMode.PublicationOnly);
		private static readonly string[] __sortAlgorithms = 
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
			nameof(IListExtension.SortBrick),
		};

		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;

			//TestDomainName();

			//TestFibonacci();
			//TestGroupAnagrams();
			//TestKadaneMaximum();
			//TestLevenshteinDistance();
			//TestDeepestPit();

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
			//TestBinaryTreeFindClosest();
			//TestBinaryTreeBranchSums();
			//TestBinaryTreeInvert();

			//TestAVLTreeAdd();
			//TestAVLTreeRemove();
			
			//TestRedBlackTreeAdd();
			//TestRedBlackTreeRemove();

			//TestAllBinaryTrees();
			//TestAllBinaryTreesFunctionality();
			//TestAllBinaryTreesPerformance();

			//TestSortedSetPerformance();

			//TestTreeEquality();

			//TestTrie();
			//TestTrieSimilarWordsRemoval();

			//TestSkipList();

			//TestDisjointSet();
			
			//TestBinaryHeapAdd();
			//TestBinaryHeapRemove();
			//TestBinaryHeapElementAt();
			//TestBinaryHeapDecreaseKey();
		
			//TestBinomialHeapAdd();
			//TestBinomialHeapRemove();
			//TestBinomialHeapElementAt();
			//TestBinomialHeapDecreaseKey();
			
			//TestPairingHeapAdd();
			//TestPairingHeapRemove();
			//TestPairingHeapElementAt();
			//TestPairingHeapDecreaseKey();
			
			//TestFibonacciHeapAdd();
			//TestFibonacciHeapRemove();
			//TestFibonacciHeapElementAt();
			//TestFibonacciHeapDecreaseKey();
			
			// IndexMin not working yet!!!
			//TestIndexMinAdd();
			//TestIndexMinRemove();
			//TestIndexMinElementAt();
			//TestIndexMinDecreaseKey();

			//TestAllHeapsPerformance();

			//TestGraph();

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

		private static void TestFibonacci()
		{
			bool more;
			Console.Clear();
			Console.WriteLine("Testing Fibonacci number: ");

			do
			{
				Console.Write($"Type in {"a number".BrightGreen()} to calculate the Fibonacci number for or {"ESCAPE".BrightRed()} key to exit. ");
				string response = Console.ReadLine();
				more = !string.IsNullOrWhiteSpace(response);
				if (more && uint.TryParse(response, out uint value)) Console.WriteLine(asm.Numeric.Math.Fibonacci(value));
			}
			while (more);
		}

		private static void TestGroupAnagrams()
		{
			string[][] allWords =
			{
				null, //null
				new []{""}, //[]
				new []{"test"}, // ["test"]
				new []{"abc", "dabd", "bca", "cab", "ddba"}, //["abc", "bca", "cab"], ["dabd", "ddba"]
				new []{"abc", "cba", "bca"}, //["abc", "cba", "bca"]
				new []{"zxc", "asd", "weq", "sda", "qwe", "xcz"}, //["zxc", "xcz"], ["asd", "sda"], ["weq", "qwe"]
				new []{"yo", "act", "flop", "tac", "cat", "oy", "olfp"}, //["yo", "oy"], ["flop", "olfp"], ["act", "tac", "cat"]
				new []{"cinema", "a", "flop", "iceman", "meacyne", "lofp", "olfp"}, //["cinema", "iceman"], ["flop", "lofp", "olfp"], ["a"], ["meacyne"]
				new []{"abc", "abe", "abf", "abg"}, //["abc"], ["abe"], ["abf"], ["abg"]
			};
			int i = -1;
			bool more;
			Console.Clear();
			Console.WriteLine("Testing Group Anagrams: ");

			do
			{
				Console.Clear();
				Title("Testing group anagrams...");
				i = (i + 1) % allWords.Length;
				string[] words = allWords[i];
				Console.Write("Words: ".BrightBlack());
				
				if (words == null) Console.WriteLine("<null>");
				else if (words.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", words) + "]");
				
				IReadOnlyCollection<IReadOnlyList<string>> anagrams = StringHelper.GroupAnagrams(words);
				Console.Write("Anagrams: ".BrightYellow());

				if (anagrams == null) Console.WriteLine("<null>");
				else if (anagrams.Count == 0) Console.WriteLine("[]");
				else Console.WriteLine(string.Join(", ", anagrams.Select(e => "[" + string.Join(", ", e) + "]")));
				
				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestKadaneMaximum()
		{
			int[][] allNumbers =
			{
				new int[0], //0
				new []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, //55
				new []{-1, -2, -3, -4, -5, -6, -7, -8, -9, -10}, //-1
				new []{-10, -2, -9, -4, -8, -6, -7, -1, -3, -5}, //-1
				new []{1, 2, 3, 4, 5, 6, -20, 7, 8, 9, 10}, //35
				new []{1, 2, 3, 4, 5, 6, -22, 7, 8, 9, 10}, //34
				new []{1, 2, -4, 3, 5, -9, 8, 1, 2}, //11
				new []{3, 4, -6, 7, 8}, //16
			};
			int i = -1;
			bool more;
			Console.Clear();
			Console.WriteLine("Testing Kadane algorithm: maximum sum that can be obtained by summing up adjacent numbers");

			do
			{
				i = (i + 1) % allNumbers.Length;
				int[] numbers = allNumbers[i];
				Console.Write("Numbers: ".BrightBlack());
				
				if (numbers.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", numbers) + "]");
				
				Console.WriteLine("Sum: ".BrightYellow() + numbers.KadaneMaximumSum());
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestLevenshteinDistance()
		{
			(string First, string Second)[] allStrings =
			{
				(string.Empty, string.Empty), //0
				(string.Empty, "abc"), //3
				("abc", "abc"), //0
				("abc", "abx"), //1
				("abc", "abcx"), //1
				("abc", "yabcx"), //2
				("algoexpert", "algozexpert"), //1
				("abcdefghij", "1234567890"), //10
				("abcdefghij", "a234567890"), //9
				("biting", "mitten"), //4
				("cereal", "saturday"), //6
				("cereal", "saturdzz"), //7
				("abbbbbbbbb", "bbbbbbbbba"), //2
				("abc", "yabd"), //2
				("xabc", "abcx"), //2
			};

			int i = -1;
			bool more;
			Console.Clear();
			Console.WriteLine(@"Testing Levenshtein distance: minimum number of edit operations 
(insert/delete/substitute) to change first string to obtain the second string.");

			do
			{
				i = (i + 1) % allStrings.Length;
				(string first, string second) = allStrings[i];
				Console.WriteLine($"Strings: '{first}', '{second}'");
				Console.WriteLine("Levenshtein Distance: ".BrightYellow() + StringHelper.LevenshteinDistance(first, second));
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestDeepestPit()
		{
			(string Label, int[] Array)[] allNumbers =
			{
				("Test case none: ", new int[0]), //-1
				("Test case 1: ", new []{0, 1, 3, -2, 0, 1, 0, -3, 2, 3}), //4
				("Test case 2: ", new []{0, 1, 3, -2, 0, 1, 0, 0, -3, 2, 3}), //3
				("Test case 3: ", new []{0, 1, 3, -2, 0, 0, 1, 0, 0, -3, 2, 3}), //3
				("Test case 4: ", new []{0, 1, 3, -2, 0, 0, 1, 0, -3, -1, 1, 2, 3}), //4
				("Test case 5: ", new []{0, 1, 3, -2, 0, 0, 1, 0, -3, -1, -1, 1, 2, 3}), //2
				("Monotonically decreasing: ", new []{0, -1, -2, -3, -4, -5, -6, -7, -8, -9}), //-1
				("Monotonically increasing: ", new []{0, 1, 2, 3}), //-1
				("All the same, zeros: ", new []{0, 0, 0, 0}), //-1
				("Extreme no pit, monotonically increasing: ", new []{-100000000, 0, 100000000}), //-1
				("Extreme depth 1 w/o pit: ", new []{100000000, 0, 0, 100000000}), //-1
				("Extreme depth 1 w pit: ", new []{100000000, 0, 100000000}), //100000000
				("Extreme depth 2 w pit: ", new []{100000000, -100000000, 0, 0, 100000000}), //100000000
				("Extreme depth 3 w pit: ", new []{100000000, -100000000, 100000000, 0, 100000000}), //200000000
				("Extreme depth 2 w false first pit: ", new []{100000000, -100000000, -100000000, 100000000, 0, 100000000}), //100000000
				("Volcano shape: ", new []{0, 1, 2, 3, 10, 100, 90, 100, 3, 2, 1, 0}), //10
				("Mountain shape: ", new []{0, 1, 2, 3, 10, 100, 3, 2, 1, 0}), //-1
				("Plateau: ", new []{0, 1, 2, 3, 10, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 3, 2, 1, 0}), //-1
			};
			int i = -1;
			bool more;
			Console.Clear();
			Console.WriteLine("Testing Deepest Pit: ");

			do
			{
				i = (i + 1) % allNumbers.Length;
				(string label, int[] numbers) = allNumbers[i];
				Console.Write(label.BrightBlack());
				
				if (numbers.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", numbers) + "]");
				
				Console.WriteLine("Deepest Pit: ".BrightYellow() + numbers.DeepestPit());
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
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
			int threads;
#if DEBUG
			// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
			threads = LIMIT_THREADS
						? 1
						: RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
#else
			// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
			threads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
#endif

			Func<int, TaskResult> exec = e =>
			{
				Console.Write(", {0}", e);
				return TaskResult.Success;
			};
			Queue<ThreadQueueMode> modes = new Queue<ThreadQueueMode>(EnumHelper<ThreadQueueMode>.GetValues());
			Stopwatch clock = new Stopwatch();

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
						
						queue.Complete();
						
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
		
		private static void TestSortAlgorithm()
		{
			const string algorithm = nameof(IListExtension.SortInsertion);

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
		}

		private static void TestSortAlgorithms()
		{
			const int RESULT_COUNT = 10;

			Title("Testing Sort Algorithms...");

			Stopwatch watch = new Stopwatch();
			IComparer<int> numbersComparer = Comparer<int>.Default;
			IComparer<string> stringComparer = StringComparer.Ordinal;
			IDictionary<string, long> numericResults = new Dictionary<string, long>();
			IDictionary<string, long> stringResults = new Dictionary<string, long>();
			string sectionSeparator = new string('*', 80).BrightMagenta();
			bool more;
			int tests = 0;
			int[] numbers = GetRandomIntegers(START);
			string[] strings = GetRandomStrings(START).ToArray();

			do
			{
				Console.Clear();

				if (tests == 0)
				{
					Console.WriteLine("Numbers: ".BrightCyan() + string.Join(", ", numbers));
					Console.WriteLine("String: ".BrightCyan() + string.Join(", ", strings.Select(e => e.SingleQuote())));
					CompilationHint();
				}

				foreach (string algorithm in __sortAlgorithms)
				{
					GC.Collect();
					Action<IList<int>, int, int, IComparer<int>, bool> sortNumbers = GetAlgorithm<int>(algorithm);
					Action<IList<string>, int, int, IComparer<string>, bool> sortStrings = GetAlgorithm<string>(algorithm);
					Console.WriteLine(sectionSeparator);
					Console.WriteLine($"Testing {algorithm.BrightCyan()} algorithm: ");

					Console.Write("Numbers");
					int[] ints = (int[])numbers.Clone();
					watch.Restart();
					sortNumbers(ints, 0, -1, numbersComparer, false);
					numericResults[algorithm] = watch.ElapsedTicks;
					Console.WriteLine($" => {numericResults[algorithm].ToString().BrightGreen()}");
					if (tests == 0) Console.WriteLine("Result: " + string.Join(", ", ints));

					Console.Write("Strings");

					string[] str = (string[])strings.Clone();
					watch.Restart();
					sortStrings(str, 0, -1, stringComparer, false);
					stringResults[algorithm] = watch.ElapsedTicks;
					Console.WriteLine($" => {stringResults[algorithm].ToString().BrightGreen()}");
					if (tests == 0) Console.WriteLine("Result: " + string.Join(", ", str.Select(e => e.SingleQuote())));
					Console.WriteLine();
				}

				Console.WriteLine(sectionSeparator);
				Console.WriteLine("Finished".BrightYellow());
				Console.WriteLine();
				Console.WriteLine($"Fastest {RESULT_COUNT} numeric sort:".BrightGreen());
			
				foreach (KeyValuePair<string, long> pair in numericResults
															.OrderBy(e => e.Value)
															.Take(RESULT_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value}");
				}

				Console.WriteLine();
				Console.WriteLine($"Fastest {RESULT_COUNT} string sort:".BrightGreen());

				foreach (KeyValuePair<string, long> pair in stringResults
															.OrderBy(e => e.Value)
															.Take(RESULT_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value}");
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 2) continue;

				if (tests > 0)
				{
					Console.Write($"Would you like to increase the array size? {"[Y]".BrightGreen()} or {"any other key".Dim()} to exit. ");
					response = Console.ReadKey(true);
					Console.WriteLine();
					if (response.Key != ConsoleKey.Y) continue;
				}

				switch (tests)
				{
					case 0:
						numbers = GetRandomIntegers(SMALL);
						strings = GetRandomStrings(SMALL).ToArray();
						break;
					case 1:
						numbers = GetRandomIntegers(MEDIUM);
						strings = GetRandomStrings(MEDIUM).ToArray();
						break;
					case 2:
						numbers = GetRandomIntegers(HEAVY);
						strings = GetRandomStrings(HEAVY).ToArray();
						break;
				}

				tests++;
			}
			while (more);
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
			MinMaxQueue<int> queue = new MinMaxQueue<int>();

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
			
			MinMaxStack<int> stack = new MinMaxStack<int>();

			foreach (int value in values)
			{
				stack.Push(value);
				Console.WriteLine($"Adding Value: {value}, Min: {stack.Minimum}, Max: {stack.Maximum}");
			}

			Console.WriteLine();

			while (stack.Count > 0)
			{
				Console.WriteLine($"Dequeue Value: {stack.Pop()}, Min: {stack.Minimum}, Max: {stack.Maximum}");
			}
		}

		private static void TestSinglyLinkedList()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			SinglyLinkedList<int> list = new SinglyLinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing SingleLinkedList...");
				CompilationHint();
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
				int x = IListExtension.PickRandom(values);
				SinglyLinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				int value = IListExtension.PickRandom(values);
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
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
			}
			while (more);

			clock.Stop();
		}

		private static void TestLinkedList()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			LinkedList<int> list = new LinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing LinkedList...");
				CompilationHint();
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
				int x = IListExtension.PickRandom(values);
				LinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine("Didn't find a shit...!".BrightRed());
					return;
				}

				int value = IListExtension.PickRandom(values);
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
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
			}
			while (more);

			clock.Stop();
		}

		private static void TestDeque()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			Deque<int> deque = new Deque<int>();

			do
			{
				Console.Clear();
				Title("Testing Deque...");
				CompilationHint();
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
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
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
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			LinkedDeque<int> deque = new LinkedDeque<int>();

			do
			{
				Console.Clear();
				Title("Testing Deque...");
				CompilationHint();
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
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
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
				int value = IListExtension.PickRandom(values);
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
				value = IListExtension.PickRandom(values);
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
				int value = IListExtension.PickRandom(values);
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

		private static void TestBinaryTreeFindClosest()
		{
			bool more;
			Console.Clear();
			Title("Testing BinaryTree FindClosest...");

			BinarySearchTree<int> tree = new BinarySearchTree<int>();
			int len = RNGRandomHelper.Next(1, 50);
			int[] values = GetRandomIntegers(true, len);
			int min = values.Min();
			int max = values.Max();
			tree.Clear();
			tree.Add(values);
			tree.Print();

			do
			{
				int value = RNGRandomHelper.Next(min, max);
				Console.WriteLine($"Closest value to {value.ToString().Yellow()} => {tree.FindClosestValue(value, -1)} ");

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestBinaryTreeBranchSums()
		{
			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Title("Testing BinaryTree BranchSums...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				tree.Clear();
				tree.Add(values);
				tree.Print();
				Console.WriteLine("Branch Sums: ".BrightBlack() + string.Join(", ", tree.BranchSums()));

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestBinaryTreeInvert()
		{
			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Title("Testing BinaryTree Invert...");
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				tree.Clear();
				tree.Add(values);
				tree.Print();
				Console.WriteLine("Inverted: ".BrightYellow());

				tree.Invert();
				tree.Print();

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
				int value = IListExtension.PickRandom(values);
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
				value = IListExtension.PickRandom(values);
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

				int value = IListExtension.PickRandom(values);
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
				value = IListExtension.PickRandom(values);
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
				int value = IListExtension.PickRandom(array);
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
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();
			int[] values = GetRandomIntegers(true, 30);

			do
			{
				Console.Clear();
				Title("Testing all BinaryTrees performance...");

				DoTheTest(binarySearchTree, values);
				ConsoleHelper.Pause();

				DoTheTest(avlTree, values);
				ConsoleHelper.Pause();

				DoTheTest(redBlackTree, values);
				ConsoleHelper.Pause();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] values)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {tree.GetType().Name}...".BrightGreen());
				tree.Clear();
				Debug.Assert(tree.Count == 0, "Values are not cleared correctly!");

				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				Console.WriteLine();
				Console.WriteLine($"Array: {string.Join(", ", values)}");
				
				tree.Add(values);
				Console.WriteLine($"Added {tree.Count} of {values.Length} items.");
				tree.PrintProps();

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				int missed = 0;

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
				Console.WriteLine($"Found {found} of {values.Length} items.");

				tree.Print();

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				missed = 0;

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
				Console.WriteLine($"Removed {removed} of {values.Length} items.");
			}
		}

		private static void TestAllBinaryTreesPerformance()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();
			int[] values = GetRandomIntegers(true, START);

			do
			{
				Console.Clear();
				Title("Testing all BinaryTrees performance...");
				CompilationHint();
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
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
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
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			// this is a RedBlackTree implementation by Microsoft, just testing it.
			SortedSet<int> sortedSet = new SortedSet<int>();
			int[] values = GetRandomIntegers(true, START);

			do
			{
				Console.Clear();
				Title("Testing SortedSet performance...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");

				DoTheTest(sortedSet, values, clock);
				clock.Stop();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				tests++;
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

		private static void TestTreeEquality()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing tree equality...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(true, len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine();
				Console.WriteLine("Testing BinarySearchTree: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine();
				LinkedBinaryTree<int> tree1 = new BinarySearchTree<int>();
				LinkedBinaryTree<int> tree2 = new BinarySearchTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine("Testing BinarySearchTree and AVLTree: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine();
				tree1.Clear();
				tree2 = new AVLTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine("Testing AVLTree: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine();
				tree1 = new AVLTree<int>();
				tree2 = new AVLTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine("Testing RedBlackTree: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine();
				RedBlackTree<int> rbTree1 = new RedBlackTree<int>();
				RedBlackTree<int> rbTree2 = new RedBlackTree<int>();
				DoTheTest(rbTree1, rbTree2, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree1, LinkedBinaryTree<TNode, int> tree2, int[] array)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {tree1.GetType().Name} and {tree1.GetType().Name}...".BrightGreen());
				tree1.Add(array);
				tree2.Add(array);

				Console.WriteLine("InOrder1: ".BrightBlack() + string.Join(", ", tree1));
				Console.WriteLine("InOrder2: ".BrightBlack() + string.Join(", ", tree2));
				tree1.PrintWithProps();
				tree2.PrintWithProps();
				Console.WriteLine($"tree1 == tree2? {tree1.Equals(tree2).ToYesNo()}");
			}
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

		private static void TestSkipList()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			SkipList<int> skipList = new SkipList<int>();
			int[] values = GetRandomIntegers(true, 200_000);

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
		
		private static void TestDisjointSet()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			DisjointSet<int> disjointSet = new DisjointSet<int>();
			int[] values = GetRandomIntegers(true, 12/*200_000*/);

			do
			{
				Console.Clear();
				Title("Testing DisjointSet...");
				Console.WriteLine($"Array has {values.Length} items.");
				disjointSet.Clear();

				clock.Restart();

				foreach (int v in values)
					disjointSet.Add(v);

				Console.WriteLine($"Added {disjointSet.Count} items of {values.Length} in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test search...".BrightYellow());
				int found = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (disjointSet.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine($"Find missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Found {found} of {disjointSet.Count} items in {clock.ElapsedMilliseconds} ms.");

				
				Console.WriteLine("Test find and union...".BrightYellow());

				int threshold = (int)Math.Floor(disjointSet.Count / 0.5d);

				for (int i = 0; i < threshold; i++)
				{
					int x = IListExtension.PickRandom(values), y = IListExtension.PickRandom(values);
					Console.WriteLine($"Find {x} and {y} subsets gives {disjointSet.Find(x)} and {disjointSet.Find(y)} respectively.");
					bool connected = disjointSet.IsConnected(x, y);
					Console.WriteLine($"Are they connected? {connected.ToYesNo()}");

					if (!connected)
					{
						Console.WriteLine("Will union them.");
						disjointSet.Union(x, y);
						Console.WriteLine($"Now, are they connected? {disjointSet.IsConnected(x, y).ToYesNo()}");
					}

					Console.WriteLine();
				}

				Console.WriteLine();
				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (disjointSet.Remove(v))
					{
						removed++;
						continue;
					}
					
					Console.WriteLine($"Remove missed a value: {v} :((".BrightRed());
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.WriteLine("Test to clear the list...");
				disjointSet.Clear();

				if (disjointSet.Count != 0)
				{
					Console.WriteLine($"Something went wrong, the count is {disjointSet.Count}...!".BrightRed());
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

		private static void TestBinaryHeapAdd()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinaryHeap.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheTest(heap, values);

				heap = new MinBinaryHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				BinaryHeap<double, Student> studentsHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentsHeap, students);

				studentsHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentsHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestBinaryHeapRemove()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinaryHeap.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheTest(heap, values);

				heap = new MinBinaryHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				BinaryHeap<double, Student> studentsHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentsHeap, students);

				studentsHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentsHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(heap.ExtractValue());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestBinaryHeapElementAt()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinaryHeap ElementAt...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinBinaryHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinaryHeap<double, Student> studentHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestBinaryHeapDecreaseKey()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinaryHeap DecreaseKey...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinBinaryHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinaryHeap<double, Student> studentHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue, e => e.Grade);

				studentHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue, e => e.Grade);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue, Func<TValue, TKey> getKey)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, key);
					Console.WriteLine($"Extracted {extracted}, expected {key}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {key}.");
				}

				Console.WriteLine();
			}

			static void DoTheValueTest<TNode, TValue>(BinaryHeap<TNode, TValue, TValue> heap, TValue[] array, TValue newKeyValue)
				where TNode : KeyedBinaryNode<TNode, TValue, TValue>
			{
				Queue<TValue> queue = new Queue<TValue>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
					Console.WriteLine($"Extracted {extracted}, expected {newKeyValue}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {node.Value}.");
				}

				Console.WriteLine();
			}

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array, Queue<TKey> queue)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				const int MAX = 10;

				int max = Math.Min(MAX, array.Length);
				queue.Clear();
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestBinomialHeapAdd()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinomialHeap.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheTest(heap, values);

				heap = new MinBinomialHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestBinomialHeapRemove()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinomialHeap.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheTest(heap, values);

				heap = new MinBinomialHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(heap.ExtractValue());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestBinomialHeapElementAt()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinomialHeap ElementAt...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinBinomialHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestBinomialHeapDecreaseKey()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing BinomialHeap DecreaseKey...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinBinomialHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue, e => e.Grade);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue, e => e.Grade);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue, Func<TValue, TKey> getKey)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, key);
					Console.WriteLine($"Extracted {extracted}, expected {key}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {key}.");
				}

				Console.WriteLine();
			}

			static void DoTheValueTest<TNode, TValue>(BinomialHeap<TNode, TValue, TValue> heap, TValue[] array, TValue newKeyValue)
				where TNode : BinomialNode<TNode, TValue, TValue>
			{
				Queue<TValue> queue = new Queue<TValue>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
					Console.WriteLine($"Extracted {extracted}, expected {newKeyValue}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {node.Value}.");
				}

				Console.WriteLine();
			}

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array, Queue<TKey> queue)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				const int MAX = 10;

				int max = Math.Min(MAX, array.Length);
				queue.Clear();
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestPairingHeapAdd()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing PairingHeap.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheTest(heap, values);

				heap = new MinPairingHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestPairingHeapRemove()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing PairingHeap.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheTest(heap, values);

				heap = new MinPairingHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(heap.ExtractValue());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestPairingHeapElementAt()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing PairingHeap ElementAt...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinPairingHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestPairingHeapDecreaseKey()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing PairingHeap DecreaseKey...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinPairingHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue, e => e.Grade);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue, e => e.Grade);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue, Func<TValue, TKey> getKey)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, key);
					Console.WriteLine($"Extracted {extracted}, expected {key}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {key}.");
				}

				Console.WriteLine();
			}

			static void DoTheValueTest<TNode, TValue>(PairingHeap<TNode, TValue, TValue> heap, TValue[] array, TValue newKeyValue)
				where TNode : PairingNode<TNode, TValue, TValue>
			{
				Queue<TValue> queue = new Queue<TValue>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
					Console.WriteLine($"Extracted {extracted}, expected {newKeyValue}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {node.Value}.");
				}

				Console.WriteLine();
			}

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array, Queue<TKey> queue)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				const int MAX = 10;

				int max = Math.Min(MAX, array.Length);
				queue.Clear();
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestFibonacciHeapAdd()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing FibonacciHeap.Add()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheTest(heap, values);

				heap = new MinFibonacciHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		private static void TestFibonacciHeapRemove()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing FibonacciHeap.Remove()...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheTest(heap, values);

				heap = new MinFibonacciHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Count > 0)
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(heap.ExtractValue());
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestFibonacciHeapElementAt()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing FibonacciHeap ElementAt...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinFibonacciHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestFibonacciHeapDecreaseKey()
		{
			bool more;

			do
			{
				Console.Clear();
				Title("Testing FibonacciHeap DecreaseKey...");

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
				Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinFibonacciHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue, e => e.Grade);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue, e => e.Grade);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue, Func<TValue, TKey> getKey)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, key);
					Console.WriteLine($"Extracted {extracted}, expected {key}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {key}.");
				}

				Console.WriteLine();
			}

			static void DoTheValueTest<TNode, TValue>(FibonacciHeap<TNode, TValue, TValue> heap, TValue[] array, TValue newKeyValue)
				where TNode : FibonacciNode<TNode, TValue, TValue>
			{
				Queue<TValue> queue = new Queue<TValue>();
				DoTheTest(heap, array, queue);

				bool succeeded = true;

				while (succeeded && queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
					Console.WriteLine($"Extracted {extracted}, expected {newKeyValue}");
					Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {node.Value}.");
				}

				Console.WriteLine();
			}

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array, Queue<TKey> queue)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				const int MAX = 10;

				int max = Math.Min(MAX, array.Length);
				queue.Clear();
				Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
			}
		}

		#region Not working
		/*
		 * something is off about this class!
		 * I'm 100% sure there must be a bug in there because it can't be right to refer
		 * to _pq[1] instead of _pq[0] while it uses freely the zero based offset!
		 * I'm not sure if the original code works but the idea is cool. It might perform
		 * better but it'll take time to adjust it. Maybe later.
		 */
		//private static void TestIndexMinAdd()
		//{
		//	bool more;

		//	do
		//	{
		//		Console.Clear();
		//		Title("Testing IndexMin.Add()...");

		//		int len = RNGRandomHelper.Next(1, 12);
		//		int[] values = GetRandomIntegers(len);
		//		Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

		//		IndexMin<int> heap = new MaxIndexMin<int>();
		//		DoTheTest(heap, values);

		//		heap = new MinIndexMin<int>();
		//		DoTheTest(heap, values);

		//		Student[] students = GetRandomStudents(len);
		//		IndexMin<double, Student> studentsHeap = new MaxIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentsHeap, students);

		//		studentsHeap = new MinIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentsHeap, students);

		//		Console.WriteLine();
		//		Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
		//		ConsoleKeyInfo response = Console.ReadKey(true);
		//		Console.WriteLine();
		//		more = response.Key == ConsoleKey.Y;
		//	}
		//	while (more);

		//	static void DoTheTest<TNode, TKey, TValue>(IndexMin<TNode, TKey, TValue> heap, TValue[] array)
		//		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		//	{
		//		Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

		//		foreach (TValue value in array)
		//		{
		//			heap.Add(value);
		//			//heap.PrintWithProps();
		//		}

		//		Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
		//	}
		//}

		//private static void TestIndexMinRemove()
		//{
		//	bool more;

		//	do
		//	{
		//		Console.Clear();
		//		Title("Testing IndexMin.Remove()...");

		//		int len = RNGRandomHelper.Next(1, 12);
		//		int[] values = GetRandomIntegers(len);
		//		Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

		//		IndexMin<int> heap = new MaxIndexMin<int>();
		//		DoTheTest(heap, values);

		//		heap = new MinIndexMin<int>();
		//		DoTheTest(heap, values);

		//		Student[] students = GetRandomStudents(len);
		//		IndexMin<double, Student> studentsHeap = new MaxIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentsHeap, students);

		//		studentsHeap = new MinIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentsHeap, students);

		//		Console.WriteLine();
		//		Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
		//		ConsoleKeyInfo response = Console.ReadKey(true);
		//		Console.WriteLine();
		//		more = response.Key == ConsoleKey.Y;
		//	}
		//	while (more);

		//	static void DoTheTest<TNode, TKey, TValue>(IndexMin<TNode, TKey, TValue> heap, TValue[] array)
		//		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		//	{
		//		Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
		//		heap.Add(array);
		//		Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
		//		Console.WriteLine("Test removing...");
		//		bool removeStarted = false;

		//		while (heap.Count > 0)
		//		{
		//			if (!removeStarted) removeStarted = true;
		//			else Console.Write(", ");

		//			Console.Write(heap.ExtractValue());
		//		}

		//		Console.WriteLine();
		//		Console.WriteLine();
		//	}
		//}

		//private static void TestIndexMinElementAt()
		//{
		//	bool more;

		//	do
		//	{
		//		Console.Clear();
		//		Title("Testing IndexMin ElementAt...");

		//		int len = RNGRandomHelper.Next(1, 12);
		//		int[] values = GetRandomIntegers(len);
		//		int k = RNGRandomHelper.Next(1, values.Length);
		//		Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

		//		IndexMin<int> heap = new MaxIndexMin<int>();
		//		DoTheTest(heap, values, k);

		//		heap = new MinIndexMin<int>();
		//		DoTheTest(heap, values, k);

		//		Student[] students = GetRandomStudents(len);
		//		Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
		//		Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

		//		IndexMin<double, Student> studentHeap = new MaxIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentHeap, students, k);

		//		studentHeap = new MinIndexMin<double, Student>(e => e.Grade);
		//		DoTheTest(studentHeap, students, k);

		//		Console.WriteLine();
		//		Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
		//		ConsoleKeyInfo response = Console.ReadKey(true);
		//		Console.WriteLine();
		//		more = response.Key == ConsoleKey.Y;
		//	}
		//	while (more);

		//	static void DoTheTest<TNode, TKey, TValue>(IndexMin<TNode, TKey, TValue> heap, TValue[] array, int k)
		//		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		//	{
		//		Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());
		//		heap.Add(array);
		//		Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
		//		Console.WriteLine($"Kth element at position {k} = {heap.ElementAt(k).ToString().BrightCyan().Underline()}");
		//		Console.WriteLine();
		//		Console.WriteLine();
		//	}
		//}

		//private static void TestIndexMinDecreaseKey()
		//{
		//	bool more;

		//	do
		//	{
		//		Console.Clear();
		//		Title("Testing IndexMin DecreaseKey...");

		//		int len = RNGRandomHelper.Next(1, 12);
		//		int[] values = GetRandomIntegers(len);
		//		Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));
		//		Console.WriteLine("Array [sorted]: ".Yellow() + string.Join(", ", values.OrderBy(e => e)));

		//		IndexMin<int> heap = new MaxIndexMin<int>();
		//		DoTheValueTest(heap, values, int.MaxValue);

		//		heap = new MinIndexMin<int>();
		//		DoTheValueTest(heap, values, int.MinValue);

		//		Student[] students = GetRandomStudents(len);
		//		Console.WriteLine("Students: ".BrightBlack() + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
		//		Console.WriteLine("Students [sorted]: ".Yellow() + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

		//		IndexMin<double, Student> studentHeap = new MaxIndexMin<double, Student>(e => e.Grade);
		//		DoTheKeyTest(studentHeap, students, int.MaxValue, e => e.Grade);

		//		studentHeap = new MinIndexMin<double, Student>(e => e.Grade);
		//		DoTheKeyTest(studentHeap, students, int.MinValue, e => e.Grade);

		//		Console.WriteLine();
		//		Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
		//		ConsoleKeyInfo response = Console.ReadKey(true);
		//		Console.WriteLine();
		//		more = response.Key == ConsoleKey.Y;
		//	}
		//	while (more);

		//	static void DoTheKeyTest<TNode, TKey, TValue>(IndexMin<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue, Func<TValue, TKey> getKey)
		//		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		//	{
		//		Queue<TKey> queue = new Queue<TKey>();
		//		DoTheTest(heap, array, queue);

		//		bool succeeded = true;

		//		while (succeeded && queue.Count > 0)
		//		{
		//			TKey key = queue.Dequeue();
		//			TNode node = heap.FindByKey(key);
		//			Debug.Assert(node != null, $"Node for key {key} is not found.");
		//			heap.DecreaseKey(node, newKeyValue);
		//			TKey extracted = heap.ExtractValue().Key;
		//			succeeded = heap.Comparer.IsEqual(extracted, key);
		//			Console.WriteLine($"Extracted {extracted}, expected {key}");
		//			Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {key}.");
		//		}

		//		Console.WriteLine();
		//	}

		//	static void DoTheValueTest<TNode, TValue>(IndexMin<TNode, TValue, TValue> heap, TValue[] array, TValue newKeyValue)
		//		where TNode : KeyedBinaryNode<TNode, TValue, TValue>
		//	{
		//		Queue<TValue> queue = new Queue<TValue>();
		//		DoTheTest(heap, array, queue);

		//		bool succeeded = true;

		//		while (succeeded && queue.Count > 0)
		//		{
		//			TValue key = queue.Dequeue();
		//			TNode node = heap.Find(key);
		//			Debug.Assert(node != null, $"Node for value {key} is not found.");
		//			heap.DecreaseKey(node, newKeyValue);
		//			TValue extracted = heap.ExtractValue().Key;
		//			succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
		//			Console.WriteLine($"Extracted {extracted}, expected {newKeyValue}");
		//			Debug.Assert(succeeded, $"Extracted a different value {extracted} instead of {node.Value}.");
		//		}

		//		Console.WriteLine();
		//	}

		//	static void DoTheTest<TNode, TKey, TValue>(IndexMin<TNode, TKey, TValue> heap, TValue[] array, Queue<TKey> queue)
		//		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		//	{
		//		const int MAX = 10;

		//		int max = Math.Min(MAX, array.Length);
		//		queue.Clear();
		//		Console.WriteLine($"Test adding ({heap.GetType().Name})...".BrightGreen());

		//		foreach (TValue v in array)
		//		{
		//			TNode node = heap.MakeNode(v);
		//			if (queue.Count < max) queue.Enqueue(node.Key);
		//			heap.Add(node);
		//		}

		//		Console.WriteLine("Enumeration: ".BrightBlack() + string.Join(", ", heap));
		//	}
		//}
		#endregion

		private static void TestAllHeapsPerformance()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int tests = 0;
			int[] values = GetRandomIntegers(true, START);
			Student[] students = GetRandomStudents(START);
			Func<Student, double> getKey = e => e.Grade;

			do
			{
				Console.Clear();
				Title("Testing All Heap types performance...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");
				Title("Testing IHeap<int> types performance...");

				// BinaryHeap
				DoHeapTest(new MinBinaryHeap<int>(), values, clock);
				clock.Stop();

				// BinomialHeap
				DoHeapTest(new MinBinomialHeap<int>(), values, clock);
				clock.Stop();

				// PairingHeap
				DoHeapTest(new MinPairingHeap<int>(), values, clock);
				clock.Stop();

				// FibonacciHeap
				DoHeapTest(new MinFibonacciHeap<int>(), values, clock);
				clock.Stop();

				Title("Testing IKeyedHeap<TNode, TKey, TValue> types performance...");
				
				// BinaryHeap
				DoHeapTest(new MinBinaryHeap<double, Student>(getKey), students, clock);
				clock.Stop();

				// BinomialHeap
				DoHeapTest(new MinBinomialHeap<double, Student>(getKey), students, clock);
				clock.Stop();

				// PairingHeap
				DoHeapTest(new MinPairingHeap<double, Student>(getKey), students, clock);
				clock.Stop();

				// FibonacciHeap
				DoHeapTest(new MinFibonacciHeap<double, Student>(getKey), students, clock);
				clock.Stop();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 0) continue;
				values = GetRandomIntegers(true, HEAVY);
				students = GetRandomStudents(HEAVY);
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoHeapTest<T>(IHeap<T> heap, T[] values, Stopwatch clock)
			{
				Console.WriteLine();
				Console.WriteLine($"Testing {heap.GetType().Name}...".BrightGreen());
				heap.Clear();
				Console.WriteLine($"Original values: {values.Length.ToString().BrightYellow()}...");
				Debug.Assert(heap.Count == 0, "Values are not cleared correctly!");

				clock.Restart();
				heap.Add(values);
				Console.WriteLine($"Added {heap.Count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine("Test removing...".BrightRed());
				int removed = 0;
				clock.Restart();

				while (heap.Count > 0)
				{
					heap.ExtractValue();
					removed++;
				}
				Console.WriteLine($"Removed {removed} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
				Debug.Assert(removed == values.Length, "Not all values are removed correctly!");
			}
		}

		private static void TestGraph()
		{
			const int MAX_LIST = 26;

			bool more;
			GraphList<char> graph;
			WeightedGraphList<char, int> weightedGraph;
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
					weightedGraph = new WeightedUndirectedGraphList<char, int>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(weightedGraph, values);
				})
				.Add("Weighted directed graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedDirectedGraphList<char, int>();
					if (values.Count == 0) AddValues(values);
					DoTheTest(weightedGraph, values);
				})
				.Add("Weighted mixed graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedMixedGraphList<char, int>();
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

			static bool DoTheTest<TAdjacencyList, TEdge>(GraphList<char, TAdjacencyList, TEdge> graph, List<char> values)
				where TAdjacencyList : class, ICollection<TEdge>
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
				Console.Write($@"{"Would you like to add a bit of randomization?".Yellow()} {"[Y]".BrightGreen()} / {"any key".Dim()}.
This may cause cycles but will make it much more fun for finding shortest paths. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				int threshold = response.Key != ConsoleKey.Y ? 0 : (int)Math.Floor(values.Count * 0.5d);

				Console.Write($"{"Can the edges have negative weights?".Yellow()} {"[Y]".BrightGreen()} / {"any key".Dim()}.");
				response = Console.ReadKey(true);
				Console.WriteLine();
				int min = response.Key == ConsoleKey.Y
							? (int)sbyte.MinValue
							: byte.MinValue, max = sbyte.MaxValue;

				Queue<char> queue = new Queue<char>(values);
				char from = queue.Dequeue();
				Action<char, char> addEdge = graph switch
				{
					MixedGraphList<char> mGraph => (f, t) => mGraph.AddEdge(f, t, __fakeGenerator.Value.Random.Bool()),
					WeightedMixedGraphList<char, int> wmGraph => (f, t) => wmGraph.AddEdge(f, t, RNGRandomHelper.Next(min, max), __fakeGenerator.Value.Random.Bool()),
					WeightedGraphList<char, int> wGraph => (f, t) => wGraph.AddEdge(f, t, RNGRandomHelper.Next(min, max)),
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
						queue.Enqueue(IListExtension.PickRandom(values));
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

					if (!char.IsLetter(response.KeyChar) || !graph.ContainsKey(response.KeyChar))
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

			static bool DoTheTestWithValue<TAdjacencyList, TEdge>(GraphList<char, TAdjacencyList, TEdge> graph, List<char> values, char value)
				where TAdjacencyList : class, ICollection<TEdge>
			{
				const string LINE_SEPARATOR = "*******************************************************************************";

				Console.WriteLine("Breadth First: ".Yellow() + string.Join(", ", graph.Enumerate(value, BreadthDepthTraverse.BreadthFirst)));
				Console.WriteLine("Depth First: ".Yellow() + string.Join(", ", graph.Enumerate(value, BreadthDepthTraverse.DepthFirst)));
				Console.WriteLine("Degree: ".Yellow() + graph.Degree(value));

				// detect a cycle
				IEnumerable<char> cycle = graph.FindCycle();
				if (cycle != null) Console.WriteLine("Found cycle: ".BrightRed() + string.Join(", ", cycle));

				// test specific graph type features
				switch (graph)
				{
					case DirectedGraphList<char> directedGraph:
						Console.WriteLine("InDegree: ".Yellow() + directedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", directedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case MixedGraphList<char> mixedGraph:
						Console.WriteLine("InDegree: ".Yellow() + mixedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", mixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case WeightedDirectedGraphList<char, int> weightedDirectedGraph:
						Console.WriteLine("InDegree: ".Yellow() + weightedDirectedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", weightedDirectedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
					case WeightedUndirectedGraphList<char, int> weightedUndirectedGraph:
						Console.WriteLine(LINE_SEPARATOR);
						WeightedUndirectedGraphList<char, int> spanningTree = weightedUndirectedGraph.GetMinimumSpanningTree(SpanningTreeAlgorithm.Prim);

						if (spanningTree != null)
						{
							Console.WriteLine("Prim Spanning Tree: ".Yellow());
							spanningTree.Print();
							Console.WriteLine(LINE_SEPARATOR);
						}
						
						spanningTree = weightedUndirectedGraph.GetMinimumSpanningTree(SpanningTreeAlgorithm.Kruskal);

						if (spanningTree != null)
						{
							Console.WriteLine("Kruskal Spanning Tree: ".Yellow());
							spanningTree.Print();
							Console.WriteLine(LINE_SEPARATOR);
						}
						break;
					case WeightedMixedGraphList<char, int> weightedMixedGraph:
						Console.WriteLine("InDegree: ".Yellow() + weightedMixedGraph.InDegree(value));
						try { Console.WriteLine("Topological Sort: ".Yellow() + string.Join(", ", weightedMixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine("Topological Sort: ".Yellow() + e.Message.BrightRed()); }
						break;
				}

				if (graph is WeightedGraphList<char, int> wGraph)
				{
					char to = IListExtension.PickRandom(values);
					ConsoleKeyInfo response;

					do
					{
						Console.Write($@"Current position is '{value.ToString().BrightGreen()}'. Type in {"a character".BrightGreen()} to find the shortest path to,
(You can press the {"RETURN".BrightGreen()} key to accept the current random value '{to.ToString().BrightGreen()}'),
or press {"ESCAPE".BrightRed()} key to exit this test. ");
						response = Console.ReadKey();
						Console.WriteLine();
						if (response.Key == ConsoleKey.Escape) return false;
						if (response.Key == ConsoleKey.Enter) continue;
						if (!char.IsLetter(response.KeyChar) || !wGraph.ContainsKey(response.KeyChar)) Console.WriteLine($"Character '{value}' is not found or not connected!");
						to = response.KeyChar;
						break;
					}
					while (response.Key != ConsoleKey.Enter);

					Console.WriteLine();
					Console.WriteLine($"{"Shortest Path".Yellow()} from '{value.ToString().BrightCyan()}' to '{to.ToString().BrightCyan()}'");
					
					Console.Write("Dijkstra: ");
					try { Console.WriteLine(string.Join(" -> ", wGraph.SingleSourcePath(value, to, SingleSourcePathAlgorithm.Dijkstra))); }
					catch (Exception e) { Console.WriteLine(e.Message.BrightRed()); }
					
					Console.Write("Bellman-Ford: ");
					try { Console.WriteLine(string.Join(" -> ", wGraph.SingleSourcePath(value, to, SingleSourcePathAlgorithm.BellmanFord))); }
					catch (Exception e) { Console.WriteLine(e.Message.BrightRed()); }
				}

				return true;
			}
		}

		private static void Title(string title)
		{
			Console.WriteLine();
			Console.WriteLine(title.Bold().BrightBlack());
			Console.WriteLine();
		}

		private static void CompilationHint()
		{
			Console.WriteLine(COMPILATION_TEXT);
			Console.WriteLine();
		}

		[NotNull]
		private static Action<IList<T>, int, int, IComparer<T>, bool> GetAlgorithm<T>([NotNull] string name)
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

		private static bool RandomBool() { return RandomHelper.Default.Next(1) == 1; }

		[NotNull]
		private static int[] GetRandomIntegers(int len = 0) { return GetRandomIntegers(false, len); }
		
		[NotNull]
		private static int[] GetRandomIntegers(bool unique, int len = 0)
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

		private class Student : IComparable<Student>, IComparable, IEquatable<Student>
		{
			public string Name { get; set; }

			public double Grade { get; set; }

			/// <inheritdoc />
			public override string ToString() { return $"{Name} [{Grade:F2}]"; }

			/// <inheritdoc />
			public int CompareTo(Student other)
			{
				if (ReferenceEquals(this, other)) return 0;
				if (ReferenceEquals(other, null)) return -1;
				int cmp = Grade.CompareTo(other.Grade);
				if (cmp != 0) return cmp;
				return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
			}

			/// <inheritdoc />
			int IComparable.CompareTo(object obj) { return CompareTo(obj as Student); }

			/// <inheritdoc />
			public bool Equals(Student other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && Grade.Equals(other.Grade);
			}
		}
	}
}

public static class Extension
{
	[NotNull]
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
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
		Console.WriteLine($"{"LeftMost:".Yellow()} {thisValue.LeftMost()} {"RightMost:".Yellow()} {thisValue.RightMost()}");
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

	public static void PrintProps<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue)
		where TAdjacencyList : class, ICollection<TEdge>
	{
		Console.WriteLine();
		Console.WriteLine($"{"Order:".Yellow()} {thisValue.Count.ToString().Underline()}.");
		Console.WriteLine($"{"Size:".Yellow()} {thisValue.GetSize().ToString().Underline()}.");
	}

	public static void PrintWithProps<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue)
		where TAdjacencyList : class, ICollection<TEdge>
	{
		PrintProps(thisValue);
		thisValue.Print();
	}

	public static void Print<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue)
		where TAdjacencyList : class, ICollection<TEdge>
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void Print<TNode, TKey, TValue>([NotNull] this BinaryHeap<TNode, TKey, TValue> thisValue)
		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void Print<TNode, TKey, TValue>([NotNull] this BinomialHeap<TNode, TKey, TValue> thisValue)
		where TNode : BinomialNode<TNode, TKey, TValue>
	{
		Console.WriteLine();
		Console.WriteLine("Count: " + thisValue.Count);
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void Print<TNode, TKey, TValue>([NotNull] this FibonacciHeap<TNode, TKey, TValue> thisValue)
		where TNode : FibonacciNode<TNode, TKey, TValue>
	{
		Console.WriteLine();
		Console.WriteLine("Count: " + thisValue.Count);
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}

	public static void Print<TNode, TKey, TValue>([NotNull] this PairingHeap<TNode, TKey, TValue> thisValue)
		where TNode : PairingNode<TNode, TKey, TValue>
	{
		Console.WriteLine();
		Console.WriteLine("Count: " + thisValue.Count);
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}
}
