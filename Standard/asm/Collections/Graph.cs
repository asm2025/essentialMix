using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Graph_(abstract_data_type)">Graph (abstract data type)</see> using the <see cref="IDictionary{TKey,TValue}">adjacency list</see> representation.
	/// </summary>
	/// <typeparam name="TNode">The node type. Must inherit from <see cref="GraphNode{TNode,T}"/></typeparam>
	/// <typeparam name="TEdge">The edge type. Must inherit from <see cref="GraphEdge{TNode, TEdge,T}"/></typeparam>
	/// <typeparam name="T">The element type of the tree</typeparam>
	// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
	// https://www.researchgate.net/publication/2349751_Design_And_Implementation_Of_A_Generic_Graph_Container_In_Java
	// https://www.lri.fr/~filliatr/ftp/publis/ocamlgraph-tfp-8.pdf
	// https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.1944&rep=rep1&type=pdf
	// https://gist.github.com/kevinmorio/f7102c5094aa748503f9
	[DebuggerDisplay("{Label} Count = {Count}")]
	[Serializable]
	public abstract class Graph<TNode, TEdge, T> : ICollection<T>, ICollection
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphEdge<TNode, TEdge, T>
	{
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
		///// a semi recursive approach to traverse the graph
		///// </summary>
		//internal sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		//{
		//	private readonly Graph<TNode, TEdge, T> _graph;
		//	private readonly int _version;
		//	private readonly TNode _root;
		//	private readonly DynamicQueue<TNode> _queueOrStack;
		//	private readonly HashSet<T> _visited;
		//	private readonly Func<bool> _moveNext;

		//	private TNode _current;
		//	private bool _started;
		//	private bool _done;

		//	internal Enumerator([NotNull] Graph<TNode, TEdge, T> graph, [NotNull] TNode root, GraphTraverseMethod method, HorizontalFlow flow)
		//	{
		//		_graph = graph;
		//		_version = _graph._version;
		//		_root = root;
		//		_visited = new HashSet<T>(graph.Comparer);

		//		switch (method)
		//		{
		//			case GraphTraverseMethod.BreadthFirst:
		//				_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.FIFO);
		//				_moveNext = flow switch
		//				{
		//					HorizontalFlow.LeftToRight => BreadthFirstLR,
		//					HorizontalFlow.RightToLeft => BreadthFirstRL,
		//					_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
		//				};
		//				break;
		//			case GraphTraverseMethod.DepthFirst:
		//				_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.LIFO);
		//				_moveNext = flow switch
		//				{
		//					HorizontalFlow.LeftToRight => DepthFirstLR,
		//					HorizontalFlow.RightToLeft => DepthFirstRL,
		//					_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
		//				};
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException(nameof(method), method, null);
		//		}
		//	}

		//	/// <inheritdoc />
		//	[NotNull]
		//	public T Current
		//	{
		//		get
		//		{
		//			if (_current == null) throw new InvalidOperationException();
		//			return _current.Value;
		//		}
		//	}

		//	/// <inheritdoc />
		//	[NotNull]
		//	object IEnumerator.Current => Current;

		//	/// <inheritdoc />
		//	public IEnumerator<T> GetEnumerator()
		//	{
		//		IEnumerator enumerator = this;
		//		enumerator.Reset();
		//		return this;
		//	}

		//	/// <inheritdoc />
		//	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		//	public bool MoveNext() { return _moveNext(); }

		//	void IEnumerator.Reset()
		//	{
		//		_current = null;
		//		_started = _done = false;
		//		_queueOrStack.Clear();
		//		_visited.Clear();
		//	}

		//	/// <inheritdoc />
		//	public void Dispose()
		//	{
		//		_current = null;
		//		_queueOrStack.Clear();
		//	}

		//	private bool BreadthFirstLR()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Left-Right (Queue)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		return true;
		//	}

		//	private bool BreadthFirstRL()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Right-Left (Queue)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		return true;
		//	}

		//	private bool DepthFirstLR()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Left-Right (Stack)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		do
		//		{
		//			_current = _queueOrStack.Count > 0
		//							? _queueOrStack.Dequeue()
		//							: null;
		//			if (_current != null) continue;
		//			_done = true;
		//			return false;
		//		}
		//		while (_visited.Contains(_current.Value));

		//		_visited.Add(_current.Value);

		//		// Queue the next nodes
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		return true;
		//	}

		//	private bool DepthFirstRL()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Right-Left (Stack)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		return true;
		//	}
		//}

		///// <summary>
		///// iterative approach to traverse the tree
		///// </summary>
		//internal sealed class Iterator
		//{
		//	private readonly LinkedBinaryTree<TNode, T> _tree;
		//	private readonly TNode _root;
		//	private readonly BinaryTreeTraverseMethod _method;

		//	internal Iterator([NotNull] LinkedBinaryTree<TNode, T> tree, [NotNull] TNode root, BinaryTreeTraverseMethod method)
		//	{
		//		_tree = tree;
		//		_root = root;
		//		_method = method;
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Action<TNode> visitCallback)
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
		//			Queue<TNode> queue = new Queue<TNode>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = queue.Dequeue();
		//				visitCallback(current);

		//				// Queue the next nodes
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<TNode> queue = new Queue<TNode>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = queue.Dequeue();
		//				visitCallback(current);

		//				// Queue the next nodes
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = stack.Pop();
		//				visitCallback(current);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (current.Right != null) stack.Push(current.Right);
		//				if (current.Left != null) stack.Push(current.Left);
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = stack.Pop();
		//				visitCallback(current);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (current.Left != null) stack.Push(current.Left);
		//				if (current.Right != null) stack.Push(current.Right);
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			TNode current = _root;

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
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			TNode current = _root;

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
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			TNode current = _root;

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

		//				TNode peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the left branch.
		//				 * Is there a right node?
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Right != null && lastVisited != peek.Right)
		//				{
		//					// Navigate right
		//					current = peek.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			TNode current = _root;

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

		//				TNode peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the right branch.
		//				 * Is there a left node?
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Left != null && lastVisited != peek.Left)
		//				{
		//					// Navigate left
		//					current = peek.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					visitCallback(current);
		//					current = null;
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Func<TNode, bool> visitCallback)
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
		//			Queue<TNode> queue = new Queue<TNode>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = queue.Dequeue();
		//				if (!visitCallback(current)) break;

		//				// Queue the next nodes
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<TNode> queue = new Queue<TNode>();

		//			// Start at the root
		//			queue.Enqueue(_root);

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = queue.Dequeue();
		//				if (!visitCallback(current)) break;

		//				// Queue the next nodes
		//				if (current.Right != null) queue.Enqueue(current.Right);
		//				if (current.Left != null) queue.Enqueue(current.Left);
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = stack.Pop();
		//				if (!visitCallback(current)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (current.Right != null) stack.Push(current.Right);
		//				if (current.Left != null) stack.Push(current.Left);
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			stack.Push(_root);

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				TNode current = stack.Pop();
		//				if (!visitCallback(current)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (current.Left != null) stack.Push(current.Left);
		//				if (current.Right != null) stack.Push(current.Right);
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			TNode current = _root;

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
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();

		//			// Start at the root
		//			TNode current = _root;

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
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			TNode current = _root;

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

		//				TNode peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the left branch.
		//				 * Is there a right node?
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Right != null && lastVisited != peek.Right)
		//				{
		//					// Navigate right
		//					current = peek.Right;
		//				}
		//				else
		//				{
		//					// visit the next queued node
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
		//			Stack<TNode> stack = new Stack<TNode>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			TNode current = _root;

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

		//				TNode peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the right branch.
		//				 * Is there a left node?
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Left != null && lastVisited != peek.Left)
		//				{
		//					// Navigate left
		//					current = peek.Left;
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop();
		//					if (!visitCallback(current)) break;
		//					current = null;
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Action<TNode, int> visitCallback)
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
		//			Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = queue.Dequeue();
		//				visitCallback(node, depth);

		//				// Queue the next nodes
		//				if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
		//				if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = queue.Dequeue();
		//				visitCallback(node, depth);

		//				// Queue the next nodes
		//				if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
		//				if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = stack.Pop();
		//				visitCallback(node, depth);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (node.Right != null) stack.Push((depth + 1, node.Right));
		//				if (node.Left != null) stack.Push((depth + 1, node.Left));
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = stack.Pop();
		//				visitCallback(node, depth);

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (node.Left != null) stack.Push((depth + 1, node.Left));
		//				if (node.Right != null) stack.Push((depth + 1, node.Right));
		//			}
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = stack.Pop();
		//					visitCallback(current.Node, current.Depth);

		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = stack.Pop();
		//					visitCallback(current.Node, current.Depth);

		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//					continue;
		//				}

		//				(int Depth, TNode Node) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the left branch.
		//				 * Is there a right TNode
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Node.Right != null && lastVisited != peek.Node.Right)
		//				{
		//					// Navigate right
		//					current = (peek.Depth + 1, peek.Node.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop().Node;
		//					visitCallback(current.Node, current.Depth);
		//					current = (-1, null);
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//					continue;
		//				}

		//				(int Depth, TNode Node) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the right branch.
		//				 * Is there a left TNode
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Node.Left != null && lastVisited != peek.Node.Left)
		//				{
		//					// Navigate left
		//					current = (peek.Depth + 1, peek.Node.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop().Node;
		//					visitCallback(current.Node, current.Depth);
		//					current = (-1, null);
		//				}
		//			}
		//		}
		//	}

		//	public void Iterate(HorizontalFlow flow, [NotNull] Func<TNode, int, bool> visitCallback)
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
		//			Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = queue.Dequeue();
		//				if (!visitCallback(node, depth)) break;

		//				// Queue the next nodes
		//				if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
		//				if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
		//			}
		//		}

		//		void LevelOrderRL()
		//		{
		//			// Root-Right-Left (Queue)
		//			Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

		//			// Start at the root
		//			queue.Enqueue((0, _root));

		//			while (queue.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = queue.Dequeue();
		//				if (!visitCallback(node, depth)) break;

		//				// Queue the next nodes
		//				if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
		//				if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
		//			}
		//		}

		//		void PreOrderLR()
		//		{
		//			// Root-Left-Right (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = stack.Pop();
		//				if (!visitCallback(node, depth)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (node.Right != null) stack.Push((depth + 1, node.Right));
		//				if (node.Left != null) stack.Push((depth + 1, node.Left));
		//			}
		//		}

		//		void PreOrderRL()
		//		{
		//			// Root-Right-Left (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			stack.Push((0, _root));

		//			while (stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				// visit the next queued node
		//				(int depth, TNode node) = stack.Pop();
		//				if (!visitCallback(node, depth)) break;

		//				/*
		//				* The stack works backwards (LIFO).
		//				* It means whatever we want to
		//				* appear first, we must add last.
		//				*/
		//				// Queue the next nodes
		//				if (node.Left != null) stack.Push((depth + 1, node.Left));
		//				if (node.Right != null) stack.Push((depth + 1, node.Right));
		//			}		
		//		}

		//		void InOrderLR()
		//		{
		//			// Left-Root-Right (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = stack.Pop();
		//					if (!visitCallback(current.Node, current.Depth)) break;

		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//				}
		//			}
		//		}

		//		void InOrderRL()
		//		{
		//			// Right-Root-Left (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = stack.Pop();
		//					if (!visitCallback(current.Node, current.Depth)) break;

		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//				}
		//			}
		//		}

		//		void PostOrderLR()
		//		{
		//			// Left-Right-Root (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate left
		//					current = (current.Depth + 1, current.Node.Left);
		//					continue;
		//				}

		//				(int Depth, TNode Node) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the left branch.
		//				 * Is there a right TNode
		//				 * if yes, then navigate right.
		//				 */
		//				if (peek.Node.Right != null && lastVisited != peek.Node.Right)
		//				{
		//					// Navigate right
		//					current = (peek.Depth + 1, peek.Node.Right);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop().Node;
		//					if (!visitCallback(current.Node, current.Depth)) break;
		//					current = (-1, null);
		//				}
		//			}
		//		}

		//		void PostOrderRL()
		//		{
		//			// Right-Left-Root (Stack)
		//			Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
		//			TNode lastVisited = null;
		//			// Start at the root
		//			(int Depth, TNode Node) current = (0, _root);

		//			while (current.Node != null || stack.Count > 0)
		//			{
		//				if (version != _tree._version) throw new VersionChangedException();

		//				if (current.Node != null)
		//				{
		//					stack.Push(current);
		//					// Navigate right
		//					current = (current.Depth + 1, current.Node.Right);
		//					continue;
		//				}

		//				(int Depth, TNode Node) peek = stack.Peek();
		//				/*
		//				 * At this point we are either coming from
		//				 * either the root node or the right branch.
		//				 * Is there a left TNode
		//				 * if yes, then navigate left.
		//				 */
		//				if (peek.Node.Left != null && lastVisited != peek.Node.Left)
		//				{
		//					// Navigate left
		//					current = (peek.Depth + 1, peek.Node.Left);
		//				}
		//				else
		//				{
		//					// visit the next queued node
		//					current = peek;
		//					lastVisited = stack.Pop().Node;
		//					if (!visitCallback(current.Node, current.Depth)) break;
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
		protected Graph()
			: this((IEqualityComparer<T>)null)
		{
		}

		protected Graph(IEqualityComparer<T> comparer)
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
			Nodes = new KeyedDictionary<T, TNode>(e => e.Value, Comparer);
			Edges = new Dictionary<T, KeyedDictionary<T, TEdge>>(Comparer);
			_collectionRef = Nodes;
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			Add(collection);
		}

		public string Label { get; set; }

		/// <summary>
		/// <inheritdoc cref="ICollection{T}" />
		/// <para>Graph's count is also its order.</para>
		/// </summary>
		public int Count => Nodes.Count;

		public int Size => Edges.Count;

		/// <inheritdoc />
		public bool IsReadOnly => Nodes.IsReadOnly;
		
		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		/// <inheritdoc />
		public bool IsSynchronized => _collectionRef.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collectionRef.SyncRoot;

		[NotNull]
		protected internal KeyedDictionary<T, TNode> Nodes { get; }

		[NotNull]
		protected internal IDictionary<T, KeyedDictionary<T, TEdge>> Edges { get; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Nodes.Keys.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Add(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (Nodes.ContainsKey(value)) throw new DuplicateKeyException();
			Nodes.Add(NewNode(value));
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				Add(value);
			}
		}

		public abstract void AddEdge(T from, T to);

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (!Nodes.RemoveByKey(value)) return false;
			RemoveAllEdges(value);
			return true;
		}

		public abstract void RemoveEdge([NotNull] T from, [NotNull] T to);
		
		public abstract void RemoveEdges([NotNull] T from);
		
		public abstract void RemoveAllEdges([NotNull] T value);

		public void ClearEdges() { Edges.Clear(); }

		/// <inheritdoc />
		public void Clear()
		{
			Nodes.Clear();
			Edges.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T value) { return !ReferenceEquals(value, null) && Nodes.ContainsKey(value); }

		public bool ContainsEdge([NotNull] T from, [NotNull] T to)
		{
			return Edges.Count > 0 && Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> edges) && edges.ContainsKey(to);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Nodes.Keys.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { ((ICollection)Nodes.Keys).CopyTo(array, index); }

		[NotNull]
		protected abstract TNode NewNode(T value);

		[NotNull]
		protected abstract TEdge NewEdge(T value);

		[NotNull]
		protected KeyedDictionary<T, TEdge> NewEdges() { return new KeyedDictionary<T, TEdge>(e => e.To.Value, Comparer); }
	}

	public static class GraphExtension
	{
		public static void WriteTo<TNode, TEdge, T>([NotNull] this Graph<TNode, TEdge, T> thisValue, [NotNull] TextWriter writer)
			where TNode : GraphNode<TNode, T>
			where TEdge : GraphEdge<TNode, TEdge, T>
		{
			if (thisValue.Edges.Count == 0) return;

			IDictionary<T, KeyedDictionary<T, TEdge>> allEdges = thisValue.Edges;

			foreach (TNode node in thisValue.Nodes.Values)
			{
				if (!allEdges.TryGetValue(node.Value, out KeyedDictionary<T, TEdge> nodeEdges)) continue;
				writer.WriteLine($"{node}->[{string.Join(", ", nodeEdges.Values)}]");
			}
		}
	}
}