using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Graph_(abstract_data_type)">Graph (abstract data type)</see> using the <see cref="IDictionary{TKey,TValue}">adjacency list</see> representation.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type. Must inherit from <see cref="GraphVertex{TVertex,T}"/></typeparam>
	/// <typeparam name="TEdge">The edge type. Must inherit from <see cref="GraphEdge{TVertex, TEdge,T}"/></typeparam>
	/// <typeparam name="T">The element type of the tree</typeparam>
	// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
	// https://www.researchgate.net/publication/2349751_Design_And_Implementation_Of_A_Generic_Graph_Container_In_Java
	// https://www.lri.fr/~filliatr/ftp/publis/ocamlgraph-tfp-8.pdf
	// https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.1944&rep=rep1&type=pdf
	// https://gist.github.com/kevinmorio/f7102c5094aa748503f9
	[DebuggerDisplay("Label = '{Label}', Count = {Count}")]
	[Serializable]
	public abstract class GraphList<TVertex, TEdge, T> : ICollection<T>, ICollection
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		/// <summary>
		/// a semi recursive approach to traverse the graph
		/// </summary>
		public sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		{
			private readonly GraphList<TVertex, TEdge, T> _graph;
			private readonly int _version;
			private readonly TVertex _root;
			private readonly DynamicQueue<TVertex> _queueOrStack;
			private readonly HashSet<T> _visited;
			private readonly Func<bool> _moveNext;

			private TVertex _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] GraphList<TVertex, TEdge, T> graph, [NotNull] TVertex root, GraphTraverseMethod method)
			{
				_graph = graph;
				_version = _graph._version;
				_root = root;
				_visited = new HashSet<T>(graph.Comparer);

				switch (method)
				{
					case GraphTraverseMethod.BreadthFirst:
						_queueOrStack = new DynamicQueue<TVertex>(DequeuePriority.FIFO);
						_moveNext = BreadthFirst;
						break;
					case GraphTraverseMethod.DepthFirst:
						_queueOrStack = new DynamicQueue<TVertex>(DequeuePriority.LIFO);
						_moveNext = DepthFirst;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(method), method, null);
				}
			}

			/// <inheritdoc />
			[NotNull]
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			[NotNull]
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext() { return _moveNext(); }

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = null;
				_started = _done = false;
				_queueOrStack.Clear();
				_visited.Clear();
			}

			/// <inheritdoc />
			public void Dispose()
			{
				_current = null;
				_queueOrStack.Clear();
			}

			private bool BreadthFirst()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued vertex
				do
				{
					_current = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: null;
				}
				while (_current != null && _visited.Contains(_current.Value));

				if (_current == null)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current.Value);
				// Queue the next vertices
				if (!_graph.Edges.TryGetValue(_current.Value, out KeyedDictionary<T, TEdge> edges)) return true;

				foreach (TEdge edge in edges.Values) 
					_queueOrStack.Enqueue(edge.To);

				return true;
			}

			private bool DepthFirst()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued vertex
				do
				{
					_current = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: null;
				}
				while (_current != null && _visited.Contains(_current.Value));

				if (_current == null)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current.Value);
				// Queue the next vertices
				if (!_graph.Edges.TryGetValue(_current.Value, out KeyedDictionary<T, TEdge> edges)) return true;

				foreach (TEdge edge in edges.Values) 
					_queueOrStack.Enqueue(edge.To);

				return true;
			}
		}

		/*
		 * 1. Walk: traversing a graph where vertex and edges can be repeated.
		 *		a. open walk: when starting and ending vertices are different.
		 *		b. closed walk when starting and ending vertices are the same.
		 * 2. Trail: an open walk in which no edge is repeated.
		 * 3. Circuit: a closed walk in which no edge is repeated.
		 * 4. Path: a trail in which neither vertices nor edges are repeated.
		 * 5. Cycle: traversing a graph where neither vertices nor edges are
		 * repeated and starting and ending vertices are the same.
		 */

		///// <summary>
		///// iterative approach to traverse the tree
		///// </summary>
		//public sealed class Iterator
		//{
		//	private readonly LinkedBinaryTree<TVertex, T> _tree;
		//	private readonly TVertex _root;
		//	private readonly BinaryTreeTraverseMethod _method;

		//	internal Iterator([NotNull] LinkedBinaryTree<TVertex, T> tree, [NotNull] TVertex root, BinaryTreeTraverseMethod method)
		//	{
		//		_tree = tree;
		//		_root = root;
		//		_method = method;
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Action<TVertex> visitCallback)
		//	{
		//		int version = _tree._version;

		//		switch (_method)
		//		{
		//			case BinaryTreeTraverseMethod.LevelOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						LevelOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						LevelOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PreOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PreOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PreOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.InOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						InOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						InOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PostOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PostOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PostOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException();
		//		}

		//		void LevelOrderLR()
		//		{
		//			// Root-Left-Right (Queue)
		//			Queue<TVertex> queue = new Queue<TVertex>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = queue.Dequeue();
		//				visitCallback(current);

		//				// Queue the next vertices
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<TVertex> queue = new Queue<TVertex>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = queue.Dequeue();
		//				visitCallback(current);

		//				// Queue the next vertices
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = stack.Pop();
		//				visitCallback(current);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (current.Right != null) stack.Push(current.Right);
		//				if (current.Left != null) stack.Push(current.Left);
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = stack.Pop();
		//				visitCallback(current);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (current.Left != null) stack.Push(current.Left);
		//				if (current.Right != null) stack.Push(current.Right);
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = current.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					visitCallback(current);

		//					// Navigate right
		//					current = current.Right;
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = current.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					visitCallback(current);

		//					// Navigate right
		//					current = current.Left;
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = current.Left;
		//					continue;
		//				}

		//				TVertex peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the left branch.
		//				 * Is there a right vertex?
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Right != null && lastVisited != peek.Right)
		//				{
		//					// Navigate right
		//					current = peek.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					visitCallback(current);
		//					current = null;
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = current.Right;
		//					continue;
		//				}

		//				TVertex peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the right branch.
		//				 * Is there a left vertex?
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Left != null && lastVisited != peek.Left)
		//				{
		//					// Navigate left
		//					current = peek.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					visitCallback(current);
		//					current = null;
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Func<TVertex, bool> visitCallback)
		//	{
		//		int version = _tree._version;

		//		switch (_method)
		//		{
		//			case BinaryTreeTraverseMethod.LevelOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						LevelOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						LevelOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PreOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PreOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PreOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.InOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						InOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						InOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PostOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PostOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PostOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException();
		//		}

		//		void LevelOrderLR()
		//		{
		//			// Root-Left-Right (Queue)
		//			Queue<TVertex> queue = new Queue<TVertex>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = queue.Dequeue();
		//				if (!visitCallback(current)) break;

		//				// Queue the next vertices
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<TVertex> queue = new Queue<TVertex>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = queue.Dequeue();
		//				if (!visitCallback(current)) break;

		//				// Queue the next vertices
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = stack.Pop();
		//				if (!visitCallback(current)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (current.Right != null) stack.Push(current.Right);
		//				if (current.Left != null) stack.Push(current.Left);
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				TVertex current = stack.Pop();
		//				if (!visitCallback(current)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (current.Left != null) stack.Push(current.Left);
		//				if (current.Right != null) stack.Push(current.Right);
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = current.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					if (!visitCallback(current)) break;

		//					// Navigate right
		//					current = current.Right;
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();

		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = current.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					if (!visitCallback(current)) break;

		//					// Navigate right
		//					current = current.Left;
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = current.Left;
		//					continue;
		//				}

		//				TVertex peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the left branch.
		//				 * Is there a right vertex?
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Right != null && lastVisited != peek.Right)
		//				{
		//					// Navigate right
		//					current = peek.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					if (!visitCallback(current)) break;
		//					current = null;
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<TVertex> stack = new Stack<TVertex>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			TVertex current = _root;

		//			while (current != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = current.Right;
		//					continue;
		//				}

		//				TVertex peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the right branch.
		//				 * Is there a left vertex?
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Left != null && lastVisited != peek.Left)
		//				{
		//					// Navigate left
		//					current = peek.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					if (!visitCallback(current)) break;
		//					current = null;
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Action<TVertex, int> visitCallback)
		//	{
		//		int version = _tree._version;

		//		switch (_method)
		//		{
		//			case BinaryTreeTraverseMethod.LevelOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						LevelOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						LevelOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PreOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PreOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PreOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.InOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						InOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						InOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PostOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PostOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PostOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException();
		//		}

		//		void LevelOrderLR()
		//		{
		//			// Root-Left-Right (Queue)
		//			Queue<(int Depth, TVertex Vertex)> queue = new Queue<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = queue.Dequeue();
		//				visitCallback(vertex, depth);

		//				// Queue the next vertices
		//				if (vertex.Left != null) queue.Enqueue((depth + 1, vertex.Left));
		//				if (vertex.Right != null) queue.Enqueue((depth + 1, vertex.Right));
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<(int Depth, TVertex Vertex)> queue = new Queue<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = queue.Dequeue();
		//				visitCallback(vertex, depth);

		//				// Queue the next vertices
		//				if (vertex.Right != null) queue.Enqueue((depth + 1, vertex.Right));
		//				if (vertex.Left != null) queue.Enqueue((depth + 1, vertex.Left));
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = stack.Pop();
		//				visitCallback(vertex, depth);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (vertex.Right != null) stack.Push((depth + 1, vertex.Right));
		//				if (vertex.Left != null) stack.Push((depth + 1, vertex.Left));
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = stack.Pop();
		//				visitCallback(vertex, depth);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (vertex.Left != null) stack.Push((depth + 1, vertex.Left));
		//				if (vertex.Right != null) stack.Push((depth + 1, vertex.Right));
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					visitCallback(current.Vertex, current.Depth);

		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					visitCallback(current.Vertex, current.Depth);

		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//					continue;
		//				}

		//				(int Depth, TVertex Vertex) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the left branch.
		//				 * Is there a right TVertex
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Vertex.Right != null && lastVisited != peek.Vertex.Right)
		//				{
		//					// Navigate right
		//					current = (peek.Depth + 1, peek.Vertex.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop().Vertex;
		//					visitCallback(current.Vertex, current.Depth);
		//					current = (-1, null);
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//					continue;
		//				}

		//				(int Depth, TVertex Vertex) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the right branch.
		//				 * Is there a left TVertex
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Vertex.Left != null && lastVisited != peek.Vertex.Left)
		//				{
		//					// Navigate left
		//					current = (peek.Depth + 1, peek.Vertex.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop().Vertex;
		//					visitCallback(current.Vertex, current.Depth);
		//					current = (-1, null);
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Func<TVertex, int, bool> visitCallback)
		//	{
		//		int version = _tree._version;

		//		switch (_method)
		//		{
		//			case BinaryTreeTraverseMethod.LevelOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						LevelOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						LevelOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PreOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PreOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PreOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.InOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						InOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						InOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			case BinaryTreeTraverseMethod.PostOrder:
		//				switch (flow)
		//				{
		//					case HorizontalFlow.LeftToRight:
		//						PostOrderLR();
		//						break;
		//					case HorizontalFlow.RightToLeft:
		//						PostOrderRL();
		//						break;
		//					default:
		//						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
		//				}
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException();
		//		}

		//		void LevelOrderLR()
		//		{
		//			// Root-Left-Right (Queue)
		//			Queue<(int Depth, TVertex Vertex)> queue = new Queue<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = queue.Dequeue();
		//				if (!visitCallback(vertex, depth)) break;

		//				// Queue the next vertices
		//				if (vertex.Left != null) queue.Enqueue((depth + 1, vertex.Left));
		//				if (vertex.Right != null) queue.Enqueue((depth + 1, vertex.Right));
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<(int Depth, TVertex Vertex)> queue = new Queue<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = queue.Dequeue();
		//				if (!visitCallback(vertex, depth)) break;

		//				// Queue the next vertices
		//				if (vertex.Right != null) queue.Enqueue((depth + 1, vertex.Right));
		//				if (vertex.Left != null) queue.Enqueue((depth + 1, vertex.Left));
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = stack.Pop();
		//				if (!visitCallback(vertex, depth)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (vertex.Right != null) stack.Push((depth + 1, vertex.Right));
		//				if (vertex.Left != null) stack.Push((depth + 1, vertex.Left));
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued vertex
		//				(int depth, TVertex vertex) = stack.Pop();
		//				if (!visitCallback(vertex, depth)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next vertices
		//				if (vertex.Left != null) stack.Push((depth + 1, vertex.Left));
		//				if (vertex.Right != null) stack.Push((depth + 1, vertex.Right));
		//			}		
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					if (!visitCallback(current.Vertex, current.Depth)) break;

		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();

		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = stack.Pop();
		//					if (!visitCallback(current.Vertex, current.Depth)) break;

		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Vertex.Left);
		//					continue;
		//				}

		//				(int Depth, TVertex Vertex) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the left branch.
		//				 * Is there a right TVertex
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Vertex.Right != null && lastVisited != peek.Vertex.Right)
		//				{
		//					// Navigate right
		//					current = (peek.Depth + 1, peek.Vertex.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop().Vertex;
		//					if (!visitCallback(current.Vertex, current.Depth)) break;
		//					current = (-1, null);
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<(int Depth, TVertex Vertex)> stack = new Stack<(int Depth, TVertex Vertex)>();
		//			TVertex lastVisited = null;
		//			// Start at the root
		//			(int Depth, TVertex Vertex) current = (0, _root);

		//			while (current.Vertex != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Vertex != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Vertex.Right);
		//					continue;
		//				}

		//				(int Depth, TVertex Vertex) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root vertex or the right branch.
		//				 * Is there a left TVertex
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Vertex.Left != null && lastVisited != peek.Vertex.Left)
		//				{
		//					// Navigate left
		//					current = (peek.Depth + 1, peek.Vertex.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued vertex
		//					current = peek;
		//					lastVisited = stack.Pop().Vertex;
		//					if (!visitCallback(current.Vertex, current.Depth)) break;
		//					current = (-1, null);
		//				}
		//			}
		//		}
		//	}
		//}

		[NotNull]
		private readonly ICollection _collectionRef;

		protected internal int _version;

		/// <inheritdoc />
		protected GraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		protected GraphList(IEqualityComparer<T> comparer)
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
			Vertices = new KeyedDictionary<T, TVertex>(e => e.Value, Comparer);
			Edges = new Dictionary<T, KeyedDictionary<T, TEdge>>(Comparer);
			_collectionRef = Vertices;
		}

		/// <inheritdoc />
		protected GraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected GraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			Add(collection);
		}

		public string Label { get; set; }

		/// <summary>
		/// <inheritdoc cref="ICollection{T}" />
		/// <para>Graph's count is also its order.</para>
		/// </summary>
		public int Count => Vertices.Count;
		
		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => Vertices.IsReadOnly;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => _collectionRef.IsSynchronized;

		/// <inheritdoc />
		object ICollection.SyncRoot => _collectionRef.SyncRoot;

		[NotNull]
		protected internal KeyedDictionary<T, TVertex> Vertices { get; }

		[NotNull]
		protected internal IDictionary<T, KeyedDictionary<T, TEdge>> Edges { get; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Vertices.Keys.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate vertices' values in a semi recursive approach
		/// </summary>
		/// <param name="value">The starting vertex's value</param>
		/// <param name="method">The traverse method</param>
		/// <returns></returns>
		[NotNull]
		public Enumerator Enumerate([NotNull] T value, GraphTraverseMethod method)
		{
			if (!Vertices.TryGetValue(value, out TVertex root)) throw new KeyNotFoundException();
			return new Enumerator(this, root, method);
		}

		/// <inheritdoc />
		public void Add(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (Vertices.ContainsKey(value)) throw new DuplicateKeyException();
			Vertices.Add(NewVertex(value));
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				Add(value);
			}
		}

		public abstract void AddEdge([NotNull] T from, [NotNull] T to);

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (!Vertices.RemoveByKey(value)) return false;
			RemoveAllEdges(value);
			return true;
		}

		public abstract void RemoveEdge([NotNull] T from, [NotNull] T to);

		public void RemoveEdges([NotNull] T value)
		{
			Edges.Remove(value);
		}

		public void RemoveAllEdges([NotNull] T value)
		{
			Edges.Remove(value);
			if (Edges.Count == 0) return;

			List<T> empty = new List<T>();

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
			{
				pair.Value.RemoveByKey(value);
				if (pair.Value.Count == 0) empty.Add(pair.Key);
			}

			if (empty.Count == 0) return;

			foreach (T key in empty)
			{
				Edges.Remove(key);
			}
		}

		public void ClearEdges() { Edges.Clear(); }

		/// <inheritdoc />
		public void Clear()
		{
			Vertices.Clear();
			Edges.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T value) { return !ReferenceEquals(value, null) && Vertices.ContainsKey(value); }

		public bool ContainsEdge([NotNull] T from, [NotNull] T to)
		{
			return Edges.Count > 0 && Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> edges) && edges.ContainsKey(to);
		}

		public bool ContainsEdge([NotNull] T value)
		{
			return Edges.Count > 0 && Edges.ContainsKey(value);
		}

		public int OutDegree([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges)
						? edges.Count
						: 0;
		}

		public virtual int GetSize() { return Edges.Values.Sum(e => e.Count); }

		public ICollection<T> FindCycle(bool ignoreLoop = false) { return FindCycle(Edges.Keys, ignoreLoop); }
		protected ICollection<T> FindCycle([NotNull] ICollection<T> vertices, bool ignoreLoop = false)
		{
			if (vertices.Count == 0) return null;
			
			Stack<T> stack = new Stack<T>();
			Queue<T> cycle = new Queue<T>();
			HashSet<T> visiting = new HashSet<T>(Comparer);
			HashSet<T> visited = new HashSet<T>(Comparer);
			int version = _version;

			foreach (T vertex in vertices)
			{
				if (version != _version) throw new VersionChangedException();
				stack.Push(vertex);

				while (stack.Count > 0)
				{
					T value = stack.Pop();
					if (visited.Contains(value)) continue;
					visiting.Add(value);
					cycle.Enqueue(value);

					if (Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges))
					{
						foreach (TEdge edge in edges)
						{
							if (visited.Contains(edge.To.Value) || ignoreLoop && IsLoop(value, edge)) continue;

							// cycle detected
							if (visiting.Contains(edge.To.Value))
							{
								cycle.Enqueue(edge.To.Value);

								while (!Comparer.Equals(cycle.Peek(), edge.To.Value))
									cycle.Dequeue();

								return cycle.ToArray();
							}

							stack.Push(edge.To.Value);
						}

						continue;
					}

					visiting.Remove(value);
					cycle.Dequeue();
					visited.Add(value);
				}
			}

			return null;
		}

		public bool IsLoop([NotNull] T value, [NotNull] TEdge edge)
		{
			return Comparer.Equals(value, edge.To.Value);
		}

		public bool IsInternal([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges) && edges.Count > 1;
		}

		public bool IsExternal([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges) && edges.Count < 2;
		}

		/// <summary>
		/// Gets the value with the maximum number of connections
		/// </summary>
		[NotNull]
		public IEnumerable<T> Top(int count = 1)
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: Edges.OrderByDescending(e => e.Value.Count)
									.Take(count)
									.Select(e => e.Key);
		}

		/// <summary>
		/// Gets the value with the minimum number of connections
		/// </summary>
		[NotNull]
		public IEnumerable<T> Bottom(int count = 1)
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: Edges.OrderBy(e => e.Value.Count)
									.Take(count)
									.Select(e => e.Key);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Vertices.Keys.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { ((ICollection)Vertices.Keys).CopyTo(array, index); }

		[NotNull]
		protected abstract TVertex NewVertex(T value);

		[NotNull]
		protected abstract TEdge NewEdge(T value);

		[NotNull]
		protected KeyedDictionary<T, TEdge> NewEdgesContainer() { return new KeyedDictionary<T, TEdge>(e => e.To.Value, Comparer); }
	}

	public static class GraphExtension
	{
		public static void WriteTo<TVertex, TEdge, T>([NotNull] this GraphList<TVertex, TEdge, T> thisValue, [NotNull] TextWriter writer)
			where TVertex : GraphVertex<TVertex, T>
			where TEdge : GraphEdge<TVertex, TEdge, T>
		{
			if (thisValue.Edges.Count == 0) return;

			IDictionary<T, KeyedDictionary<T, TEdge>> allEdges = thisValue.Edges;

			foreach (TVertex vertex in thisValue.Vertices.Values)
			{
				if (!allEdges.TryGetValue(vertex.Value, out KeyedDictionary<T, TEdge> vertexEdges)) continue;
				writer.WriteLine($"{vertex}->[{string.Join(", ", vertexEdges.Values)}]");
			}
		}
	}
}