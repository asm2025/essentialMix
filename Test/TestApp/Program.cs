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
using asm.Patterns.Collections;
using asm.Patterns.Layout;
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

			//TestSortAlgorithm();
			//TestSortAlgorithms();

			//TestLinkedQueue();
			//TestMinMaxQueue();

			//TestBinaryTreeFromTraversal();
			
			//TestBinarySearchTreeAdd();
			//TestBinarySearchTreeRemove();
			//TestBinarySearchTreeBalance();
			
			//TestAVLTreeAdd();
			//TestAVLTreeRemove();
			
			//TestRedBlackTreeAdd();
			//TestRedBlackTreeRemove();

			//TestAllBinaryTrees();

			//TestTreeEquality();
			
			//TestHeapAdd();
			//TestHeapRemove();
			
			//TestPriorityQueue();

			//TestHeapElementAt();

			//TestThreadQueue();

			//TestTrie();
			//TestTrieSimilarWordsRemoval();

			TestGraphAdd();
			
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
				string[] strings = GetRandomStrings(RNGRandomHelper.Next(3, 10));
				Console.WriteLine("Numbers: ".BrightCyan() + string.Join(", ", numbers));
				Console.WriteLine("String: ".BrightCyan() + string.Join(", ", strings.Select(e => e.SingleQuote())));

				Console.Write("Numbers");
				watch.Restart();
				sortNumbers(numbers, 0, -1, numbersComparer, false);
				double numericResults = watch.Elapsed.TotalMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {numericResults.ToString("F6").BrightGreen()}");
				Console.WriteLine("Result: " + string.Join(", ", numbers));
				Console.WriteLine();

				Console.Write("Strings");
				watch.Restart();
				sortStrings(strings, 0, -1, stringComparer, false);
				double stringResults = watch.Elapsed.TotalMilliseconds;
				watch.Stop();
				Console.WriteLine($" => {stringResults.ToString("F6").BrightGreen()}");
				Console.WriteLine("Result: " + string.Join(", ", strings.Select(e => e.SingleQuote())));
				Console.WriteLine();

				Console.WriteLine("Finished".BrightYellow());
				Console.WriteLine();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
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
			double[] time = new double[TRIES];
			string sectionSeparator = new string('*', 80).BrightMagenta();
			bool more;

			do
			{
				Console.Clear();
				int[] numbers = GetRandomIntegers(RNGRandomHelper.Next(5, 20));
				string[] strings = GetRandomStrings(RNGRandomHelper.Next(3, 10));
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
						time[i] = watch.Elapsed.TotalMilliseconds;
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
						time[i] = watch.Elapsed.TotalMilliseconds;
						watch.Stop();
						if (i < time.Length - 1) ints = (int[])numbers.Clone();
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
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
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

		private static void TestBinaryTreeFromTraversal()
		{
			const string TREE_DATA_LEVEL = "FCIADGJBEHK";
			const string TREE_DATA_PRE = "FCABDEIGHJK";
			const string TREE_DATA_IN = "ABCDEFGHIJK";
			const string TREE_DATA_POST = "BAEDCHGKJIF";
			const int NUM_TESTS = 7;

			Title("Testing BinaryTree from traversal values...");

			BinarySearchTree<char> tree = new BinarySearchTree<char>();

			bool more;
			int i = 0;

			do
			{
				Console.Clear();
				Console.WriteLine();

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

				tree.Print();
				tree.Print(Orientation.Horizontal);
				i++;

				if (i >= NUM_TESTS)
				{
					more = false;
					continue;
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to move to next test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestBinarySearchTreeAdd()
		{
			Title("Testing BinarySearchTree.Add()...");

			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.Print();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestBinarySearchTreeRemove()
		{
			Title("Testing BinarySearchTree.Remove()...");

			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine("Test removing...".BrightRed());
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.Print();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestBinarySearchTreeBalance()
		{
			Title("Testing BinarySearchTree.Balance()...");

			bool more;
			BinarySearchTree<int> tree = new BinarySearchTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine("Test removing...".BrightRed());
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.Print();

				if (!tree.IsBalanced())
				{
					Console.WriteLine("Test balancing...".BrightGreen());
					tree.Balance();
					tree.Print();
				}

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestAVLTreeAdd()
		{
			Title("Testing AVLTree.Add()...");

			bool more;
			AVLTree<int> tree = new AVLTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.Print();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestAVLTreeRemove()
		{
			Title("Testing AVLTree.Remove()...");

			bool more;
			AVLTree<int> tree = new AVLTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine("Test removing...".BrightRed());
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.Print();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestRedBlackTreeAdd()
		{
			Title("Testing RedBlackTree.Add()...");

			bool more;
			RedBlackTree<int> tree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.Print();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestRedBlackTreeRemove()
		{
			Title("Testing RedBlackTree.Remove()...");

			bool more;
			RedBlackTree<int> tree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				tree.Clear();
				tree.Add(values);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", tree));
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine("Test removing...".BrightRed());
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.Print();

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestAllBinaryTrees()
		{
			Title("Testing all BinaryTrees...");

			bool more;
			BinarySearchTree<int> binarySearchTree = new BinarySearchTree<int>();
			AVLTree<int> avlTree = new AVLTree<int>();
			RedBlackTree<int> redBlackTree = new RedBlackTree<int>();

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				DoTheTest(binarySearchTree, values);

				DoTheTest(avlTree, values);

				DoTheTest(redBlackTree, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
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
				tree.Print();
				tree.Print(Orientation.Horizontal);

				Console.WriteLine("Test removing...".BrightRed());
				int value = array.PickRandom();
				Console.WriteLine($"will remove {value.ToString().BrightCyan().Underline()}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!".BrightRed());
					return;
				}

				tree.Print();
				if (tree.AutoBalance || tree.IsBalanced()) return;

				Console.WriteLine("Test balancing...".BrightGreen());
				tree.Balance();
				tree.Print();
			}
		}

		private static void TestTreeEquality()
		{
			Title("Testing tree equality...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
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
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
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
				tree1.Print();
				tree2.Print();
				Console.WriteLine($"tree1 == tree2? {tree1.Equals(tree2).ToYesNo()}");
			}
		}

		private static void TestHeapAdd()
		{
			Title("Testing Heap.Add()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
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
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());

				foreach (T value in array)
				{
					heap.Add(value);
					//heap.Print();
				}

				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				heap.Print(Orientation.Horizontal);
			}
		}

		private static void TestHeapRemove()
		{
			Title("Testing Heap.Remove()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
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
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				heap.Print(Orientation.Horizontal);
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (heap.Remove(out T value))
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(value);
				}

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void TestPriorityQueue()
		{
			Title("Testing PriorityQueue...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				PriorityQueue<int> intQueue = new PriorityQueue<int>();
				DoTheTest(intQueue, values);

				Student[] students = GetRandomStudents(len);
				PriorityQueue<double, Student> studentQueue = new PriorityQueue<double, Student>(e => e.Grade * -1);
				DoTheTest(studentQueue, students);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);

			static void DoTheTest<TPriority, T>(PriorityQueue<TPriority, T> queue, T[] array)
				where TPriority : struct, IComparable
			{
				Console.WriteLine($"Test adding ({queue.GetType()})...".BrightGreen());
				queue.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", queue));
				queue.Print();
				queue.Print(Orientation.Horizontal);
				Console.WriteLine("Test removing...");
				bool removeStarted = false;

				while (queue.Remove(out T value))
				{
					if (!removeStarted) removeStarted = true;
					else Console.Write(", ");

					Console.Write(value);
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
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				int k = RNGRandomHelper.Next(1, values.Length);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Heap<int> heap = new MaxHeap<int>();
				DoTheTest(heap, values, k);

				heap = new MinHeap<int>();
				DoTheTest(heap, values, k);

				Student[] students = GetRandomStudents(len);
				PriorityQueue<double, Student> studentQueue = new PriorityQueue<double, Student>(e => e.Grade * -1);
				DoTheTest2(studentQueue, students, k);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);

			static void DoTheTest<T>(Heap<T> heap, T[] array, int k)
			{
				Console.WriteLine($"Test adding ({heap.GetType()})...".BrightGreen());
				heap.Add(array);
				Console.WriteLine("InOrder: ".BrightBlack() + string.Join(", ", heap));
				heap.Print();
				heap.Print(Orientation.Horizontal);
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
				queue.Print(Orientation.Horizontal);
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

			int len = RNGRandomHelper.Next(1000, 5000);
			int[] values = GetRandomIntegers(len);
			int timeout = RNGRandomHelper.Next(0, 1);
			int maximumThreads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);
			Func<int, TaskResult> exec = e =>
			{
				Console.Write(", {0}", e);
				return TaskResult.Success;
			};
			Queue<ThreadQueueMode> modes = new Queue<ThreadQueueMode>(EnumHelper<ThreadQueueMode>.GetValues());

			while (modes.Count > 0)
			{
				Console.Clear();
				Console.WriteLine();
				ThreadQueueMode mode = modes.Dequeue();
				Title($"Testing Thread queue [{mode.ToString().BrightCyan().Underline()}]...");

				// if there is a timeout, will use a CancellationTokenSource.
				using (CancellationTokenSource cts = timeout > 0
														? new CancellationTokenSource(TimeSpan.FromMinutes(timeout))
														: null)
				{
					CancellationToken token = cts?.Token ?? CancellationToken.None;

#if DEBUG
					// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
					int threads = LIMIT_THREADS
									? 1
									: maximumThreads;
#else
					// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
					int threads = MaximumThreads;
#endif

					if (threads < 1 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;

					ProducerConsumerQueueOptions<int> options = mode == ThreadQueueMode.ThresholdTaskGroup
																	? new ProducerConsumerThresholdQueueOptions<int>(threads, exec)
																	{
																		// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
																		Threshold = TimeSpan.FromSeconds(1)
																	}
																	: new ProducerConsumerQueueOptions<int>(threads, exec);
					Console.WriteLine($"{len.ToString().BrightYellow()} values, {options.Threads.ToString().BrightYellow()} threads, {timeout.ToString().BrightYellow()} minute(s) timeout");
					Console.WriteLine();
			
					// Create a generic queue producer
					using (IProducerConsumer<int> queue = ProducerConsumerQueue.Create(mode, options, token))
					{
						queue.WorkStarted += (sender, args) =>
						{
							Console.WriteLine();
							Console.WriteLine($"Starting {mode}...".BrightGreen());
						};

						queue.WorkCompleted += (sender, args) =>
						{
							Console.WriteLine();
							Console.WriteLine("Finished.".BrightRed());
							Console.WriteLine();
						};

						foreach (int value in values)
						{
							queue.Enqueue(value);
						}

						// call this to allow other threads to kick in
						Thread.Sleep(0);

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
				if (response.KeyChar != 'Y' && response.KeyChar != 'y') modes.Clear();
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
				Console.WriteLine();
				if (values.Count == 0) AddWords(trie, values);
				Console.WriteLine("Words list: ".BrightBlack() + string.Join(", ", values));

				string word = values.PickRandom();
				DoTheTest(trie, word, values);
				
				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
				if (!more || values.Count >= MAX_LIST) continue;

				Console.WriteLine();
				Console.Write($"Would you like to add more words? {"[Y]".BrightGreen()} / {"any key".Dim()} ");
				response = Console.ReadKey(true);
				if (response.KeyChar != 'Y' && response.KeyChar != 'y') continue;
				Console.WriteLine();
				AddWords(trie, values);
			}
			while (more);

			static void AddWords(Trie<char> trie, ISet<string> set)
			{
				int len = RNGRandomHelper.Next(10, 20);
				Console.WriteLine($"Generating {len} words: ".BrightGreen());
				string[] newValues = GetRandomStrings(len);

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
			{
				trie.Add(value);
			}

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

		private static void TestGraphAdd()
		{
			const int MAX_LIST = 26;

			bool more;
			Graph<GraphNode<char>, GraphEdge<char>, char> graph;
			Graph<GraphNode<char>, GraphWeightedEdge<char>, char> weightedGraph;
			ISet<char> values = new HashSet<char>();
			Menu menu = new Menu()
				.Add("Undirected graph", () =>
				{
					Console.WriteLine();
					graph = new UndirectedGraph<char>();
					if (values.Count == 0) AddChar(values);
					DoTheTest(graph, values);
				})
				.Add("Directed graph", () =>
				{
					Console.WriteLine();
					graph = new DirectedGraph<char>();
					if (values.Count == 0) AddChar(values);
					DoTheTest(graph, values);
				})
				.Add("Weighted undirected graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedUndirectedGraph<char>();
					if (values.Count == 0) AddChar(values);
					DoTheTest(weightedGraph, values);
				})
				.Add("Weighted directed graph", () =>
				{
					Console.WriteLine();
					weightedGraph = new WeightedDirectedGraph<char>();
					if (values.Count == 0) AddChar(values);
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
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
				if (!more || values.Count >= MAX_LIST) continue;

				Console.WriteLine();
				Console.Write($"Would you like to add more character? {"[Y]".BrightGreen()} / {"any key".Dim()} ");
				response = Console.ReadKey(true);
				if (response.KeyChar != 'Y' && response.KeyChar != 'y') continue;
				Console.WriteLine();
				AddChar(values);
			}
			while (more);

			static void AddChar(ISet<char> set)
			{
				int len = RNGRandomHelper.Next(1, 12);
				Console.WriteLine($"Generating {len} characters: ".BrightGreen());
				char[] newValues = GetRandomChar(len);
				int count = 0;

				foreach (char value in newValues)
				{
					if (!set.Add(value)) continue;
					count++;
				}

				Console.WriteLine($"Added {count} characters to the set".BrightGreen());
			}

			static void DoTheTest<TEdge>(Graph<GraphNode<char>, TEdge, char> graph, ISet<char> values)
				where TEdge : GraphEdge<GraphNode<char>, TEdge, char>
			{
				Console.WriteLine("Test adding nodes...");
				Console.WriteLine("characters list: ".BrightBlack() + string.Join(", ", values));
				graph.Clear();
				graph.Add(values);

				if (graph.Count != values.Count)
				{
					Console.WriteLine("Something went wrong, not all nodes were added...!".BrightRed());
					return;
				}

				if (graph.Count == 1)
				{
					Console.WriteLine("Huh, must add more nodes...!".BrightRed());
					return;
				}
				
				Console.WriteLine("All nodes are added...!".BrightGreen() + " Let's try adding some relationships...");

				Queue<char> queue = new Queue<char>(values);
				int threshold = (int)Math.Floor(values.Count * 0.5d);
				char from = queue.Dequeue();

				while (queue.Count > 0)
				{
					char to = queue.Dequeue();
					if (graph.Comparer.Equals(from, to) || graph.ContainsEdge(from, to)) continue;
					Console.WriteLine($"Adding {from.ToString().BrightCyan().Underline()} to {to.ToString().BrightCyan().Underline()}...");
					graph.AddEdge(from, to);

					if (threshold > 0 && queue.Count % 2 == 0)
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
				Console.WriteLine($"Picking a value wit maximum relations: '{value.ToString().BrightCyan().Underline()}'...");
				Console.WriteLine("Breadth First: " + string.Join(", ", graph.Enumerate(value, GraphTraverseMethod.BreadthFirst)));
				Console.WriteLine("Depth First: " + string.Join(", ", graph.Enumerate(value, GraphTraverseMethod.DepthFirst)));
			}
		}

		private static void Title(string title)
		{
			Console.WriteLine();
			Console.WriteLine(title.Bold().BrightBlack());
			Console.WriteLine();
		}

		[NotNull]
		private static int[] GetRandomIntegers(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
	
			int[] values = new int[len];

			for (int i = 0; i < len; i++)
			{
				values[i] = RNGRandomHelper.Next(1, short.MaxValue);
			}

			return values;
		}

		[NotNull]
		private static double[] GetRandomDoubles(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);

			double[] values = new double[len];

			for (int i = 0; i < len; i++)
			{
				values[i] = (i + 1) * RNGRandomHelper.NextDouble();
			}
	
			return values;
		}

		[NotNull]
		private static char[] GetRandomChar(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
	
			char[] values = new char[len];

			for (int i = 0; i < len; i++)
			{
				values[i] = (char)RNGRandomHelper.Next('a', 'z');
			}

			return values;
		}

		[NotNull]
		private static string[] GetRandomStrings(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
			return __fakeGenerator.Value.Random.WordsArray(len);
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

	public static void Print<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, bool diagnosticInfo = true)
		where TNode : LinkedBinaryNode<TNode, T>
	{
		Console.WriteLine();
		Console.WriteLine($"{"Dimensions:".Yellow()} {thisValue.Count.ToString().Underline()} x {thisValue.GetHeight().ToString().Underline()}.");
		Console.WriteLine($"{"Balanced:".Yellow()} {thisValue.IsBalanced().ToYesNo()}");
		Console.WriteLine($"{"Valid:".Yellow()} {thisValue.Validate().ToYesNo()}");
		Console.WriteLine($"{"Minimum:".Yellow()} {thisValue.Minimum()} {"Maximum:".Yellow()} {thisValue.Maximum()}");
		thisValue.Print(Orientation.Vertical, diagnosticInfo);
	}

	public static void Print<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, Orientation orientation, bool diagnosticInfo = true)
		where TNode : LinkedBinaryNode<TNode, T>
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out, orientation, diagnosticInfo);
	}

	public static void Print<T>([NotNull] this ArrayBinaryTree<T> thisValue, bool diagnosticInfo = true)
	{
		Console.WriteLine();
		Console.WriteLine($"{"Dimensions:".Yellow()} {thisValue.Count.ToString().Underline()} x {thisValue.GetHeight().ToString().Underline()}.");
		Console.WriteLine($"{"Valid:".Yellow()} {thisValue.Validate().ToYesNo()}");
		Console.WriteLine($"{"Minimum:".Yellow()} {thisValue.Minimum()} {"Maximum:".Yellow()} {thisValue.Maximum()}");
		thisValue.Print(Orientation.Vertical, diagnosticInfo);
	}

	public static void Print<T>([NotNull] this ArrayBinaryTree<T> thisValue, Orientation orientation, bool diagnosticInfo = true)
	{
		Console.WriteLine();
		thisValue.WriteTo(Console.Out, orientation, diagnosticInfo);
	}

	public static void Print<TNode, TEdge, T>([NotNull] this Graph<TNode, TEdge, T> thisValue)
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphEdge<TNode, TEdge, T>
	{
		Console.WriteLine();
		Console.WriteLine($"{"Order:".Yellow()} {thisValue.Count.ToString().Underline()}.");
		Console.WriteLine($"{"Size:".Yellow()} {thisValue.Size.ToString().Underline()}.");
		Console.WriteLine();
		thisValue.WriteTo(Console.Out);
	}
}
