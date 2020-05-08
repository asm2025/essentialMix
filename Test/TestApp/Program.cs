using System;
using System.Text;
using asm.Collections;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace TestApp
{
	internal class Program
	{
		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;

			TestBinaryTree();
			//TestBinarySearchTree();
			//TestAVLTree();

			ConsoleHelper.Pause();
		}

		private static void TestBinaryTree()
		{
			const string TREE_DATA_LEVEL = "FCIADGJBEHK";
			const string TREE_DATA_PRE = "FCABDEIGHJK";
			const string TREE_DATA_IN = "ABCDEFGHIJK";
			const string TREE_DATA_POST = "BAEDCHGKJIF";
			const int NUM_TESTS = 7;

			LinkedBinarySearchTree<char> tree = new LinkedBinarySearchTree<char>();
			
			bool more;
			int i = 0;

			do
			{
				Console.Clear();
				Console.WriteLine();

				switch (i)
				{
					case 0:
						Console.WriteLine($@"Data from LevelOrder traversal:
{TREE_DATA_LEVEL}");
						tree.FromLevelOrder(TREE_DATA_LEVEL);
						break;
					case 1:
						Console.WriteLine($@"Data from PreOrder traversal:
{TREE_DATA_PRE}");
						tree.FromPreOrder(TREE_DATA_PRE);
						break;
					case 2:
						Console.WriteLine($@"Data from InOrder traversal:
{TREE_DATA_IN}");
						tree.FromInOrder(TREE_DATA_IN);
						break;
					case 3:
						Console.WriteLine($@"Data from PostOrder traversal:
{TREE_DATA_POST}");
						tree.FromPostOrder(TREE_DATA_POST);
						break;
					case 4:
						Console.WriteLine($@"Data from InOrder and LevelOrder traversals:
{TREE_DATA_IN}
{TREE_DATA_LEVEL}");
						tree.FromInOrderAndLevelOrder(TREE_DATA_IN, TREE_DATA_LEVEL);
						break;
					case 5:
						Console.WriteLine($@"Data from InOrder and PreOrder traversals:
{TREE_DATA_IN}
{TREE_DATA_PRE}");
						tree.FromInOrderAndPreOrder(TREE_DATA_IN, TREE_DATA_PRE);
						break;
					case 6:
						Console.WriteLine($@"Data from InOrder and PostOrder traversals:
{TREE_DATA_IN}
{TREE_DATA_POST}");
						tree.FromInOrderAndPostOrder(TREE_DATA_IN, TREE_DATA_POST);
						break;
				}

				tree.Print();
				i++;

				if (i >= NUM_TESTS)
				{
					more = false;
					continue;
				}

				Console.Write("Press [Y] to move to next test or any other key to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestBinarySearchTree()
		{
			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: " + string.Join(", ", values));

				Console.WriteLine("Test adding...");
				LinkedBinarySearchTree<int> tree = new LinkedBinarySearchTree<int>(values);
				Console.WriteLine("InOrder: " + string.Join(", ", tree));
				tree.Print();

				Console.WriteLine("Test removing...");
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!");
					return;
				}

				tree.Print();

				if (!tree.IsBalanced())
				{
					Console.WriteLine("Test balancing...");
					tree.Balance();
					tree.Print();
				}

				Console.Write("Press [Y] to make another test or any other key to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		private static void TestAVLTree()
		{
			bool more;

			do
			{
				Console.Clear();
				Console.WriteLine();

				int len = RNGRandomHelper.Next(1, 12);
				int[] values = GetRandomIntegers(len);
				Console.WriteLine("Array: " + string.Join(", ", values));

				Console.WriteLine("Test adding...");
				AVLTree<int> tree = new AVLTree<int>(values);
				Console.WriteLine("InOrder: " + string.Join(", ", tree));
				tree.Print();

				Console.WriteLine("Test removing...");
				int value = values.PickRandom();
				Console.WriteLine($"will remove {value}.");

				if (!tree.Remove(value))
				{
					Console.WriteLine("Didn't remove a shit...!");
					return;
				}

				tree.Print();

				Console.Write("Press [Y] to make another test or any other key to exit. ");
				ConsoleKeyInfo response = Console.ReadKey(true);
				more = response.KeyChar == 'Y' || response.KeyChar == 'y';
			}
			while (more);
		}

		[NotNull]
		private static int[] GetRandomIntegers(int len = 0)
		{
			if (len < 1) len = RNGRandomHelper.Next(1, 20);
	
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
			if (len < 1) len = RNGRandomHelper.Next(1, 20);

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
			if (len < 1) len = RNGRandomHelper.Next(1, 20);

			string[] values = new string[len];
			StringBuilder sb = new StringBuilder();
	
			for (int i = 0; i < len; i++)
			{
				sb.Length = 0;
				int n = RNGRandomHelper.Next(1, 20);
		
				for (int j = 0; j < n; j++)
				{
					sb.Append((char)RNGRandomHelper.Next(32, 126));
				}
		
				values[i] = sb.ToString();
			}
	
			return values;
		}
	}
}

public static class BinaryTreeExtension
{
	public static void Print<T>([NotNull] this LinkedBinaryTree<T> thisValue, bool diagnosticInfo = true)
	{
		Console.WriteLine();
		Console.WriteLine($"Dimensions: {thisValue.Count} x {thisValue.Height}");
		Console.WriteLine($"Balanced: {thisValue.IsBalanced()}, factor: {thisValue.BalanceFactor}");
		Console.WriteLine($"Is valid: {thisValue.Validate()}");
		thisValue.Print(Orientation.Vertical, diagnosticInfo);
	}

	public static void Print<T>([NotNull] this LinkedBinaryTree<T> thisValue, Orientation orientation, bool diagnosticInfo = true)
	{
		string treeString = thisValue.ToString(orientation, diagnosticInfo);
		Console.WriteLine();
		Console.WriteLine(treeString);
		Console.WriteLine();
	}
}
