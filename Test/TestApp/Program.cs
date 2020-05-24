using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using asm.Collections;
using asm.Exceptions;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Layout;
using Bogus;
using Crayon;
using JetBrains.Annotations;

namespace TestApp
{
	internal class Program
	{
		private static readonly Lazy<Faker> __stringGenerator = new Lazy<Faker>(() => new Faker(), LazyThreadSafetyMode.PublicationOnly);

		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;

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

			TestAllBinaryTrees();

			ConsoleHelper.Pause();
		}

		private static void TestSortAlgorithms()
		{
			const int TRIES = 100;
			const int RESULT_COUNT = 5;

			TestTitle("Testing Sort Algorithms...");

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
					Console.WriteLine($" => {numericResults[algorithm].ToString("F6").BrightGreen()}");
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
			TestTitle("Testing LinkedQueue...");
			
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
			TestTitle("Testing MinMaxQueue...");

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

			TestTitle("Testing BinaryTree from traversal values...");

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
			TestTitle("Testing BinarySearchTree.Add()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				BinarySearchTree<int> tree = new BinarySearchTree<int>();

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
			TestTitle("Testing BinarySearchTree.Remove()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				BinarySearchTree<int> tree = new BinarySearchTree<int>();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.Print();
				}

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
			TestTitle("Testing BinarySearchTree.Balance()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();
				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				BinarySearchTree<int> tree = new BinarySearchTree<int>();

				foreach (int v in values)
				{
					tree.Add(v);
					//tree.Print();
				}

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
			TestTitle("Testing AVLTree.Add()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				AVLTree<int> tree = new AVLTree<int>();

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
			TestTitle("Testing AVLTree.Remove()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				AVLTree<int> tree = new AVLTree<int>(values);
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
			TestTitle("Testing RedBlackTree.Add()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				RedBlackTree<int> tree = new RedBlackTree<int>();

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
			TestTitle("Testing RedBlackTree.Remove()...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				Console.WriteLine("Test adding...".BrightGreen());
				RedBlackTree<int> tree = new RedBlackTree<int>(values);
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
			TestTitle("Testing all BinaryTrees...");

			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: ".BrightBlack() + string.Join(", ", values));

				LinkedBinaryTree<int> tree = new BinarySearchTree<int>();
				DoTheTest(tree, values);

				tree = new AVLTree<int>();
				DoTheTest(tree, values);

				RedBlackTree<int> rbTree = new RedBlackTree<int>();
				DoTheTest(rbTree, values);

				Console.WriteLine();
				Console.Write($"Press {"[Y]".BrightGreen()} to make another test or {"any other key".Dim()} to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);

			static void DoTheTest<TNode>(LinkedBinaryTree<TNode, int> tree, int[] array)
				where TNode : LinkedBinaryNode<TNode, int>
			{
				Console.WriteLine($"Testing {tree.GetType().Name}...".BrightGreen());
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

		private static void TestTitle(string title)
		{
			Console.WriteLine();
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
		private static string[] GetRandomStrings(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 12);
			return __stringGenerator.Value.Lorem.Words(len);
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
		string treeString = thisValue.ToString(orientation, diagnosticInfo);
		Console.WriteLine();
		Console.WriteLine(treeString);
	}
}
