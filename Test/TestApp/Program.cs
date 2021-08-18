using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Bogus;
using Bogus.DataSets;
using JetBrains.Annotations;
using System.ServiceProcess;
using System.Threading.Tasks;
using essentialMix;
using essentialMix.Newtonsoft.Helpers;
using essentialMix.Patterns.Threading;
using essentialMix.Threading;
using essentialMix.Collections;
using essentialMix.Comparers;
using essentialMix.Cryptography;
using essentialMix.Cryptography.Settings;
using essentialMix.Exceptions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Events;
using Other.Microsoft.Collections;
using essentialMix.Threading.Helpers;
using essentialMix.Threading.Patterns.ProducerConsumer;
using Newtonsoft.Json;
using Other.JonSkeet.MiscUtil.Collections;

using TimeoutException = System.TimeoutException;
using Menu = EasyConsole.Menu;
using AutoResetEvent = essentialMix.Threading.AutoResetEvent;
using ManualResetEvent = essentialMix.Threading.ManualResetEvent;

using static Crayon.Output;

// ReSharper disable UnusedMember.Local
namespace TestApp
{
	internal class Program
	{
		private const int START = 10;
		private const int SMALL = 10_000;
		private const int MEDIUM = 100_000;
		private const int HEAVY = 1_000_000;
		
		private const int MAX_ITERATION_INC = 3;
		private const int TOP_COUNT = 10;
		
		private const int PAUSE_TIMEOUT = 3000;

		private static readonly string __compilationText = Yellow($@"
This is C# (a compiled language), so the test needs to run at least
once before considering results in order for the code to be compiled
and run at full speed. The first time this test run, it will start 
with just {START} items and the next time when you press '{Bright.Green("Y")}', it will 
work with {HEAVY} items.");

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

			//TestExpressionExtension();

			//TestFibonacci();
			//TestGroupAnagrams();
			//TestKadaneMaximum();
			//TestLevenshteinDistance();
			//TestDeepestPit();

			//TestThreadQueue();
			//TestThreadQueueWithPriorityQueue();
			//TestAsyncLock();

			//TestSortAlgorithm();
			//TestSortAlgorithms();

			//TestLinkedQueue();
			//TestMinMaxQueue();
			//TestSinglyLinkedList();
			//TestLinkedList();
			//TestDeque();
			//TestLinkedDeque();
			//TestCircularBuffer();
			//TestLinkedCircularBuffer();
			//TestBitCollection();
			//TestQueueAdapter();

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

			//TestAllHeapsPerformance();

			//TestGraph();

			//TestAsymmetric();

			//TestSingletonAppGuard();

			//TestImpersonationHelper();
			
			//TestServiceHelper();
			
			//TestUriHelper();
			//TestUriHelperRelativeUrl();
			
			//TestJsonUriConverter();

			//TestDevicesMonitor();

			//TestAppInfo();

			//TestObservableCollections();

			//TestEnumerateDirectoriesAndFiles();
			
			//TestEventWaitHandle();
			
			TestWaitForEvent();

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
					Console.WriteLine(Bright.Black("Testing:"));
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
				Console.WriteLine(Bright.Red("No matching entries..!"));
				return;
			}

			Console.WriteLine($"Found {Bright.Green(matchingDomains.Count.ToString())} entries:");

			foreach ((string, string) tuple in matchingDomains)
			{
				Console.WriteLine(tuple.Item1);
				Console.WriteLine(tuple.Item2);
				Console.WriteLine();
			}
		}

		private static void TestExpressionExtension()
		{
			Expression[] expressions =
			{
				(Expression<Action<Student>>)(s => s.MethodWithInputs(1, "2")),
				(Expression<Action<Student>>)(s => s.MethodWithArrayInputs(new[]{1, 2}, new[]{"3", "4"})),
				(Expression<Action<Student>>)(s => s.MethodWithInputs(1, new[]{2, 3}))
			};

			Title("Testing expression extension...");

			StringBuilder sb = new StringBuilder();

			foreach (Expression expression in expressions)
			{
				object[] values = expression.GetValues();
				sb.Length = 0;
				sb.Append('{');

				foreach (object value in values)
				{
					Add(sb, value);
				}

				sb.Append('}');
				Console.WriteLine($"{expression} => {sb}");
			}

			static void Add(StringBuilder sb, object value)
			{
				if (sb.Length > 1 && sb[sb.Length - 1] != '{') sb.Append(", ");

				if (value is null)
				{
					sb.Append("null");
				}
				else if (value is string s)
				{
					sb.Append($"\"{s}\"");
				}
				else if (value is char c)
				{
					sb.Append($"'{c}'");
				}
				else if (value is IEnumerable enumerable)
				{
					sb.Append('{');

					foreach (object o in enumerable)
						Add(sb, o);

					sb.Append('}');
				}
				else
				{
					sb.Append(value);
				}
			}
		}

		private static void TestFibonacci()
		{
			bool more;
			Console.Clear();
			Console.WriteLine("Testing Fibonacci number: ");

			do
			{
				Console.Write($"Type in {Bright.Green("a number")} to calculate the Fibonacci number for or {Bright.Red("ESCAPE")} key to exit. ");
				string response = Console.ReadLine();
				more = !string.IsNullOrWhiteSpace(response);
				if (more && uint.TryParse(response, out uint value)) Console.WriteLine(essentialMix.Numeric.Math2.Fibonacci(value));
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
				Console.Write(Bright.Black("Words: "));
				
				if (words == null) Console.WriteLine("<null>");
				else if (words.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", words) + "]");
				
				IReadOnlyCollection<IReadOnlyList<string>> anagrams = StringHelper.GroupAnagrams(words);
				Console.Write(Bright.Yellow("Anagrams: "));

				if (anagrams == null) Console.WriteLine("<null>");
				else if (anagrams.Count == 0) Console.WriteLine("[]");
				else Console.WriteLine(string.Join(", ", anagrams.Select(e => "[" + string.Join(", ", e) + "]")));
				
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
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
				Console.Write(Bright.Black("Numbers: "));
				
				if (numbers.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", numbers) + "]");
				
				Console.WriteLine(Bright.Yellow("Sum: ") + numbers.KadaneMaximumSum());
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Yellow("Levenshtein Distance: ") + StringHelper.LevenshteinDistance(first, second));
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
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
				("Test case none: ", Array.Empty<int>()), //-1
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
				Console.Write(Bright.Black(label));
				
				if (numbers.Length == 0) Console.WriteLine("[]");
				else Console.WriteLine("[" + string.Join(", ", numbers) + "]");
				
				Console.WriteLine(Bright.Yellow("Deepest Pit: ") + numbers.DeepestPit());
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestThreadQueue()
		{
			int iteration = 1;
			int[] values = Enumerable.Range(1, START).ToArray();
			Console.WriteLine();
			Console.Write($"Would you like to use timeout for the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to skip timeout. ");
			int timeout = Console.ReadKey(true).Key == ConsoleKey.Y
							? 1
							: 0;
			Console.WriteLine();

			string timeoutString = timeout > 0
										? $"{timeout} minute(s)"
										: "None";
			int threads;

			if (DebugHelper.DebugMode)
			{
				// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
				threads = LimitThreads()
							? 1
							: RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			}
			else
			{
				// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
				threads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			}

			if (threads < 1 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;

			ThreadQueueMode[] modes = EnumHelper<ThreadQueueMode>.GetValues();
			Queue<ThreadQueueMode> queueModes = new Queue<ThreadQueueMode>(modes);
			HashSet<int> visited = new HashSet<int>();
			List<int> duplicates = new List<int>();
			Stopwatch clock = new Stopwatch();

			while (queueModes.Count > 0)
			{
				Console.Clear();
				Console.WriteLine();

				ThreadQueueMode mode = queueModes.Dequeue();
				Title($"Testing multi-thread queue in '{Bright.Cyan(mode.ToString())}' mode...");

				CancellationTokenSource cts = null;
				CountdownEvent cdeThreshold = null;
				IProducerConsumer<int> queue = null;

				try
				{
					int written = 0;
					// if there is a timeout, will use a CancellationTokenSource.
					cts = timeout > 0
							? new CancellationTokenSource(TimeSpan.FromMinutes(timeout))
							: null;
					cdeThreshold = new CountdownEvent(values.Length / 2);
					// copy to local variable
					int[] val = values;
					CancellationToken token = cts?.Token ?? CancellationToken.None;
					CountdownEvent threshold = cdeThreshold;

					ProducerConsumerQueueOptions<int> options = mode switch
					{
						ThreadQueueMode.ThresholdTaskGroup => new ProducerConsumerThresholdQueueOptions<int>(threads, (_, item) => Exec(item, ref written, visited, duplicates, threshold))
						{
							// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
							Threshold = TimeSpan.FromSeconds(1),
							WorkStartedCallback = que => QueueStarted(que, val, mode, clock, timeoutString),
							WorkCompletedCallback = que => QueueCompleted(que, val, visited, duplicates, mode, clock, timeoutString, written)
						},
						_ => new ProducerConsumerQueueOptions<int>(threads, (_, item) => Exec(item, ref written, visited, duplicates, threshold))
						{
							WorkStartedCallback = que => QueueStarted(que, val, mode, clock, timeoutString),
							WorkCompletedCallback = que => QueueCompleted(que, val, visited, duplicates, mode, clock, timeoutString, written)
						}
					};
				
					visited.Clear();
					duplicates.Clear();
					queue = ProducerConsumerQueue.Create(mode, options, token);

					for (int i = 0; i < values.Length; i++)
					{
						if (token.IsCancellationRequested) break;
						queue.Enqueue(values[i]);
					}

					// wait for the threshold to pause is reached to test the pause feature
					cdeThreshold.Wait(token);

					if (!token.IsCancellationRequested)
					{
						if (!queue.CanPause)
						{
							Title($"Queue does {Bright.Cyan("NOT SUPPORT")} pausing.");
						}
						else
						{
							queue.Pause();
							Console.WriteLine();
							Console.WriteLine(Bright.Red($"Queue is paused for {PAUSE_TIMEOUT / 1000} seconds."));
							TimeSpanHelper.WasteTime(PAUSE_TIMEOUT, token);
							Console.WriteLine(Bright.Red("Queue is done pausing."));
							queue.Resume();
						}
					}

					/*
					* when the queue is being disposed, it will wait until the queued items are processed.
					* this works when queue.WaitOnDispose is true, which is false by default.
					* alternatively, the following can be done to wait for all items to be processed:
					*
					* Important: marking the completion of the queue, will prohibit further items from being
					* added and queue will only process items that already have been queued.
					* queue.Complete();
					* // wait for the queue to finish
					* queue.Wait();
					*
					* another way to go about it, is not to call queue.Complete(); if this queue will
					* wait indefinitely and maybe using a CancellationTokenSource.
					*
					* It's preferable to use queue.Wait() rather than to set WaitOnDispose for the option and
					* to let the queue get disposed of.
					*/
					queue.Complete();
					queue.Wait();
				}
				finally
				{
					ObjectHelper.Dispose(ref queue);
					ObjectHelper.Dispose(ref cdeThreshold);
					ObjectHelper.Dispose(ref cts);
				}

				if (queueModes.Count == 0)
				{
					Console.WriteLine();
					Console.Write($"Would you like to repeat the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to exit. ");

					if (Console.ReadKey(true).Key == ConsoleKey.Y)
					{
						Console.WriteLine();

						foreach (ThreadQueueMode m in modes) 
							queueModes.Enqueue(m);

						if (iteration < MAX_ITERATION_INC) values = Enumerable.Range(1, values.Length * ++iteration).ToArray();
					}

					continue;
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				if (response.Key != ConsoleKey.Y) queueModes.Clear();
			}

			clock.Stop();

			static void QueueStarted(IProducerConsumer<int> queue, IReadOnlyCollection<int> values, ThreadQueueMode mode, Stopwatch clock, string timeoutString)
			{
				Console.WriteLine();
				Console.WriteLine($"Starting multi-thread test. mode: '{Bright.Cyan(mode.ToString())}', values: {Bright.Cyan(values.Count.ToString())}, threads: {Bright.Cyan(queue.Threads.ToString())}, timeout: {Bright.Cyan(timeoutString)}...");
				if (mode == ThreadQueueMode.ThresholdTaskGroup) Console.WriteLine($"in {mode} mode, {Bright.Cyan(queue.Threads.ToString())} tasks will be issued every {Bright.Cyan(((ProducerConsumerThresholdQueue<int>)queue).Threshold.TotalSeconds.ToString("N0"))} second(s).");
				Console.WriteLine();
				Console.WriteLine();
				clock.Restart();
			}

			static void QueueCompleted(IProducerConsumer<int> queue, IReadOnlyCollection<int> values, ICollection<int> visited, IList<int> duplicates, ThreadQueueMode mode, Stopwatch clock, string timeoutString, int written)
			{
				long elapsed = clock.ElapsedMilliseconds;
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Finished test. mode: '{Bright.Cyan(mode.ToString())}', threads: {Bright.Cyan(queue.Threads.ToString())}, timeout: {Bright.Cyan(timeoutString)}, elapsed: {Bright.Cyan(elapsed.ToString())} ms.");
				Console.WriteLine($"Values length: {Bright.Cyan(values.Count.ToString())}, written: {Bright.Cyan(written.ToString())}, visited length: {Bright.Cyan(visited.Count.ToString())}, sets length: {Bright.Cyan((visited.Count + duplicates.Count).ToString())}.");

				bool badValues = written != values.Count || visited.Count != values.Count;

				if (badValues)
				{
					Console.WriteLine($"Values: {string.Join(", ", values.OrderBy(e => e))}.");
					Console.WriteLine(Bright.Cyan($"Visited: {string.Join(", ", visited.OrderBy(e => e))}."));
				}

				if (duplicates.Count > 0)
				{
					if (!badValues) Console.WriteLine(Bright.Cyan($"Visited: {string.Join(", ", visited.OrderBy(e => e))}."));
					Console.WriteLine(Bright.Red($"Duplicate entries: {string.Join(", ", duplicates.OrderBy(e => e))}."));
				}
			}

			static void Exec(int e, ref int written, ISet<int> visited, IList<int> duplicates, CountdownEvent threshold)
			{
				Task.Delay(RNGRandomHelper.Next(10, 100)).Execute();
				Console.Write($"{e:D4} ");
				Interlocked.Increment(ref written);

				lock(visited)
				{
					if (!visited.Add(e))
					{
						lock(duplicates) 
							duplicates.Add(e);
					}
				}

				if (threshold.IsSet) return;
				threshold.SignalOne();
			}
		}

		private static void TestThreadQueueWithPriorityQueue()
		{
			int iteration = 1;
			Student[] values = GetRandomStudents(START);
			Console.WriteLine();
			Console.Write($"Would you like to use timeout for the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to skip timeout. ");
			int timeout = Console.ReadKey(true).Key == ConsoleKey.Y
							? 1
							: 0;
			Console.WriteLine();

			string timeoutString = timeout > 0
										? $"{timeout} minute(s)"
										: "None";
			int threads;

			if (DebugHelper.DebugMode)
			{
				// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
				threads = LimitThreads()
							? 1
							: RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			}
			else
			{
				// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
				threads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			}

			if (threads < 1 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;

			ThreadQueueMode[] modes = EnumHelper<ThreadQueueMode>.GetValues()
																.Where(e => e.SupportsProducerQueue())
																.ToArray();
			Queue<ThreadQueueMode> queueModes = new Queue<ThreadQueueMode>(modes);
			HashSet<Student> visited = new HashSet<Student>();
			List<Student> duplicates = new List<Student>();
			BinomialHeap<Student> priorityQueue = null;
			Stopwatch clock = new Stopwatch();
			int externalId = 0;

			while (queueModes.Count > 0)
			{
				Console.Clear();
				Console.WriteLine();

				ThreadQueueMode mode = queueModes.Dequeue();
				Title($"Testing multi-thread queue in '{Bright.Cyan(mode.ToString())}' mode...");

				CancellationTokenSource cts = null;
				CountdownEvent cdeThreshold = null;
				IProducerConsumer<Student> queue = null;

				try
				{
					ConsoleKey heapKey = ConsoleKey.Clear;

					while (heapKey != ConsoleKey.N && heapKey != ConsoleKey.X && heapKey != ConsoleKey.Escape)
					{
						Console.Write($"Would you like to use Mi{Bright.Green("[n]")}Heap or Ma{Bright.Green("[x]")}Heap? Press {Bright.Red("ESCAPE")} key to exit this test. ");
						heapKey = Console.ReadKey().Key;
						Console.WriteLine();
					}

					if (heapKey == ConsoleKey.N)
					{
						if (priorityQueue == null || priorityQueue.GetType() != typeof(MinBinomialHeap<Student>))
							priorityQueue = new MinBinomialHeap<Student>(ComparisonComparer.FromComparison<Student>((x, y) => x.Grade.CompareTo(y.Grade)));
						else priorityQueue.Clear();
					}
					else if (heapKey == ConsoleKey.X)
					{
						if (priorityQueue == null || priorityQueue.GetType() != typeof(MaxBinomialHeap<Student>))
							priorityQueue = new MaxBinomialHeap<Student>(ComparisonComparer.FromComparison<Student>((x, y) => x.Grade.CompareTo(y.Grade)));
						else priorityQueue.Clear();
					}
					else
					{
						break;
					}

					int written = 0;
					// if there is a timeout, will use a CancellationTokenSource.
					cts = timeout > 0
							? new CancellationTokenSource(TimeSpan.FromMinutes(timeout))
							: null;
					cdeThreshold = new CountdownEvent(values.Length / 2);
					// copy to local variable
					Student[] val = values;
					CancellationToken token = cts?.Token ?? CancellationToken.None;
					CountdownEvent threshold = cdeThreshold;
					ScheduledCallbackDelegates<Student> scheduled = e =>
					{
						Interlocked.Increment(ref externalId);
						e.External = externalId;
						return true;
					};
					ProducerConsumerQueueOptions<Student> options = mode switch
					{
						// this time will use waitOnDispose = true with the producer/consumer options
						ThreadQueueMode.ThresholdTaskGroup => new ProducerConsumerThresholdQueueOptions<Student>(threads, true, (_, item) => Exec(item, ref written, visited, duplicates, threshold))
						{
							// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
							Threshold = TimeSpan.FromSeconds(1),
							ScheduledCallback = scheduled,
							WorkStartedCallback = que => QueueStarted(que, val, mode, clock, timeoutString),
							WorkCompletedCallback = que => QueueCompleted(que, val, visited, duplicates, mode, ref externalId, clock, timeoutString, written)
						},
						_ => new ProducerConsumerQueueOptions<Student>(threads, true, (_, item) => Exec(item, ref written, visited, duplicates, threshold))
						{
							ScheduledCallback = scheduled,
							WorkStartedCallback = que => QueueStarted(que, val, mode, clock, timeoutString),
							WorkCompletedCallback = que => QueueCompleted(que, val, visited, duplicates, mode, ref externalId, clock, timeoutString, written)
						}
					};

					visited.Clear();
					duplicates.Clear();
					queue = ProducerConsumerQueue.Create(mode, priorityQueue, options, token);
					
					queue.Pause();
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine($@"For priority queues, we {Yellow("MUST ADD THE VALUES FIRST AT ONCE")} otherwise the queue won't have a chance
{Underline("to determine priorities")} because values will be consumed once entered immediately and therefore,
{Bright.Red("The queue is paused")} {Underline("until values are all entered")}.");
					Console.WriteLine();
					Console.WriteLine($@"{Underline(Yellow("Note:"))} Output might not appear in order because they are  {Underline("processed by threads")} but they actually are.
Follow the {Yellow("xx-* in the number")} printed before the student name and the {Yellow("grade value")}, where xx is the {Underline("Student.ExternalId")}.
The external id reflects the order by which they are scheduled and the -* part in xx-* is the Student.Id and it reflects the order by which they are created.");
					Console.WriteLine();

					foreach (Student value in values)
					{
						if (token.IsCancellationRequested) break;
						queue.Enqueue(value);
					}

					Console.WriteLine(Yellow("Queue is resumed."));
					queue.Resume();

					// wait for the threshold to pause is reached to test the pause feature
					cdeThreshold.Wait(token);

					if (!token.IsCancellationRequested)
					{
						if (!queue.CanPause)
						{
							Title($"Queue does {Bright.Cyan("NOT SUPPORT")} pausing.");
						}
						else
						{
							queue.Pause();
							Console.WriteLine();
							Console.WriteLine(Bright.Red($"Queue is paused for {PAUSE_TIMEOUT / 1000} seconds."));
							TimeSpanHelper.WasteTime(PAUSE_TIMEOUT, token);
							Console.WriteLine(Bright.Red("Queue is done pausing."));
							queue.Resume();
						}
					}

					/*
					* when the queue is being disposed, it will wait until the queued items are processed.
					* this works when queue.WaitOnDispose is true, which is false by default.
					* alternatively, the following can be done to wait for all items to be processed:
					*
					* Important: marking the completion of the queue, will prohibit further items from being
					* added and queue will only process items that already have been queued.
					* queue.Complete();
					* // wait for the queue to finish
					* queue.Wait();
					*
					* another way to go about it, is not to call queue.Complete(); if this queue will
					* wait indefinitely and maybe using a CancellationTokenSource.
					*
					* It's preferable to use queue.Wait() rather than to set WaitOnDispose for the option and
					* to let the queue get disposed of.
					*/
					queue.Complete();
					// this time will not use wait
				}
				finally
				{
					// queue will wait for queued items on dispose, therefore, all other objects MUST be disposed after it.
					ObjectHelper.Dispose(ref queue);
					ObjectHelper.Dispose(ref cdeThreshold);
					ObjectHelper.Dispose(ref cts);
				}

				if (queueModes.Count == 0)
				{
					Console.WriteLine();
					Console.Write($"Would you like to repeat the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to exit. ");

					if (Console.ReadKey(true).Key == ConsoleKey.Y)
					{
						Console.WriteLine();

						foreach (ThreadQueueMode m in modes) 
							queueModes.Enqueue(m);

						if (iteration < MAX_ITERATION_INC) values = GetRandomStudents(values.Length * ++iteration);
					}

					continue;
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				if (response.Key != ConsoleKey.Y) queueModes.Clear();
			}

			clock.Stop();

			static void QueueStarted(IProducerConsumer<Student> queue, IReadOnlyCollection<Student> values, ThreadQueueMode mode, Stopwatch clock, string timeoutString)
			{
				Console.WriteLine();
				Console.WriteLine($"Starting multi-thread test. mode: '{Bright.Cyan(mode.ToString())}', values: {Bright.Cyan(values.Count.ToString())}, threads: {Bright.Cyan(queue.Threads.ToString())}, timeout: {Bright.Cyan(timeoutString)}...");
				if (mode == ThreadQueueMode.ThresholdTaskGroup) Console.WriteLine($"in {mode} mode, {Bright.Cyan(queue.Threads.ToString())} tasks will be issued every {Bright.Cyan(((ProducerConsumerThresholdQueue<Student>)queue).Threshold.TotalSeconds.ToString("N0"))} second(s).");
				Console.WriteLine();
				Console.WriteLine();
				clock.Restart();
			}

			static void QueueCompleted(IProducerConsumer<Student> queue, IReadOnlyCollection<Student> values, ICollection<Student> visited, IList<Student> duplicates, ThreadQueueMode mode, ref int externalId, Stopwatch clock, string timeoutString, int written)
			{
				Interlocked.Exchange(ref externalId, 0);
				long elapsed = clock.ElapsedMilliseconds;
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Finished test. mode: '{Bright.Cyan(mode.ToString())}', threads: {Bright.Cyan(queue.Threads.ToString())}, timeout: {Bright.Cyan(timeoutString)}, elapsed: {Bright.Cyan(elapsed.ToString())} ms.");
				Console.WriteLine($"Values length: {Bright.Cyan(values.Count.ToString())}, written: {Bright.Cyan(written.ToString())}, visited length: {Bright.Cyan(visited.Count.ToString())}, sets length: {Bright.Cyan((visited.Count + duplicates.Count).ToString())}.");

				bool badValues = written != values.Count || visited.Count != values.Count;

				if (badValues)
				{
					Console.WriteLine($"Values: {string.Join(", ", values.OrderBy(e => e.Id).Select(e => $"{e.External:D2}-{e.Id:D2}. {e.Name}"))}.");
					Console.WriteLine(Bright.Cyan($"Visited: {string.Join(", ", visited.OrderBy(e => e).Select(e => $"{e.External:D2}-{e.Id:D2}. {e.Name}"))}."));
				}

				if (duplicates.Count > 0)
				{
					if (!badValues) Console.WriteLine(Bright.Cyan($"Visited: {string.Join(", ", visited.OrderBy(e => e).Select(e => $"{e.External:D2}-{e.Id:D2}. {e.Name}"))}."));
					Console.WriteLine(Bright.Red($"Duplicate entries: {string.Join(", ", duplicates.OrderBy(e => e.Id).Select(e => $"{e.External:D2}-{e.Id:D2}. {e.Name}"))}."));
				}
			}

			static void Exec(Student e, ref int written, ISet<Student> visited, List<Student> duplicates, CountdownEvent threshold)
			{
				Task.Delay(RNGRandomHelper.Next(10, 100)).Execute();
				Console.WriteLine($"{e.External:D2}-{e.Id:D2}. {e.Name}, Grade = {e.Grade:###.##}");
				Interlocked.Increment(ref written);

				lock(visited)
				{
					if (!visited.Add(e))
					{
						lock(duplicates) 
							duplicates.Add(e);
					}
				}
	
				if (threshold.IsSet) return;
				threshold.SignalOne();
			}
		}

		private static void TestAsyncLock()
		{
			Title("Testing AsyncLock");

			AsyncLock locker = null;

			try
			{
				locker = new AsyncLock();
				Task task1 = Method1(locker).ContinueWith(_ => Console.WriteLine(nameof(Method1) + " completed"));
				Task task2 = Method2(locker).ContinueWith(_ => Console.WriteLine(nameof(Method2) + " completed"));
				Task.WhenAll(task1, task2).Execute();
			}
			finally
			{
				ObjectHelper.Dispose(ref locker);
			}

			static async Task<int> Method1(AsyncLock locker)
			{
				await locker.EnterAsync();
				await Task.Delay(2000);
				return 123;
			}

			static async Task<string> Method2(AsyncLock locker)
			{
				await locker.EnterAsync();
				await Task.Delay(2000);
				return "test";
			}
		}
	
		private static void TestSortAlgorithm()
		{
			const string ALGORITHM = nameof(IListExtension.SortInsertion);

			Action<IList<int>, int, int, IComparer<int>, bool> sortNumbers = GetAlgorithm<int>(ALGORITHM);
			Action<IList<string>, int, int, IComparer<string>, bool> sortStrings = GetAlgorithm<string>(ALGORITHM);
			Console.WriteLine($"Testing {Bright.Cyan(ALGORITHM)} algorithm: ");

			Stopwatch watch = new Stopwatch();
			IComparer<int> numbersComparer = Comparer<int>.Default;
			IComparer<string> stringComparer = StringComparer.Ordinal;
			bool more;

			do
			{
				Console.Clear();
				int[] numbers = GetRandomIntegers(RNGRandomHelper.Next(5, 20));
				string[] strings = GetRandomStrings(RNGRandomHelper.Next(3, 10)).ToArray();
				Console.WriteLine(Bright.Cyan("Numbers: ") + string.Join(", ", numbers));
				Console.WriteLine(Bright.Cyan("String: ") + string.Join(", ", strings.Select(e => e.SingleQuote())));

				Console.Write("Numbers");
				watch.Restart();
				sortNumbers(numbers, 0, -1, numbersComparer, false);
				long numericResults = watch.ElapsedMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {Bright.Green(numericResults.ToString())}");
				Console.WriteLine("Result: " + string.Join(", ", numbers));
				Console.WriteLine();

				Console.Write("Strings");
				watch.Restart();
				sortStrings(strings, 0, -1, stringComparer, false);
				long stringResults = watch.ElapsedMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {Bright.Green(stringResults.ToString())}");
				Console.WriteLine("Result: " + string.Join(", ", strings.Select(e => e.SingleQuote())));
				Console.WriteLine();

				Console.WriteLine(Bright.Yellow("Finished"));
				Console.WriteLine();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestSortAlgorithms()
		{
			Title("Testing Sort Algorithms...");

			Stopwatch watch = new Stopwatch();
			IComparer<int> numbersComparer = Comparer<int>.Default;
			IComparer<string> stringComparer = StringComparer.Ordinal;
			IDictionary<string, long> numericResults = new Dictionary<string, long>();
			IDictionary<string, long> stringResults = new Dictionary<string, long>();
			string sectionSeparator = Bright.Magenta(new string('*', 80));
			bool more;
			int tests = 0;
			int[] numbers = GetRandomIntegers(START);
			string[] strings = GetRandomStrings(START).ToArray();

			do
			{
				Console.Clear();

				if (tests == 0)
				{
					Console.WriteLine(Bright.Cyan("Numbers: ") + string.Join(", ", numbers));
					Console.WriteLine(Bright.Cyan("String: ") + string.Join(", ", strings.Select(e => e.SingleQuote())));
					CompilationHint();
				}

				foreach (string algorithm in __sortAlgorithms)
				{
					GC.Collect();
					Action<IList<int>, int, int, IComparer<int>, bool> sortNumbers = GetAlgorithm<int>(algorithm);
					Action<IList<string>, int, int, IComparer<string>, bool> sortStrings = GetAlgorithm<string>(algorithm);
					Console.WriteLine(sectionSeparator);
					Console.WriteLine($"Testing {Bright.Cyan(algorithm)} algorithm: ");

					Console.Write("Numbers");
					int[] ints = (int[])numbers.Clone();
					watch.Restart();
					sortNumbers(ints, 0, -1, numbersComparer, false);
					numericResults[algorithm] = watch.ElapsedTicks;
					Console.WriteLine($" => {Bright.Green(numericResults[algorithm].ToString())}");
					if (tests == 0) Console.WriteLine("Result: " + string.Join(", ", ints));

					Console.Write("Strings");

					string[] str = (string[])strings.Clone();
					watch.Restart();
					sortStrings(str, 0, -1, stringComparer, false);
					stringResults[algorithm] = watch.ElapsedTicks;
					Console.WriteLine($" => {Bright.Green(stringResults[algorithm].ToString())}");
					if (tests == 0) Console.WriteLine("Result: " + string.Join(", ", str.Select(e => e.SingleQuote())));
					Console.WriteLine();
				}

				Console.WriteLine(sectionSeparator);
				Console.WriteLine(Bright.Yellow("Finished"));
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Fastest {TOP_COUNT} numeric sort:"));
			
				foreach (KeyValuePair<string, long> pair in numericResults
															.OrderBy(e => e.Value)
															.Take(TOP_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value}");
				}

				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Fastest {TOP_COUNT} string sort:"));

				foreach (KeyValuePair<string, long> pair in stringResults
															.OrderBy(e => e.Value)
															.Take(TOP_COUNT))
				{
					Console.WriteLine($"{pair.Key} {pair.Value}");
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 2) continue;

				if (tests > 0)
				{
					Console.Write($"Would you like to increase the array size? {Bright.Green("[Y]")} or {Dim("any other key")} to exit. ");
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
			IList<int> values = GetRandomIntegers(true, START);
			SinglyLinkedList<int> list = new SinglyLinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing SingleLinkedList...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Count} items.");
				Console.WriteLine("Test adding...");

				list.Clear();
				int count = list.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Count.ToString())}...");
				clock.Restart();

				foreach (int v in values)
				{
					list.AddLast(v);
					count++;
				}

				Console.WriteLine($"Added {count} items of {values.Count} in {clock.ElapsedMilliseconds} ms.");

				if (list.Count != values.Count)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				Console.WriteLine(Bright.Yellow("Test find a random value..."));
				int x = values.PickRandom();
				SinglyLinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
					return;
				}

				int value = values.PickRandom();
				Console.WriteLine($"Found. Now will add {Bright.Cyan().Underline(value.ToString())} after {Bright.Cyan().Underline(x.ToString())}...");
				list.AddAfter(node, value);
				Console.WriteLine("Node's next: " + node.Next.Value);
				list.Remove(node.Next);

				Console.WriteLine($"Test adding {Bright.Cyan().Underline(value.ToString())} before {Bright.Cyan().Underline(x.ToString())}...");
				SinglyLinkedListNode<int> previous = list.AddBefore(node, value);
				list.Remove(previous);
	
				Console.WriteLine($"Test adding {Bright.Cyan().Underline(value.ToString())} to the beginning of the list...");
				list.AddFirst(value);
				list.RemoveFirst();

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
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
										? Bright.Red($"Remove missed a value: {v} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
					Console.WriteLine("Does it contain the value? " + list.Contains(v).ToYesNo());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
			IList<int> values = GetRandomIntegers(true, START);
			LinkedList<int> list = new LinkedList<int>();

			do
			{
				Console.Clear();
				Title("Testing LinkedList...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Count} items.");
				Console.WriteLine("Test adding...");

				list.Clear();
				int count = list.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Count.ToString())}...");
				clock.Restart();

				foreach (int v in values)
				{
					list.AddLast(v);
					count++;
				}

				Console.WriteLine($"Added {count} items of {values.Count} in {clock.ElapsedMilliseconds} ms.");

				if (list.Count != values.Count)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				Console.WriteLine(Bright.Yellow("Test find a random value..."));
				int x = values.PickRandom();
				LinkedListNode<int> node = list.Find(x);

				if (node == null)
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
					return;
				}

				int value = values.PickRandom();
				Console.WriteLine($"Found. Now will add {Bright.Cyan().Underline(value.ToString())} after {Bright.Cyan().Underline(x.ToString())}...");
				list.AddAfter(node, value);
				Console.WriteLine("Node's next: " + node.Next?.Value);

				Console.WriteLine($"Test adding {Bright.Cyan().Underline(value.ToString())} before {Bright.Cyan().Underline(x.ToString())}...");
				list.AddBefore(node, value);
				Console.WriteLine("Node's previous: " + node.Previous?.Value);
				
				Console.WriteLine($"Test adding {Bright.Cyan().Underline(value.ToString())} to the beginning of the list...");
				list.AddFirst(value);

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
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
										? Bright.Red($"Remove missed a value: {v} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
					Console.WriteLine("Does it contain the value? " + list.Contains(v).ToYesNo());
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Removed {removed} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				bool canPrint = values.Length <= START * 2;
				Console.Clear();
				Title("Testing Deque...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test queue functionality...");

				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 1) continue;
				values = GetRandomIntegers(true, tests == 0 ? START * 2 : HEAVY);
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(Deque<int> deque, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				deque.Clear();
				int count = deque.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (deque.Count != values.Length)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
		
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
			int iteration = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			LinkedDeque<int> deque = new LinkedDeque<int>();

			do
			{
				bool canPrint = values.Length <= START * 2;
				Console.Clear();
				Title("Testing Deque...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test queue functionality...");

				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || iteration > 1) continue;
				values = GetRandomIntegers(true, iteration == 0 ? START * 2 : HEAVY);
				iteration++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(LinkedDeque<int> deque, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				deque.Clear();
				int count = deque.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (deque.Count != values.Length)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
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

		private static void TestCircularBuffer()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = Enumerable.Range(1, START).ToArray();
			CircularBuffer<int> buffer = new CircularBuffer<int>(values.Length / 2);

			do
			{
				bool canPrint = values.Length <= START * 2;
				Console.Clear();
				Title("Testing CircularBuffer...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");

				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();

				// Queue test
				Title("Testing CircularBuffer as a Queue...");
				DoTheTest(buffer, values, buffer.Enqueue, buffer.Dequeue, print, clock);
				Title("End testing CircularBuffer as a Queue...");
				ConsoleHelper.Pause();

				// Stack test
				Title("Testing CircularBuffer as a Stack...");
				DoTheTest(buffer, values, buffer.Enqueue, buffer.Pop, print, clock);
				Title("End testing CircularBuffer as a Queue...");
				
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 1) continue;
				values = Enumerable.Range(1, tests == 0 ? START * 2 : HEAVY).ToArray();
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(CircularBuffer<int> buffer, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				buffer.Clear();
				int count = buffer.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (values.Length >= buffer.Capacity && buffer.Count != buffer.Capacity)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				if (print) Console.WriteLine(string.Join(", ", buffer));

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				int missed = 0;
				int offset = values.Length - buffer.Count;
				count = buffer.Count;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					int v = values[offset + i];

					if (buffer.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Yellow("Test copy..."));
				int[] array = new int[buffer.Capacity];
				buffer.CopyTo(array, 0);
				if (print) Console.WriteLine(string.Join(", ", array));

				Console.WriteLine(Bright.Red("Test removing..."));
		
				int removed = 0;
				count = buffer.Count;
				clock.Restart();

				if (print)
				{
					while (buffer.Count > 0 && count > 0)
					{
						Console.Write(remove());
						count--;
						removed++;
						if (buffer.Count > 0) Console.Write(", ");
					}
				}
				else
				{
					while (buffer.Count > 0 && count > 0)
					{
						remove();
						count--;
						removed++;
					}
				}

				Debug.Assert(count == 0 && buffer.Count == 0, $"Values are not cleared correctly! {count} != {buffer.Count}.");
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Removed {removed} of {buffer.Capacity} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestLinkedCircularBuffer()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = Enumerable.Range(1, START).ToArray();
			LinkedCircularBuffer<int> buffer = new LinkedCircularBuffer<int>(values.Length / 2);

			do
			{
				bool canPrint = values.Length <= START * 2;
				Console.Clear();
				Title("Testing LinkedCircularBuffer...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");

				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();

				// Queue test
				Title("Testing LinkedCircularBuffer as a Queue...");
				DoTheTest(buffer, values, buffer.Enqueue, buffer.Dequeue, print, clock);
				Title("End testing LinkedCircularBuffer as a Queue...");
				ConsoleHelper.Pause();

				// Stack test
				Title("Testing LinkedCircularBuffer as a Stack...");
				DoTheTest(buffer, values, buffer.Enqueue, buffer.Pop, print, clock);
				Title("End testing LinkedCircularBuffer as a Queue...");
				
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 1) continue;
				values = Enumerable.Range(1, tests == 0 ? START * 2 : HEAVY).ToArray();
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(LinkedCircularBuffer<int> buffer, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				buffer.Clear();
				int count = buffer.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (values.Length >= buffer.Capacity && buffer.Count != buffer.Capacity)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				if (print) Console.WriteLine(string.Join(", ", buffer));

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				int missed = 0;
				int offset = values.Length - buffer.Count;
				count = buffer.Count;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					int v = values[offset + i];

					if (buffer.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Yellow("Test copy..."));
				int[] array = new int[buffer.Capacity];
				buffer.CopyTo(array, 0);
				if (print) Console.WriteLine(string.Join(", ", array));

				Console.WriteLine(Bright.Red("Test removing..."));
		
				int removed = 0;
				count = buffer.Count;
				clock.Restart();

				if (print)
				{
					while (buffer.Count > 0 && count > 0)
					{
						Console.Write(remove());
						count--;
						removed++;
						if (buffer.Count > 0) Console.Write(", ");
					}
				}
				else
				{
					while (buffer.Count > 0 && count > 0)
					{
						remove();
						count--;
						removed++;
					}
				}

				Debug.Assert(count == 0 && buffer.Count == 0, $"Values are not cleared correctly! {count} != {buffer.Count}.");
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Removed {removed} of {buffer.Capacity} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestBitCollection()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			IList<uint> values = Enumerable.Range(1, START).Select(e => (uint)e).ToArray();
			BitCollection collection = new BitCollection(values.Max());

			do
			{
				bool canPrint = values.Count <= START * 2;
				Console.Clear();
				Title("Testing BitCollection...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Count} items.");

				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();

				DoTheTest(collection, values, print, clock);
				
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 1) continue;
				values = Enumerable.Range(1, tests == 0 ? START * 2 : HEAVY).Select(e => (uint)e).ToArray();
				collection.Maximum = values.Max();
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(BitCollection collection, IList<uint> values, bool print, Stopwatch clock)
			{
				collection.Clear();
				int count = collection.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Count.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (uint v in values)
				{
					collection.Add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Count} items in {clock.ElapsedMilliseconds} ms.");

				if (collection.Count != values.Count)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				if (print) Console.WriteLine(string.Join(", ", collection));

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				int missed = 0;
				count = collection.Count;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					uint v = values[i];

					if (collection.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Yellow("Test toggle..."));
				count = collection.Count / 4;
				
				HashSet<uint> set = new HashSet<uint>(count);

				while (set.Count < count)
				{
					uint value = values.PickRandom();
					set.Add(value);
				}

				if (print) Console.WriteLine($"Toggling values: {string.Join(", ", set)}");
				clock.Restart();

				foreach (uint value in set)
				{
					collection.Toggle(value);
				}

				Console.WriteLine($"Toggled {set.Count} items in {clock.ElapsedMilliseconds} ms.");
				if (print) Console.WriteLine(string.Join(", ", collection));

				Console.WriteLine(Bright.Yellow("Test copy..."));
				uint[] array = new uint[collection.Count];
				collection.CopyTo(array, 0);
				if (print) Console.WriteLine(string.Join(", ", array));

				Console.WriteLine(Bright.Red("Test removing..."));
		
				int removed = 0;
				int collectionCount = collection.Count;
				count = collection.Count;
				clock.Restart();

				foreach (uint value in values)
				{
					if (!collection.Remove(value)) continue;
					count--;
					removed++;
				}

				Debug.Assert(count == 0 && collection.Count == 0, $"Values are not cleared correctly! {count} != {collection.Count}.");
				Console.WriteLine();
				Console.WriteLine($"Removed {removed} of {collectionCount} items in {clock.ElapsedMilliseconds} ms.");
			}
		}

		private static void TestQueueAdapter()
		{
			bool more;
			int tests = 0;
			Stopwatch clock = new Stopwatch();
			int[] values = GetRandomIntegers(true, START);
			QueueAdapter<Queue<int>, int> adapter = new QueueAdapter<Queue<int>, int>(new Queue<int>());

			do
			{
				bool canPrint = values.Length <= START * 2;
				Console.Clear();
				Title("Testing QueueAdapter...");
				CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");
				Console.WriteLine("Test queue functionality...");
				if (canPrint) Console.Write($"Would you like to print the results? {Bright.Green("[Y]")} or {Dim("any other key")}: ");
				bool print = canPrint && Console.ReadKey(true).Key == ConsoleKey.Y;
				Console.WriteLine();
				DoTheTest(adapter, values, adapter.Enqueue, adapter.Dequeue, print, clock);
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || tests > 1) continue;
				values = GetRandomIntegers(true, tests == 0 ? START * 2 : HEAVY);
				tests++;
			}
			while (more);

			clock.Stop();

			static void DoTheTest(QueueAdapter<Queue<int>, int> adapter, int[] values, Action<int> add, Func<int> remove, bool print, Stopwatch clock)
			{
				adapter.Clear();
				int count = adapter.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				if (print) Console.WriteLine(string.Join(", ", values));
				clock.Restart();

				foreach (int v in values)
				{
					add(v);
					count++;
				}

				Console.WriteLine($"Added {count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				if (adapter.Count != values.Length)
				{
					Console.WriteLine(Bright.Red("Something went wrong, Count isn't right...!"));
					return;
				}

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				int missed = 0;
				count = adapter.Count / 4;
				clock.Restart();

				// will just test for items not more than MAX_SEARCH
				for (int i = 0; i < count; i++)
				{
					int v = values[i];

					if (adapter.Contains(v))
					{
						found++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
		
				int removed = 0;
				count = adapter.Count;
				clock.Restart();

				if (print)
				{
					while (adapter.Count > 0 && count > 0)
					{
						Console.Write(remove());
						count--;
						removed++;
						if (adapter.Count > 0) Console.Write(", ");
					}
				}
				else
				{
					while (adapter.Count > 0 && count > 0)
					{
						remove();
						count--;
						removed++;
					}
				}

				Debug.Assert(count == 0 && adapter.Count == 0, $"Values are not cleared correctly! {count} != {adapter.Count}.");
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
						Console.WriteLine($@"Data from {Bright.Cyan("LevelOrder")} traversal:
{TREE_DATA_LEVEL}");
						tree.FromLevelOrder(TREE_DATA_LEVEL);
						break;
					case 1:
						Console.WriteLine($@"Data from {Bright.Cyan("PreOrder")} traversal:
{TREE_DATA_PRE}");
						tree.FromPreOrder(TREE_DATA_PRE);
						break;
					case 2:
						Console.WriteLine($@"Data from {Bright.Cyan("InOrder")} traversal:
{TREE_DATA_IN}");
						tree.FromInOrder(TREE_DATA_IN);
						break;
					case 3:
						Console.WriteLine($@"Data from {Bright.Cyan("PostOrder")} traversal:
{TREE_DATA_POST}");
						tree.FromPostOrder(TREE_DATA_POST);
						break;
					case 4:
						Console.WriteLine($@"Data from {Bright.Cyan("InOrder")} and {Bright.Cyan("LevelOrder")} traversals:
{TREE_DATA_IN}
{TREE_DATA_LEVEL}");
						tree.FromInOrderAndLevelOrder(TREE_DATA_IN, TREE_DATA_LEVEL);
						break;
					case 5:
						Console.WriteLine($@"Data from {Bright.Cyan("InOrder")} and {Bright.Cyan("PreOrder")} traversals:
{TREE_DATA_IN}
{TREE_DATA_PRE}");
						tree.FromInOrderAndPreOrder(TREE_DATA_IN, TREE_DATA_PRE);
						break;
					case 6:
						Console.WriteLine($@"Data from {Bright.Cyan("InOrder")} and {Bright.Cyan("PostOrder")} traversals:
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
				Console.Write($"Press {Bright.Green("[Y]")} to move to next test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(true, len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Count, $"Values are not added correctly! {values.Count} != {tree.Count}.");
				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");
				int value = values.PickRandom();
				Console.WriteLine($"will look for {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
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

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Count} items.");

				Console.WriteLine();
				Console.WriteLine("Test removing a random value...");
				value = values.PickRandom();
				Console.WriteLine($"will remove {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
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
						Debug.Assert(values.Count - removed == tree.Count, $"Values are not removed correctly! {values.Count - removed} != {tree.Count}.");
						continue;
					}

					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(true, len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();
				tree.Add(values);
				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine(Bright.Red("Test removing..."));
				int value = values.PickRandom();
				Console.WriteLine($"will remove {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
					return;
				}

				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine($"Closest value to {Yellow(value.ToString())} => {tree.FindClosestValue(value, -1)} ");

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				tree.Clear();
				tree.Add(values);
				tree.Print();
				Console.WriteLine(Bright.Black("Branch Sums: ") + string.Join(", ", tree.BranchSums()));

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				tree.Clear();
				tree.Add(values);
				tree.Print();
				Console.WriteLine(Bright.Yellow("Inverted: "));

				tree.Invert();
				tree.Print();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(true, len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Count, $"Values are not added correctly! {values.Count} != {tree.Count}.");
				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");
				int value = values.PickRandom();
				Console.WriteLine($"will look for {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
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

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Count} items.");

				Console.WriteLine(Bright.Red("Test removing..."));
				value = values.PickRandom();
				Console.WriteLine($"will remove {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
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
						Debug.Assert(values.Count - removed == tree.Count, $"Values are not removed correctly! {values.Count - removed} != {tree.Count}.");
						continue;
					}
					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(true, len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine(Bright.Green("Test adding..."));
				tree.Clear();
				tree.Add(values);
				Debug.Assert(tree.Count == values.Count, $"Values are not added correctly! {values.Count} != {tree.Count}.");
				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine("Test finding a random value...");

				int value = values.PickRandom();
				Console.WriteLine($"will look for {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Contains(value))
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
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

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}
				Console.WriteLine($"Found {found} of {values.Count} items.");

				Console.WriteLine();
				Console.WriteLine(Bright.Red("Test removing..."));
				value = values.PickRandom();
				Console.WriteLine($"will remove {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
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
						Debug.Assert(values.Count - removed == tree.Count, $"Values are not removed correctly! {values.Count - removed} != {tree.Count}.");
						continue;
					}
					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
				}

				Console.WriteLine("OK");
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(true, len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				DoTheTest(binarySearchTree, values);

				DoTheTest(avlTree, values);

				DoTheTest(redBlackTree, values);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, IList<int> array)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {tree.GetType().Name}..."));
				tree.Clear();
				tree.Add(array);

				Console.WriteLine(Bright.Black("InOrder: ") + string.Join(", ", tree));
				tree.PrintWithProps();

				Console.WriteLine(Bright.Red("Test removing..."));
				int value = array.PickRandom();
				Console.WriteLine($"will remove {Bright.Cyan().Underline(value.ToString())}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
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
				Title("Testing all BinaryTrees functionality...");

				DoTheTest(binarySearchTree, values);
				ConsoleHelper.Pause();

				DoTheTest(avlTree, values);
				ConsoleHelper.Pause();

				DoTheTest(redBlackTree, values);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] values)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {tree.GetType().Name}..."));
				tree.Clear();
				Debug.Assert(tree.Count == 0, "Values are not cleared correctly!");

				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				Console.WriteLine();
				Console.WriteLine($"Array: {string.Join(", ", values)}");
				
				tree.Add(values);
				Console.WriteLine($"Added {tree.Count} of {values.Length} items.");
				tree.PrintProps();

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {values.Length} items.");

				tree.Print();

				Console.WriteLine(Bright.Red("Test removing..."));
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
										? Bright.Red($"Remove missed a value: {v} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Green($"Testing {tree.GetType().Name}..."));
				tree.Clear();
				Debug.Assert(tree.Count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");

				clock.Restart();
				tree.Add(values);
				Console.WriteLine($"Added {tree.Count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");
				tree.PrintProps();

				Console.WriteLine(Bright.Yellow("Test search..."));
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
										? Bright.Red($"Find missed a value: {v} :((")
										: Bright.Red("FIND MISSED A LOT :(("));
					if (missed > 3) return;
					//return;
				}
				Console.WriteLine($"Found {found} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
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
										? Bright.Red($"Remove missed a value: {v} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				Console.WriteLine(Bright.Green($"Testing {sortedSet.GetType().Name}..."));
				sortedSet.Clear();

				int count = sortedSet.Count;
				Debug.Assert(count == 0, "Values are not cleared correctly!");
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				clock.Restart();

				foreach (T v in values)
				{
					sortedSet.Add(v);
					count++;
					Debug.Assert(count == sortedSet.Count, $"Values are not added correctly! {count} != {sortedSet.Count}.");
				}

				Console.WriteLine($"Added {count} items of {values.Length} in {clock.ElapsedMilliseconds} ms.");
				Console.WriteLine();
				Console.WriteLine($"{Yellow("Count:")} {Underline(sortedSet.Count.ToString())}.");
				Console.WriteLine($"{Yellow("Minimum:")} {sortedSet.Min} {Yellow("Maximum:")} {sortedSet.Max}");

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				clock.Restart();

				foreach (T v in values)
				{
					if (sortedSet.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
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
					
					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				Console.WriteLine();
				Console.WriteLine(Bright.Black("Testing BinarySearchTree: ") + string.Join(", ", values));
				Console.WriteLine();
				LinkedBinaryTree<int> tree1 = new BinarySearchTree<int>();
				LinkedBinaryTree<int> tree2 = new BinarySearchTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine(Bright.Black("Testing BinarySearchTree and AVLTree: ") + string.Join(", ", values));
				Console.WriteLine();
				tree1.Clear();
				tree2 = new AVLTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine(Bright.Black("Testing AVLTree: ") + string.Join(", ", values));
				Console.WriteLine();
				tree1 = new AVLTree<int>();
				tree2 = new AVLTree<int>();
				DoTheTest(tree1, tree2, values);

				Console.WriteLine();
				Console.WriteLine(Bright.Black("Testing RedBlackTree: ") + string.Join(", ", values));
				Console.WriteLine();
				RedBlackTree<int> rbTree1 = new RedBlackTree<int>();
				RedBlackTree<int> rbTree2 = new RedBlackTree<int>();
				DoTheTest(rbTree1, rbTree2, values);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree1, LinkedBinaryTree<TNode, int> tree2, int[] array)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {tree1.GetType().Name} and {tree1.GetType().Name}..."));
				tree1.Add(array);
				tree2.Add(array);

				Console.WriteLine(Bright.Black("InOrder1: ") + string.Join(", ", tree1));
				Console.WriteLine(Bright.Black("InOrder2: ") + string.Join(", ", tree2));
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
				Console.WriteLine(Bright.Black("Words list: ") + string.Join(", ", values));

				string word = values.PickRandom();
				DoTheTest(trie, word, values);
				
				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || values.Count >= MAX_LIST) continue;

				Console.Write($"Would you like to add more words? {Bright.Green("[Y]")} / {Dim("any key")} ");
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
				Console.WriteLine(Bright.Green($"Generating {len} words: "));
				ICollection<string> newValues = GetRandomStrings(true, len);

				foreach (string value in newValues)
				{
					if (!set.Add(value)) continue;
					trie.Add(value);
				}
			}

			static void DoTheTest(Trie<char> trie, string token, ISet<string> values)
			{
				Console.WriteLine($"Test find '{Bright.Cyan().Underline(token)}'...");

				if (!trie.Contains(token))
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
					return;
				}
				
				Console.WriteLine(Bright.Green("Found...!") + " Let's try all caps...");

				if (!trie.Contains(token.ToUpperInvariant()))
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
					return;
				}

				Console.WriteLine(Bright.Green("Found...!") + " Let's try words with a common prefix...");

				string prefix = token;

				if (prefix.Length > 1)
				{
					Match match = Regex.Match(prefix, @"^([\w\-]+)");
					prefix = !match.Success || match.Value.Length == prefix.Length
								? prefix.Left(prefix.Length / 2)
								: match.Value;
				}

				Console.WriteLine($"Prefix: '{Bright.Cyan().Underline(prefix)}'");
				int results = 0;

				foreach (IEnumerable<char> enumerable in trie.Find(prefix))
				{
					Console.WriteLine($"{++results}: " + string.Concat(enumerable));
				}

				if (results == 0)
				{
					Console.WriteLine(Bright.Red("Didn't find a shit...!"));
					return;
				}

				int tries = 3;

				while (prefix.Length > 1 && results < 2 && --tries > 0)
				{
					results = 0;
					prefix = prefix.Left(prefix.Length / 2);
					Console.WriteLine();
					Console.WriteLine($"Results were too few, let's try another prefix: '{Bright.Cyan().Underline(prefix)}'");

					foreach (IEnumerable<char> enumerable in trie.Find(prefix))
					{
						Console.WriteLine($"{++results}: " + string.Concat(enumerable));
					}
				}

				Console.WriteLine();
				Console.WriteLine($"Test remove '{Bright.Red().Underline(token)}'");

				if (!trie.Remove(token))
				{
					Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
					return;
				}

				values.Remove(token);
				results = 0;
				Console.WriteLine();
				Console.WriteLine($"Cool {Bright.Green("removed")}, let's try to find the last prefix again: '{Bright.Cyan().Underline(prefix)}'");

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
			Console.WriteLine(Bright.Black("Words list: ") + string.Join(", ", values));

			foreach (string value in values) 
				trie.Add(value);

			int results = 0;
			string prefix = "car";
			Console.WriteLine();
			Console.WriteLine($"Test find '{Bright.Cyan().Underline(prefix)}'...");

			foreach (IEnumerable<char> enumerable in trie.Find(prefix))
			{
				Console.WriteLine($"{++results}: " + string.Concat(enumerable));
			}

			if (results == 0)
			{
				Console.WriteLine(Bright.Red("Didn't find a shit...!"));
				return;
			}

			string word = values[0];
			Console.WriteLine();
			Console.WriteLine($"Test remove '{Bright.Red().Underline(word)}'");

			if (!trie.Remove(word))
			{
				Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
				return;
			}

			results = 0;
			Console.WriteLine($"Cool {Bright.Green("removed")}.");
			Console.WriteLine();
			Console.WriteLine($"let's try to find the last prefix again: '{Bright.Cyan().Underline(prefix)}'");

			foreach (IEnumerable<char> enumerable in trie.Find(prefix))
			{
				Console.WriteLine($"{++results}: " + string.Concat(enumerable));
			}

			word = values[values.Length - 1];
			Console.WriteLine();
			Console.WriteLine($"Test remove '{Bright.Red().Underline(word)}'");

			if (!trie.Remove(word))
			{
				Console.WriteLine(Bright.Red("Didn't remove a shit...!"));
				return;
			}

			prefix = "ca";
			results = 0;
			Console.WriteLine($"Cool {Bright.Green("removed")}.");
			Console.WriteLine();
			Console.WriteLine($"Test find '{Bright.Cyan().Underline(prefix)}'...");

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

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (skipList.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Found {found} of {count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.WriteLine(Bright.Red("Test removing..."));
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
					
					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
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
					Console.WriteLine(Bright.Red($"Something went wrong, the count is {skipList.Count}...!"));
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
			IList<int> values = GetRandomIntegers(true, 12/*200_000*/);

			do
			{
				Console.Clear();
				Title("Testing DisjointSet...");
				Console.WriteLine($"Array has {values.Count} items.");
				disjointSet.Clear();

				clock.Restart();

				foreach (int v in values)
					disjointSet.Add(v);

				Console.WriteLine($"Added {disjointSet.Count} items of {values.Count} in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Yellow("Test search..."));
				int found = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (disjointSet.Contains(v))
					{
						found++;
						continue;
					}

					Console.WriteLine(Bright.Red($"Find missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Found {found} of {disjointSet.Count} items in {clock.ElapsedMilliseconds} ms.");

				
				Console.WriteLine(Bright.Yellow("Test find and union..."));

				int threshold = (int)Math.Floor(disjointSet.Count / 0.5d);

				for (int i = 0; i < threshold; i++)
				{
					int x = values.PickRandom(), y = values.PickRandom();
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
				Console.WriteLine(Bright.Red("Test removing..."));
				int removed = 0;
				clock.Restart();

				foreach (int v in values)
				{
					if (disjointSet.Remove(v))
					{
						removed++;
						continue;
					}
					
					Console.WriteLine(Bright.Red($"Remove missed a value: {v} :(("));
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}
				Console.WriteLine($"Removed {removed} of {values.Count} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine();
				Console.WriteLine("Test to clear the list...");
				disjointSet.Clear();

				if (disjointSet.Count != 0)
				{
					Console.WriteLine(Bright.Red($"Something went wrong, the count is {disjointSet.Count}...!"));
					ConsoleHelper.Pause();
					Console.WriteLine();
					//return;
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				IList<int> values = GetRandomIntegers(len);
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, IList<TValue> array)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("Enumeration(InOrder - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(LevelOrder): ") + string.Join(", ", heap.Enumerate(TreeTraverseMethod.LevelOrder)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(InOrder - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(LevelOrder): ") + string.Join(", ", heap.Enumerate(TreeTraverseMethod.LevelOrder)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinBinaryHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinaryHeap<double, Student> studentHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(InOrder - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(LevelOrder): ") + string.Join(", ", heap.Enumerate(TreeTraverseMethod.LevelOrder)));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} = {Bright.Cyan().Underline(heap.ElementAt(k).ToString())}");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				BinaryHeap<int> heap = new MaxBinaryHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinBinaryHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinaryHeap<double, Student> studentHeap = new MaxBinaryHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue);

				studentHeap = new MinBinaryHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(BinaryHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue)
				where TNode : KeyedBinaryNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				while (queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, key);
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

				while (queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
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
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine(Bright.Black("Enumeration(InOrder - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(LevelOrder): ") + string.Join(", ", heap.Enumerate(TreeTraverseMethod.LevelOrder)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.Print();
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheTest(heap, values);

				heap = new MinBinomialHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinBinomialHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {Bright.Cyan().Underline(heap.ElementAt(k).ToString())}");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				BinomialHeap<int> heap = new MaxBinomialHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinBinomialHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				BinomialHeap<double, Student> studentHeap = new MaxBinomialHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue);

				studentHeap = new MinBinomialHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(BinomialHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue)
				where TNode : BinomialNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				while (queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, key);
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

				while (queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
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
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheTest(heap, values);

				heap = new MinPairingHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinPairingHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {Bright.Cyan().Underline(heap.ElementAt(k).ToString())}");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				PairingHeap<int> heap = new MaxPairingHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinPairingHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				PairingHeap<double, Student> studentHeap = new MaxPairingHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue);

				studentHeap = new MinPairingHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(PairingHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue)
				where TNode : PairingNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				while (queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, key);
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

				while (queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
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
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue value in array)
				{
					heap.Add(value);
					//heap.PrintWithProps();
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheTest(heap, values);

				heap = new MinFibonacciHeap<int>();
				DoTheTest(heap, values);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinFibonacciHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheTest(studentHeap, students, k);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array, int k)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));
				heap.Add(array);
				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
				heap.Print();
				Console.WriteLine($"Kth element at position {k} element = {Bright.Cyan().Underline(heap.ElementAt(k).ToString())}");
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
				Console.WriteLine(Bright.Black("Array: ") + string.Join(", ", values));
				Console.WriteLine(Yellow("Array [sorted]: ") + string.Join(", ", values.OrderBy(e => e)));

				FibonacciHeap<int> heap = new MaxFibonacciHeap<int>();
				DoTheValueTest(heap, values, int.MaxValue);

				heap = new MinFibonacciHeap<int>();
				DoTheValueTest(heap, values, int.MinValue);

				Student[] students = GetRandomStudents(len);
				Console.WriteLine(Bright.Black("Students: ") + string.Join(", ", students.Select(e => $"{e.Name} {e.Grade:F2}")));
				Console.WriteLine(Yellow("Students [sorted]: ") + string.Join(", ", students.OrderBy(e => e.Grade).Select(e => $"{e.Name} {e.Grade:F2}")));

				FibonacciHeap<double, Student> studentHeap = new MaxFibonacciHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MaxValue);

				studentHeap = new MinFibonacciHeap<double, Student>(e => e.Grade);
				DoTheKeyTest(studentHeap, students, int.MinValue);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheKeyTest<TNode, TKey, TValue>(FibonacciHeap<TNode, TKey, TValue> heap, TValue[] array, TKey newKeyValue)
				where TNode : FibonacciNode<TNode, TKey, TValue>
			{
				Queue<TKey> queue = new Queue<TKey>();
				DoTheTest(heap, array, queue);

				while (queue.Count > 0)
				{
					TKey key = queue.Dequeue();
					TNode node = heap.FindByKey(key);
					Debug.Assert(node != null, $"Node for key {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TKey extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, key);
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

				while (queue.Count > 0)
				{
					TValue key = queue.Dequeue();
					TNode node = heap.Find(key);
					Debug.Assert(node != null, $"Node for value {key} is not found.");
					heap.DecreaseKey(node, newKeyValue);
					TValue extracted = heap.ExtractValue().Key;
					bool succeeded = heap.Comparer.IsEqual(extracted, newKeyValue);
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
				Console.WriteLine(Bright.Green($"Test adding ({heap.GetType().Name})..."));

				foreach (TValue v in array)
				{
					TNode node = heap.MakeNode(v);
					if (queue.Count < max) queue.Enqueue(node.Key);
					heap.Add(node);
				}

				Console.WriteLine(Bright.Black("Enumeration(BFS - Default): ") + string.Join(", ", heap));
				Console.WriteLine(Bright.Black("Enumeration(DFS): ") + string.Join(", ", heap.Enumerate(BreadthDepthTraversal.DepthFirst)));
				heap.Print();
			}
		}

		private static void TestAllHeapsPerformance()
		{
			bool more;
			Stopwatch clock = new Stopwatch();
			int tests = 0;
			int[] values = GetRandomIntegers(true, START);
			IDictionary<string, long> result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			Student[] students = GetRandomStudents(START);
			Func<Student, double> getKey = e => e.Grade;

			do
			{
				Console.Clear();
				Title("Testing All Heap types performance...");
				if (tests == 0) CompilationHint();
				Console.WriteLine($"Array has {values.Length} items.");
				Title("Testing IHeap<int> types performance...");
				result.Clear();

				// BinaryHeap
				DoHeapTest(new MinBinaryHeap<int>(), values, clock);
				result[typeof(MinBinaryHeap<int>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// BinomialHeap
				DoHeapTest(new MinBinomialHeap<int>(), values, clock);
				result[typeof(MinBinomialHeap<int>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// PairingHeap
				DoHeapTest(new MinPairingHeap<int>(), values, clock);
				result[typeof(MinPairingHeap<int>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// FibonacciHeap
				DoHeapTest(new MinFibonacciHeap<int>(), values, clock);
				result[typeof(MinFibonacciHeap<int>).Name] = clock.ElapsedTicks;
				clock.Stop();

				Console.WriteLine();
				Console.WriteLine("Results for Heap<int>:");

				foreach (KeyValuePair<string, long> pair in result.OrderBy(e => e.Value))
				{
					Console.WriteLine($"{pair.Key} took {pair.Value} ticks");
				}

				ConsoleHelper.Pause();

				Title("Testing IKeyedHeap<TNode, TKey, TValue> types performance...");
				result.Clear();
				
				// BinaryHeap
				DoHeapTest(new MinBinaryHeap<double, Student>(getKey), students, clock);
				result[typeof(MinBinaryHeap<double, Student>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// BinomialHeap
				DoHeapTest(new MinBinomialHeap<double, Student>(getKey), students, clock);
				result[typeof(MinBinomialHeap<double, Student>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// PairingHeap
				DoHeapTest(new MinPairingHeap<double, Student>(getKey), students, clock);
				result[typeof(MinPairingHeap<double, Student>).Name] = clock.ElapsedTicks;
				clock.Stop();

				// FibonacciHeap
				DoHeapTest(new MinFibonacciHeap<double, Student>(getKey), students, clock);
				result[typeof(MinFibonacciHeap<double, Student>).Name] = clock.ElapsedTicks;
				clock.Stop();

				Console.WriteLine();
				Console.WriteLine("Results for Heap<double, Student>:");

				foreach (KeyValuePair<string, long> pair in result.OrderBy(e => e.Value))
				{
					Console.WriteLine($"{pair.Key} took {pair.Value} ticks");
				}

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
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
				ICollection collection = heap;
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {heap.GetType().Name}..."));
				heap.Clear();
				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				Debug.Assert(collection.Count == 0, "Values are not cleared correctly!");

				clock.Restart();
				heap.Add(values);
				Console.WriteLine($"Added {collection.Count} of {values.Length} items in {clock.ElapsedMilliseconds} ms.");

				Console.WriteLine(Bright.Red("Test removing..."));
				int removed = 0;
				clock.Restart();

				while (collection.Count > 0)
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
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
				if (!more || values.Count >= MAX_LIST) continue;

				Console.Write($"Would you like to add more character? {Bright.Green("[Y]")} / {Dim("any key")} ");
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
				Console.WriteLine(Bright.Green("Generating new characters: "));
				char[] newValues = GetRandomChar(true, len);
				int count = 0;

				foreach (char value in newValues)
				{
					if (list.Contains(value)) continue;
					list.Add(value);
					count++;
				}

				Console.WriteLine(Bright.Green($"Added {count} characters to the set"));
			}

			static void DoTheTest<TAdjacencyList, TEdge>(GraphList<char, TAdjacencyList, TEdge> graph, IList<char> values)
				where TAdjacencyList : class, ICollection<TEdge>
			{
				Console.WriteLine("Test adding nodes...");
				Console.WriteLine(Bright.Black("characters list: ") + string.Join(", ", values));
				graph.Clear();
				graph.Add(values);

				if (graph.Count != values.Count)
				{
					Console.WriteLine(Bright.Red("Something went wrong, not all nodes were added...!"));
					return;
				}

				if (graph.Count == 1)
				{
					Console.WriteLine(Bright.Red("Huh, must add more nodes...!"));
					return;
				}
				
				Console.WriteLine(Bright.Green("All nodes are added...!") + " Let's try adding some relationships...");
				Console.Write($@"{Yellow("Would you like to add a bit of randomization?")} {Bright.Green("[Y]")} / {Dim("any key")}.
This may cause cycles but will make it much more fun for finding shortest paths. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				int threshold = response.Key != ConsoleKey.Y ? 0 : (int)Math.Floor(values.Count * 0.5d);

				Console.Write($"{Yellow("Can the edges have negative weights?")} {Bright.Green("[Y]")} / {Dim("any key")}.");
				response = Console.ReadKey(true);
				Console.WriteLine();
				int min = response.Key == ConsoleKey.Y
							? sbyte.MinValue
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
					Console.WriteLine($"Adding {Bright.Cyan().Underline(from.ToString())} to {Bright.Cyan().Underline(to.ToString())}...");
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
				Console.WriteLine($"Picking a value with maximum connections: '{Bright.Cyan().Underline(value.ToString())}'...");
				if (!DoTheTestWithValue(graph, values, value)) return;

				do
				{
					Console.WriteLine();
					Console.Write($@"Type in {Bright.Green("a character")} to traverse from,
or press {Bright.Red("ESCAPE")} key to exit this test. ");
					response = Console.ReadKey();
					Console.WriteLine();
					if (response.Key == ConsoleKey.Escape) continue;

					if (!char.IsLetter(response.KeyChar) || !graph.ContainsKey(response.KeyChar))
					{
						Console.WriteLine($"Character '{value}' is not found or not connected!");
						continue;
					}

					value = response.KeyChar;
					if (!DoTheTestWithValue(graph, values, value)) return;
				}
				while (response.Key != ConsoleKey.Escape);

				Console.WriteLine();
			}

			static bool DoTheTestWithValue<TAdjacencyList, TEdge>(GraphList<char, TAdjacencyList, TEdge> graph, IList<char> values, char value)
				where TAdjacencyList : class, ICollection<TEdge>
			{
				const string LINE_SEPARATOR = "*******************************************************************************";

				Console.WriteLine(Yellow("Breadth First: ") + string.Join(", ", graph.Enumerate(value, BreadthDepthTraversal.BreadthFirst)));
				Console.WriteLine(Yellow("Depth First: ") + string.Join(", ", graph.Enumerate(value, BreadthDepthTraversal.DepthFirst)));
				Console.WriteLine(Yellow("Degree: ") + graph.Degree(value));

				// detect a cycle
				IEnumerable<char> cycle = graph.FindCycle();
				if (cycle != null) Console.WriteLine(Bright.Red("Found cycle: ") + string.Join(", ", cycle));

				// test specific graph type features
				switch (graph)
				{
					case DirectedGraphList<char> directedGraph:
						Console.WriteLine(Yellow("InDegree: ") + directedGraph.InDegree(value));
						try { Console.WriteLine(Yellow("Topological Sort: ") + string.Join(", ", directedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine(Yellow("Topological Sort: ") + Bright.Red(e.Message)); }
						break;
					case MixedGraphList<char> mixedGraph:
						Console.WriteLine(Yellow("InDegree: ") + mixedGraph.InDegree(value));
						try { Console.WriteLine(Yellow("Topological Sort: ") + string.Join(", ", mixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine(Yellow("Topological Sort: ") + Bright.Red(e.Message)); }
						break;
					case WeightedDirectedGraphList<char, int> weightedDirectedGraph:
						Console.WriteLine(Yellow("InDegree: ") + weightedDirectedGraph.InDegree(value));
						try { Console.WriteLine(Yellow("Topological Sort: ") + string.Join(", ", weightedDirectedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine(Yellow("Topological Sort: ") + Bright.Red(e.Message)); }
						break;
					case WeightedUndirectedGraphList<char, int> weightedUndirectedGraph:
						Console.WriteLine(LINE_SEPARATOR);
						WeightedUndirectedGraphList<char, int> spanningTree = weightedUndirectedGraph.GetMinimumSpanningTree(SpanningTreeAlgorithm.Prim);

						if (spanningTree != null)
						{
							Console.WriteLine(Yellow("Prim Spanning Tree: "));
							spanningTree.Print();
							Console.WriteLine(LINE_SEPARATOR);
						}
						
						spanningTree = weightedUndirectedGraph.GetMinimumSpanningTree(SpanningTreeAlgorithm.Kruskal);

						if (spanningTree != null)
						{
							Console.WriteLine(Yellow("Kruskal Spanning Tree: "));
							spanningTree.Print();
							Console.WriteLine(LINE_SEPARATOR);
						}
						break;
					case WeightedMixedGraphList<char, int> weightedMixedGraph:
						Console.WriteLine(Yellow("InDegree: ") + weightedMixedGraph.InDegree(value));
						try { Console.WriteLine(Yellow("Topological Sort: ") + string.Join(", ", weightedMixedGraph.TopologicalSort())); }
						catch (Exception e) { Console.WriteLine(Yellow("Topological Sort: ") + Bright.Red(e.Message)); }
						break;
				}

				if (graph is WeightedGraphList<char, int> wGraph)
				{
					char to = values.PickRandom();
					ConsoleKeyInfo response;

					do
					{
						Console.Write($@"Current position is '{Bright.Green(value.ToString())}'. Type in {Bright.Green("a character")} to find the shortest path to,
(You can press the {Bright.Green("RETURN")} key to accept the current random value '{Bright.Green(to.ToString())}'),
or press {Bright.Red("ESCAPE")} key to exit this test. ");
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
					Console.WriteLine($"{Yellow("Shortest Path")} from '{Bright.Cyan(value.ToString())}' to '{Bright.Cyan(to.ToString())}'");
					
					Console.Write("Dijkstra: ");
					try { Console.WriteLine(string.Join(" -> ", wGraph.SingleSourcePath(value, to, SingleSourcePathAlgorithm.Dijkstra))); }
					catch (Exception e) { Console.WriteLine(Bright.Red(e.Message)); }
					
					Console.Write("Bellman-Ford: ");
					try { Console.WriteLine(string.Join(" -> ", wGraph.SingleSourcePath(value, to, SingleSourcePathAlgorithm.BellmanFord))); }
					catch (Exception e) { Console.WriteLine(Bright.Red(e.Message)); }
				}

				return true;
			}
		}

		private static void TestAsymmetric()
		{
			RSASettings settings = new RSASettings {
				Encoding = Encoding.UTF8,
				KeySize = 512,
				SaltSize = 8,
				Padding = RSAEncryptionPadding.Pkcs1,
				UseExpiration = false
			};
			(string publicKey, string privateKey) = QuickCipher.GenerateAsymmetricKeys(false, settings);
			Title("Generated keys");
			Console.WriteLine($@"Public:
'{publicKey}'

Private:
'{privateKey}'
");

			string data = "This is test data.";
			string encrypted = QuickCipher.AsymmetricEncrypt(publicKey, data, settings);
			SecureString decrypted = QuickCipher.AsymmetricDecrypt(privateKey, encrypted, settings);
			Title("Encrypted");
			Console.WriteLine($@"data:
'{data}'

encrypted:
'{encrypted}'

decrypted:
'{decrypted.UnSecure()}'");
		}

		private static void TestSingletonAppGuard()
		{
			SingletonAppGuard guard = null;

			try
			{
				guard = new SingletonAppGuard(1000);
				Console.WriteLine("Heellloooo.!");
				Console.WriteLine("Sleeping for a while...");
				Thread.Sleep(5000);
			}
			catch (TimeoutException)
			{
				Console.WriteLine("Can't run! Another instance is running...");
			}
			finally
			{
				ObjectHelper.Dispose(ref guard);
			}
		}

		private static void TestImpersonationHelper()
		{
			const string SERVICE_NAME = "BITS";

			Title("Testing ImpersonationHelper...");

			bool elevated = WindowsIdentityHelper.HasElevatedPrivileges();
			Console.WriteLine($"Current process is running with elevated privileges? {elevated.ToYesNo()}");

			Console.WriteLine($"Checking {SERVICE_NAME} service status...");
			ServiceController controller = null;

			try
			{
				controller = ServiceControllerHelper.GetController(SERVICE_NAME);
				Console.WriteLine($"Is running? {controller.IsRunning().ToYesNo()}");
			}
			finally
			{
				ObjectHelper.Dispose(ref controller);
			}
		}

		private static void TestServiceHelper()
		{
			const string SERVICE_NAME = "MSMQ";

			TimeSpan timeout = TimeSpanHelper.ThirtySeconds;
			Action<ServiceController> startService = sc =>
			{
				sc.Start();
				sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
			};

			Action<ServiceController> stopService = sc =>
			{
				sc.Stop();
				sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			};

			Title($"Testing ServiceControllerHelper with service {SERVICE_NAME}...");

			ServiceController controller = null;
			WindowsIdentity identity = null;

			try
			{
				controller = ServiceControllerHelper.GetController(SERVICE_NAME);
				Console.WriteLine($"Is running? {controller.IsRunning().ToYesNo()}");
				Console.WriteLine("Asserting access rights...");

				identity = WindowsIdentity.GetCurrent();

				bool canControl = identity.User != null && controller.AssertControlAccessRights(identity.User);
				Console.WriteLine($"Can control service? {canControl.ToYesNo()}");

				if (canControl)
				{
					if (controller.IsRunning())
					{
						Console.WriteLine("Stopping the service...");
						controller.InvokeWithElevatedPrivilege(stopService);
						Console.WriteLine("Sleeping for a while...");
						Thread.Sleep(3000);
					}

					if (controller.IsStopped())
					{
						Console.WriteLine("Starting the service...");
						controller.InvokeWithElevatedPrivilege(startService);
					}
				}
			}
			catch (InvalidOperationException iox) when (iox.InnerException != null)
			{
				Console.WriteLine("I'm not running with elevated privilege! Would you like to restart me as an admin? [Y / Any key]");
				if (Console.ReadKey().Key != ConsoleKey.Y) return;

				AppInfo appInfo = new AppInfo(typeof(Program).Assembly);
				ProcessHelper.ShellExec(appInfo.ExecutablePath, new ShellSettings
				{
					WorkingDirectory = appInfo.Directory,
					Verb = "runas"
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(Red(ex.CollectMessages()));
			}
			finally
			{
				ObjectHelper.Dispose(ref controller);
				ObjectHelper.Dispose(ref identity);
			}
		}

		private static void TestUriHelper()
		{
			const string URI_TEST = "http://example.com/folder path";
			
			string[] uriParts =
			{
				"/another folder",
				"more_folders/folder 2",
				"image file.jpg"
			};

			Uri baseUri = UriHelper.ToUri(URI_TEST, UriKind.Absolute);
			Console.WriteLine($"{URI_TEST} => {baseUri.String()}");

			Uri uri = new Uri(baseUri.ToString());

			foreach (string part in uriParts)
			{
				Uri newUri = UriHelper.Combine(uri, part);
				Console.WriteLine($"{uri.String()} + {part} => {newUri.String()}");
				uri = newUri;
			}

			uri = UriHelper.ToUri(uriParts[0]);
			Console.WriteLine($"{uriParts[0]} => {uri.String()}");

			uri = UriHelper.ToUri(uriParts[1]);
			Console.WriteLine($"{uriParts[1]} => {uri.String()}");

			string[] urls = {
				"server:8088",
				"server:8088/func1",
				"server:8088/func1/SubFunc1",
				"server:8088/my folder/my image.jpg",
				"http://server",
				"http://server/func1",
				"http://server/func/SubFunc1",
				"http://server:8088",
				"http://server:8088/func1",
				"http://server:8088/func1/SubFunc1",
				"magnet://server",
				"magnet://server/func1",
				"magnet://server/func/SubFunc1",
				"magnet://server:8088",
				"magnet://server:8088/func1",
				"magnet://server:8088/func1/SubFunc1",
				"http://[2001:db8::1]",
				"http://[2001:db8::1]:80",
			};

			foreach (string item in urls)
			{
				uri = UriHelper.ToUri(item);
				Console.WriteLine(uri.String());
			}
		}

		private static void TestUriHelperRelativeUrl()
		{
			const string BASE_URI = "/files/images/users";
			Uri relUri = UriHelper.Combine(BASE_URI, Guid.NewGuid().ToString(), "auto_f_92.jpg");
			Console.WriteLine(relUri.String());
		}

		private static void TestJsonUriConverter()
		{
			string[] urls = {
				"server:8088",
				"server:8088/func1",
				"server:8088/func1/SubFunc1",
				"server:8088/my folder/my image.jpg",
				"http://server",
				"http://server/func1",
				"http://server/func/SubFunc1",
				"http://server:8088",
				"http://server:8088/func1",
				"http://server:8088/func1/SubFunc1",
				"magnet://server",
				"magnet://server/func1",
				"magnet://server/func/SubFunc1",
				"magnet://server:8088",
				"magnet://server:8088/func1",
				"magnet://server:8088/func1/SubFunc1",
				"http://[2001:db8::1]",
				"http://[2001:db8::1]:80",
			};

			JsonSerializerSettings settings = JsonHelper.CreateSettings().AddConverters();
			UriTestClass uriTest = new UriTestClass();

			foreach (string item in urls)
			{
				uriTest.Uri = UriHelper.ToUri(item);
				Console.WriteLine($"{item} => {JsonConvert.SerializeObject(uriTest, settings)}");
			}
		}

		private static void TestDevicesMonitor()
		{
			TestUSBForm form = null;
			Title("Testing devices monitor");

			try
			{
				form = new TestUSBForm();
				form.ShowDialog();
			}
			catch (Exception ex)
			{
				Console.WriteLine(Bright.Red(ex.Message));
			}
			finally
			{
				ObjectHelper.Dispose(ref form);
			}
		}

		private static void TestAppInfo()
		{
			Title("Testing AppInfo");

			AppInfo appInfo = new AppInfo(typeof(Program).Assembly);
			Console.WriteLine($"Guid: {appInfo.AppGuid}");
			Console.WriteLine($"ExecutablePath: {appInfo.ExecutablePath}");
			Console.WriteLine($"ExecutableName: {appInfo.ExecutableName}");
			Console.WriteLine($"Directory: {appInfo.Directory}");
			Console.WriteLine($"Title: {appInfo.Title}");
			Console.WriteLine($"Description: {appInfo.Description}");
			Console.WriteLine($"ProductName: {appInfo.ProductName}");
			Console.WriteLine($"Company: {appInfo.Company}");
			Console.WriteLine($"Copyright: {appInfo.Copyright}");
			Console.WriteLine($"Trademark: {appInfo.Trademark}");
			Console.WriteLine($"Version: {appInfo.Version}");
			Console.WriteLine($"FileVersion: {appInfo.FileVersion}");
			Console.WriteLine($"Culture: {appInfo.Culture}");
		}

		private static void TestObservableCollections()
		{
			bool more;
			Title("Testing observable collections");
			
			ObservableList<int> list = new ObservableList<int>(); 
			list.PropertyChanged += onPropertyChanged;
			list.CollectionChanged += onCollectionChanged;

			ObservableHashSet<int> set = new ObservableHashSet<int>();
			set.PropertyChanged += onPropertyChanged;
			set.CollectionChanged += onCollectionChanged;

			ObservableSortedSet<int> sortedSet = new ObservableSortedSet<int>();
			sortedSet.PropertyChanged += onPropertyChanged;
			sortedSet.CollectionChanged += onCollectionChanged;

			ObservableDictionary<int, char> dictionary = new ObservableDictionary<int, char>();
			dictionary.PropertyChanged += onPropertyChanged;
			dictionary.CollectionChanged += onCollectionChanged;

			ObservableKeyedDictionary<int, Student> keyedDictionary = new ObservableKeyedDictionary<int, Student>(e => e.Id);
			keyedDictionary.PropertyChanged += onPropertyChanged;
			keyedDictionary.CollectionChanged += onCollectionChanged;

			int[] values = GetRandomIntegers(30);
			char[] chars = GetRandomChar(values.Length);
			Student[] students = GetRandomStudents(values.Length);

			do
			{
				DoTheTest(list, values);
				ConsoleHelper.Pause();

				DoTheTest(set, values);
				ConsoleHelper.Pause();

				DoTheTest(sortedSet, values);
				ConsoleHelper.Pause();

				DoTheTestWithValue(dictionary, values, chars);
				ConsoleHelper.Pause();

				DoTheTest(keyedDictionary, students);

				Console.WriteLine();
				Console.Write($"Press {Bright.Green("[Y]")} to make another test or {Dim("any other key")} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				Console.WriteLine();
				more = response.Key == ConsoleKey.Y;
			}
			while (more);

			static void DoTheTest<T>(ICollection<T> collection, T[] values)
			{
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {collection.GetType().Name}..."));
				collection.Clear();
				Debug.Assert(collection.Count == 0, "Values are not cleared correctly!");

				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				Console.WriteLine();
				Console.WriteLine($"Array: {string.Join(", ", values)}");

				foreach (T v in values)
				{
					collection.Add(v);
				}

				Console.WriteLine($"Added {collection.Count} of {values.Length} items.");
				Console.WriteLine(Bright.Red("Test removing..."));
				
				int removed = 0;
				int missed = 0;

				foreach (T v in values)
				{
					Console.WriteLine($"Removing {v}:");

					if (collection.Remove(v))
					{
						removed++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Remove missed a value: {v} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
					Console.WriteLine("Does it contain the value? " + collection.Contains(v).ToYesNo());
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Removed {removed} of {values.Length} items.");
			}

			static void DoTheTestWithValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey[] keys, TValue[] values)
			{
				Console.WriteLine();
				Console.WriteLine(Bright.Green($"Testing {dictionary.GetType().Name}..."));
				dictionary.Clear();
				Debug.Assert(dictionary.Count == 0, "Values are not cleared correctly!");

				Console.WriteLine($"Original values: {Bright.Yellow(values.Length.ToString())}...");
				Console.WriteLine();
				Console.WriteLine($"Keys: {string.Join(", ", keys)}");
				Console.WriteLine($"Values: {string.Join(", ", values)}");

				for (int i = 0; i < values.Length; i++)
				{
					dictionary.Add(keys[i], values[i]);
				}

				Console.WriteLine($"Added {dictionary.Count} of {values.Length} items.");
				Console.WriteLine(Bright.Red("Test removing..."));
				
				int removed = 0;
				int missed = 0;

				foreach (TKey key in keys)
				{
					Console.WriteLine($"Removing {key}:");

					if (dictionary.Remove(key))
					{
						removed++;
						continue;
					}

					missed++;
					Console.WriteLine(missed <= 3
										? Bright.Red($"Remove missed a value: {key} :((")
										: Bright.Red("REMOVE MISSED A LOT. :(("));
					Console.WriteLine("Does it contain the value? " + dictionary.ContainsKey(key).ToYesNo());
					if (missed > 3) return;
					//return;
				}

				Console.WriteLine($"Removed {removed} of {values.Length} items.");
			}

			static void onPropertyChanged(object sender, PropertyChangedEventArgs args)
			{
				Console.WriteLine($"{Bright.Cyan("Property")}[{Bright.Yellow(args.PropertyName)}]");
			}

			static void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				string item = e.OldItems is { Count: > 0 }
								? Convert.ToString(e.OldItems[0])
								: e.NewItems is { Count: > 0 }
									? Convert.ToString(e.NewItems[0])
									: "...";
				Console.WriteLine($"{item} => Collection{Bright.Cyan(e.Action.ToString())}");
			}
		}

		private static void TestEnumerateDirectoriesAndFiles()
		{
			bool more;
			string path = Directory.GetCurrentDirectory();

			do
			{
				Console.Clear();

				Title($"Testing directory enumeration [{path}].");
				Console.WriteLine("1. Change directory");
				Console.WriteLine("2. File system items");
				Console.WriteLine("3. Directories");
				Console.WriteLine("4. Files");
				Console.WriteLine("Any character to exit");
				
				char c = Console.ReadKey().KeyChar;
				Console.WriteLine();

				switch (c)
				{
					case '1':
						string newPath;

						do
						{
							Console.Clear();
							Console.WriteLine("Enter new directory [ENTER to skip]: ");
							newPath = Console.ReadLine();
						}
						while (!string.IsNullOrEmpty(newPath) && !Directory.Exists(newPath));

						path = newPath.ToNullIfEmpty() ?? Directory.GetCurrentDirectory();
						more = true;
						break;
					case '2':
						EnumFileSystem(path);
						more = true;
						ConsoleHelper.Pause();
						break;
					case '3':
						EnumDirectories(path);
						more = true;
						ConsoleHelper.Pause();
						break;
					case '4':
						EnumFiles(path);
						more = true;
						ConsoleHelper.Pause();
						break;
					default:
						Console.WriteLine("Exiting...");
						more = false;
						break;
				}
			}
			while (more);

			static void EnumFileSystem(string path)
			{
				Console.WriteLine("Enter a pattern to be used. i.e. *.txt or multi-pattern like *.txt|*.log. [ENTER to use *.*]");
				string pattern = Console.ReadLine();
				Console.WriteLine($"Apply this pattern to directories as well? {Bright.Green("[Y]")} or {Dim("any other key")} to apply it to files only. ");
				bool applyToDirectories = Console.ReadKey().Key == ConsoleKey.Y;

				IEnumerable<string> enumerate = DirectoryHelper.Enumerate(path, pattern, applyToDirectories, SearchOption.AllDirectories);
				Console.WriteLine();
				Console.WriteLine($"File system listing of {path}");
				Console.WriteLine();
				IEnumerator<string> enumerator = null;

				try
				{
					enumerator = enumerate.GetEnumerator();

					if (!enumerator.MoveNext())
					{
						Console.WriteLine("No contents found");
						return;
					}

					do
					{
						Console.WriteLine(enumerator.Current);
					}
					while (enumerator.MoveNext());
				}
				catch (Exception ex)
				{
					Console.WriteLine(Bright.Red(ex.CollectMessages()));
				}
				finally
				{
					ObjectHelper.Dispose(ref enumerator);
				}
			}

			static void EnumDirectories(string path)
			{
				Console.WriteLine("Enter a pattern to be used. i.e. de*g or multi-pattern like de*|ob*|bin. [ENTER to use *.*]");
				string pattern = Console.ReadLine();
				IEnumerable<string> enumerate = DirectoryHelper.EnumerateDirectories(path, pattern, SearchOption.AllDirectories);
				Console.WriteLine();
				Console.WriteLine($"Directories listing of {path}");
				Console.WriteLine();
				IEnumerator<string> enumerator = null;

				try
				{
					enumerator = enumerate.GetEnumerator();

					if (!enumerator.MoveNext())
					{
						Console.WriteLine("No contents found");
						return;
					}

					do
					{
						Console.WriteLine(enumerator.Current);
					}
					while (enumerator.MoveNext());
				}
				catch (Exception ex)
				{
					Console.WriteLine(Bright.Red(ex.CollectMessages()));
				}
				finally
				{
					ObjectHelper.Dispose(ref enumerator);
				}
			}

			static void EnumFiles(string path)
			{
				Console.WriteLine("Enter a pattern to be used. i.e. *.txt or multi-pattern like *.txt|*.log. [ENTER to use *.*]");
				string pattern = Console.ReadLine();
				IEnumerable<string> enumerate = DirectoryHelper.EnumerateFiles(path, pattern, SearchOption.AllDirectories);
				Console.WriteLine();
				Console.WriteLine($"Files listing of {path}");
				Console.WriteLine();
				IEnumerator<string> enumerator = null;

				try
				{
					enumerator = enumerate.GetEnumerator();

					if (!enumerator.MoveNext())
					{
						Console.WriteLine("No contents found");
						return;
					}

					do
					{
						Console.WriteLine(enumerator.Current);
					}
					while (enumerator.MoveNext());
				}
				catch (Exception ex)
				{
					Console.WriteLine(Bright.Red(ex.CollectMessages()));
				}
				finally
				{
					ObjectHelper.Dispose(ref enumerator);
				}
			}
		}

		private static void TestEventWaitHandle()
		{
			bool more;

			do
			{
				Console.Clear();

				Title("Testing EventWaitHandle asynchronously.");
				
				ConsoleKey eventTypeKey = ConsoleKey.Clear;

				while (eventTypeKey != ConsoleKey.A && eventTypeKey != ConsoleKey.M && eventTypeKey != ConsoleKey.Escape)
				{
					Console.Write($"Would you like to use {Bright.Green("[A]")}uto or {Bright.Green("[M]")}anual reset event? ");
					eventTypeKey = Console.ReadKey().Key;
					Console.WriteLine();
				}

				if (eventTypeKey == ConsoleKey.Escape) break;
				Console.Write($"Would you like to use timeout for the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to skip timeout. ");
				int timeout = Console.ReadKey(true).Key == ConsoleKey.Y
								? 30000
								: 0;
				Console.WriteLine();

				string timeoutString = timeout > 0
											? $"{(timeout / 1000.0d):##.##} second(s)"
											: "None";
				CancellationTokenSource cts = null;
				EventWaitHandleBase waitHandle = null;
				TimedCallback timedCallback = null;

				try
				{
					cts = timeout > 0
							? new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout))
							: null;
					waitHandle = eventTypeKey == ConsoleKey.A
									? new AutoResetEvent()
									: new ManualResetEvent();
					// copy to local variable
					CancellationToken token = cts?.Token ?? CancellationToken.None;
					Console.WriteLine($"Using timeout: {Bright.Cyan(timeoutString)}...");
					
					// copy to local variable
					EventWaitHandleBase handle = waitHandle;

					if (timeout > 0)
					{
						Console.WriteLine($"The automatic setup will signal the event in: {Bright.Cyan((timeout / 2 / 1000).ToString())} seconds...");
						timedCallback = TimedCallback.Create(tcb =>
						{
							tcb.Enabled = false;
							handle.Set();
						}, timeout / 2);
						waitHandle.WaitOne(timeout, token);
					}
					else
					{
						Task.Run(() => handle.WaitOne(token), token);
						Console.Write("Press any key to signal the event. ");
						Console.ReadKey(true);
						Console.WriteLine();
						waitHandle.Set();
						// for the stupid AutoReset
						waitHandle.Set();
					}

					bool isSet = waitHandle.WaitOne(0);
					Console.WriteLine(isSet
										? Bright.Green("Event signaled.")
										: Bright.Red("Some weird shit is going on there!"));
				}
				catch (Exception e)
				{
					Console.WriteLine(e.CollectMessages());
				}
				finally
				{
					ObjectHelper.Dispose(ref timedCallback);
					ObjectHelper.Dispose(ref waitHandle);
					ObjectHelper.Dispose(ref cts);
				}

				Console.WriteLine();
				Console.Write($"Would you like to repeat the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to exit. ");
				more = Console.ReadKey(true).Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void TestWaitForEvent()
		{
			const int TIMEOUT = 3000;

			bool more;
			Student student = new Student
			{
				Id = 1,
				Name = __fakeGenerator.Value.Name.FirstName(__fakeGenerator.Value.PickRandom<Name.Gender>()),
				Grade = __fakeGenerator.Value.Random.Double(0.0d, 100.0d)
			};
			WaitForEventSettings<Student> settings = WaitForEventSettings.Create(student, nameof(Student.Happened));

			do
			{
				Console.Clear();

				Title("Testing WaitForEvent asynchronously.");

				Console.Write($"Would you like to use timeout for the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to skip timeout. ");
				int timeout = Console.ReadKey(true).Key == ConsoleKey.Y
								? RNGRandomHelper.Next((int)(TIMEOUT / 1.2), (int)(TIMEOUT * 1.2))
								: TimeSpanHelper.INFINITE;
				Console.WriteLine();

				string timeoutString = timeout > 0
											? $"{(timeout / 1000.0d):##.##} second(s)"
											: "None";
				Console.WriteLine($"Event will be fired in: {Bright.Cyan((TIMEOUT / 1000).ToString())} seconds...");
				Console.WriteLine($"Using timeout: {Bright.Cyan(timeoutString)}...");

				settings.Timeout = TimeSpan.FromMilliseconds(timeout);
				student.WillHappenIn(TIMEOUT);
				Console.WriteLine(EventHelper.WatchEventAsync(settings).ConfigureAwait().Execute()
									? Bright.Green("Ok")
									: timeout > 0
										? Bright.Red("Didn't occur in time!")
										: Bright.Red("Something shitty is going on!"));

				Console.WriteLine();
				Console.Write($"Would you like to repeat the tests? {Bright.Green("[Y]")} or {Dim("any other key")} to exit. ");
				more = Console.ReadKey(true).Key == ConsoleKey.Y;
			}
			while (more);
		}

		private static void Title(string title)
		{
			Console.WriteLine();
			Console.WriteLine(Bold().Bright.Black(title));
			Console.WriteLine();
		}

		private static void CompilationHint()
		{
			Console.WriteLine(__compilationText);
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
					Id = i + 1,
					Name = __fakeGenerator.Value.Name.FirstName(__fakeGenerator.Value.PickRandom<Name.Gender>()),
					Grade = __fakeGenerator.Value.Random.Double(0.0d, 100.0d)
				};
			}

			return students;
		}

		private static bool LimitThreads()
		{
			// change this to true to use 1 thread only for debugging
			return false;
		}
	}
}
