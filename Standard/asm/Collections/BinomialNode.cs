using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value} :C{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<T>
	{
		private const int CHILD = 0;
		private const int SIBLING = 1;

		private readonly BinomialNode<T>[] _nodes = new BinomialNode<T>[2];

		public BinomialNode(T value)
		{
			Value = value;
		}

		public BinomialNode<T> Child
		{
			get => _nodes[CHILD];
			internal set => _nodes[CHILD] = value;
		}

		public BinomialNode<T> Sibling
		{
			get => _nodes[SIBLING];
			internal set => _nodes[SIBLING] = value;
		}

		public T Value { get; set; }

		public int Degree { get; internal set; }

		public bool IsLeaf => Sibling == null && Child == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		[ItemNotNull]
		public IEnumerable<BinomialNode<T>> Siblings()
		{
			BinomialNode<T> next = Sibling;

			while (next != null)
			{
				yield return next;
				next = next.Sibling;
			}
		}

		[ItemNotNull]
		public IEnumerable<BinomialNode<T>> Children()
		{
			BinomialNode<T> next = Child;

			while (next != null)
			{
				yield return next;
				next = next.Child;
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap([NotNull] BinomialNode<T> other)
		{
			T tmp = other.Value;
			other.Value = Value;
			Value = tmp;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		internal void Link([NotNull] BinomialNode<T> other)
		{
			other.Sibling = Child;
			Child = other;
			Degree++;
		}

		public static implicit operator T([NotNull] BinomialNode<T> node) { return node.Value; }
	}
}