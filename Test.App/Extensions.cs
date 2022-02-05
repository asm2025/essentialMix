using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using essentialMix.Collections;
using JetBrains.Annotations;
using static Crayon.Output;

namespace TestApp
{
	public static class Extensions
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToYesNo(this bool thisValue)
		{
			return thisValue
						? Bright.Green("Yes")
						: Bright.Red("No");
		}

		public static void PrintProps<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			Console.WriteLine();
			Console.WriteLine($"{Yellow("Dimensions:")} {Underline(thisValue.Count.ToString())} x {Underline(thisValue.GetHeight().ToString())}.");
			Console.WriteLine($"{Yellow("Balanced:")} {thisValue.IsBalanced().ToYesNo()}");
			Console.WriteLine($"{Yellow("Valid:")} {thisValue.Validate().ToYesNo()}");
			Console.WriteLine($"{Yellow("Minimum:")} {thisValue.Minimum()} {Yellow("Maximum:")} {thisValue.Maximum()}");
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
			Console.WriteLine($"{Yellow("Dimensions:")} {Underline(thisValue.Count.ToString())} x {Underline(thisValue.GetHeight().ToString())}.");
			Console.WriteLine($"{Yellow("Valid:")} {thisValue.Validate().ToYesNo()}");
			Console.WriteLine($"{Yellow("LeftMost:")} {thisValue.LeftMost()} {Yellow("RightMost:")} {thisValue.RightMost()}");
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
			Console.WriteLine($"{Yellow("Order:")} {Underline(thisValue.Count.ToString())}.");
			Console.WriteLine($"{Yellow("Size:")} {Underline(thisValue.GetSize().ToString())}.");
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

		public static void Print<TNode, T>([NotNull] this BinaryHeapBase<TNode, T> thisValue)
			where TNode : BinaryNodeBase<TNode, T>
		{
			Console.WriteLine();
			thisValue.WriteTo(Console.Out);
		}

		public static void Print<TNode, T>([NotNull] this SiblingsHeapBase<TNode, T> thisValue)
			where TNode : class, ISiblingNode<TNode, T>
		{
			Console.WriteLine();
			Console.WriteLine("Count: " + thisValue.Count);
			Console.WriteLine();
			thisValue.WriteTo(Console.Out);
		}
	}
}