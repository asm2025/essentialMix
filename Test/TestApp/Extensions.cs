using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using asm.Collections;
using Crayon;
using JetBrains.Annotations;

namespace TestApp
{
	public static class Extensions
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
}