using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	// https://www.growingwiththeweb.com/data-structures/fibonacci-heap/overview/
	// https://brilliant.org/wiki/fibonacci-heap/
	[DebuggerDisplay("{Value} :D{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class FibonacciNode<T>
	{
		private const int PARENT = 0;
		private const int CHILD = 1;
		private const int PREVIOUS = 2;
		private const int NEXT = 3;

		private readonly FibonacciNode<T>[] _nodes = new FibonacciNode<T>[4];

		public FibonacciNode(T value)
		{
			Value = value;
			_nodes[PREVIOUS] = this;
			_nodes[NEXT] = this;
		}

		public FibonacciNode<T> Parent
		{
			get => _nodes[PARENT];
			internal set => _nodes[PARENT] = value;
		}

		public FibonacciNode<T> Child
		{
			get => _nodes[CHILD];
			internal set => _nodes[CHILD] = value;
		}

		public FibonacciNode<T> Previous
		{
			get => _nodes[PREVIOUS] == this
						? null
						: _nodes[PREVIOUS];
			internal set => _nodes[PREVIOUS] = value;
		}

		public FibonacciNode<T> Next
		{
			get => _nodes[NEXT] == this
						? null
						: _nodes[NEXT];
			internal set => _nodes[NEXT] = value;
		}

		public T Value { get; set; }

		public int Degree { get; internal set; }

		public bool Marked { get; internal set; }

		public bool IsLeaf => _nodes[NEXT] == null && _nodes[CHILD] == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		[ItemNotNull]
		public IEnumerable<FibonacciNode<T>> Backwards()
		{
			FibonacciNode<T> previous = _nodes[PREVIOUS];

			while (previous != this)
			{
				yield return previous;
				previous = previous._nodes[PREVIOUS];
			}
		}

		[ItemNotNull]
		public IEnumerable<FibonacciNode<T>> Forwards()
		{
			FibonacciNode<T> next = _nodes[NEXT];

			while (next != this)
			{
				yield return next;
				next = next._nodes[NEXT];
			}
		}

		[ItemNotNull]
		public IEnumerable<FibonacciNode<T>> Ancestors()
		{
			FibonacciNode<T> parent = _nodes[PARENT];

			while (parent != null)
			{
				yield return parent;
				parent = parent._nodes[PARENT];
			}
		}

		[ItemNotNull]
		public IEnumerable<FibonacciNode<T>> Children()
		{
			FibonacciNode<T> child = _nodes[CHILD];

			while (child != null)
			{
				yield return child;
				child = child._nodes[CHILD];
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap([NotNull] FibonacciNode<T> other)
		{
			T tmp = other.Value;
			other.Value = Value;
			Value = tmp;
		}
	}
}