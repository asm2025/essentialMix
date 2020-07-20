using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)">Weighted Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	/// <typeparam name="TWeight">The weight of the edge.</typeparam>
	/// <typeparam name="T"><inheritdoc/></typeparam>
	[Serializable]
	public abstract class WeightedGraphList<T, TWeight> : GraphList<T, KeyedCollection<T, GraphEdge<T, TWeight>>, GraphEdge<T, TWeight>>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		[DebuggerDisplay("{Value}")]
		protected class PathEntry
		{
			public PathEntry([NotNull] T value, TWeight weight)
			{
				Value = value;
				Weight = weight;
			}

			[NotNull]
			public readonly T Value;
			public TWeight Weight;
		}

		private struct EdgeEnumerator : IEnumerableEnumerator<EdgeEntry<T, GraphEdge<T, TWeight>>>
		{
			private readonly WeightedGraphList<T, TWeight> _graph;
			private readonly int _version;
			private readonly Queue<T> _queue;
			private readonly Queue<EdgeEntry<T, GraphEdge<T, TWeight>>> _edges;

			private EdgeEntry<T, GraphEdge<T, TWeight>> _current;
			private bool _started;
			private bool _done;

			internal EdgeEnumerator([NotNull] WeightedGraphList<T, TWeight> graph)
			{
				_graph = graph;
				_version = graph._version;
				_current = default(EdgeEntry<T, GraphEdge<T, TWeight>>);
				_started = false;
				_done = _graph.Count == 0;

				if (_done)
				{
					_queue = null;
					_edges = null;
				}
				else
				{
					_queue = new Queue<T>();
					_edges = new Queue<EdgeEntry<T, GraphEdge<T, TWeight>>>();
				}
			}

			/// <inheritdoc />
			[NotNull]
			public EdgeEntry<T, GraphEdge<T, TWeight>> Current
			{
				get
				{
					if (_current == null) throw new InvalidOperationException();
					return _current;
				}
			}

			/// <inheritdoc />
			[NotNull]
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
			public IEnumerator<EdgeEntry<T, GraphEdge<T, TWeight>>> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;

					foreach (T vertex in _graph.Keys)
						_queue.Enqueue(vertex);
				}

				// visit the next queued edge
				if (_edges.Count > 0)
				{
					_current = _edges.Dequeue();
					return true;
				}

				// no more vertices to explore
				if (_queue.Count == 0)
				{
					_done = true;
					return false;
				}

				// Queue the next edges
				while (_queue.Count > 0)
				{
					T from = _queue.Dequeue();
					KeyedCollection<T, GraphEdge<T, TWeight>> edges = _graph[from];
					if (edges == null || edges.Count == 0) continue;

					foreach (GraphEdge<T, TWeight> edge in edges)
						_edges.Enqueue(new EdgeEntry<T, GraphEdge<T, TWeight>>(from, edge));

					_current = _edges.Dequeue();
					if (_edges.Count > 0) break;
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = default(EdgeEntry<T, GraphEdge<T, TWeight>>);
				_started = false;
				_queue.Clear();
				_edges.Clear();
				_done = _graph.Count == 0;
			}
		}

		private struct BreadthFirstEnumerator : IEnumerableEnumerator<T>
		{
			private readonly WeightedGraphList<T, TWeight> _graph;
			private readonly int _version;
			private readonly T _root;
			private readonly Queue<T> _queue;
			private readonly HashSet<T> _visited;

			private T _current;
			private bool _hasValue;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] WeightedGraphList<T, TWeight> graph, T root)
			{
				_graph = graph;
				_version = graph._version;
				_root = root;
				_current = default(T);
				_started = _hasValue = false;
				_done = _graph.Count == 0 || root == null;
				_visited = _done ? null : new HashSet<T>(graph.Comparer);
				_queue = _done ? null : new Queue<T>();
			}

			/// <inheritdoc />
			[NotNull]
			public T Current
			{
				get
				{
					if (!_hasValue) throw new InvalidOperationException();
					return _current;
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

			public bool MoveNext()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queue.Enqueue(_root);
				}

				// visit the next queued edge
				do
				{
					_hasValue = _queue.Count > 0;
					_current = _hasValue
									? _queue.Dequeue()
									: default(T);
				}
				while (_hasValue && _visited.Contains(_current));

				if (!_hasValue || _current == null /* just for R# */)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current);

				// Queue the next vertices
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = _graph[_current];
				if (edges == null || edges.Count == 0) return true;

				foreach (GraphEdge<T, TWeight> edge in edges)
					_queue.Enqueue(edge.To);

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = default(T);
				_started = _hasValue = false;
				_queue.Clear();
				_visited.Clear();
				_done = _graph.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct DepthFirstEnumerator : IEnumerableEnumerator<T>
		{
			private readonly WeightedGraphList<T, TWeight> _graph;
			private readonly int _version;
			private readonly T _root;
			private readonly Stack<T> _stack;
			private readonly HashSet<T> _visited;

			private T _current;
			private bool _hasValue;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] WeightedGraphList<T, TWeight> graph, T root)
			{
				_graph = graph;
				_version = _graph._version;
				_root = root;
				_current = default(T);
				_started = _hasValue = false;
				_done = _graph.Count == 0 || _root == null;
				_visited = _done ? null : new HashSet<T>(graph.Comparer);
				_stack = _done ? null : new Stack<T>();
			}

			/// <inheritdoc />
			[NotNull]
			public T Current
			{
				get
				{
					if (!_hasValue) throw new InvalidOperationException();
					return _current;
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

			public bool MoveNext()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_stack.Push(_root);
				}

				// visit the next queued edge
				do
				{
					_hasValue = _stack.Count > 0;
					_current = _hasValue
									? _stack.Pop()
									: default(T);
				}
				while (_hasValue && _visited.Contains(_current));

				if (!_hasValue || _current == null /* just for R# */)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current);
				
				// Queue the next vertices
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = _graph[_current];
				if (edges == null || edges.Count == 0) return true;

				foreach (GraphEdge<T, TWeight> edge in edges)
					_stack.Push(edge.To);

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = default(T);
				_started = _hasValue = false;
				_stack.Clear();
				_visited.Clear();
				_done = _graph.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		/// <inheritdoc />
		protected WeightedGraphList() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
		
		/// <inheritdoc />
		protected sealed override KeyedCollection<T, GraphEdge<T, TWeight>> MakeContainer() { return new KeyedCollection<T, GraphEdge<T, TWeight>>(e => e.To, Comparer); }

		/// <inheritdoc />
		public sealed override IEnumerableEnumerator<EdgeEntry<T, GraphEdge<T, TWeight>>> Enumerate() { return new EdgeEnumerator(this); }

		public sealed override IEnumerableEnumerator<T> Enumerate(T from, BreadthDepthTraverse method)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException();
			return method switch
			{
				BreadthDepthTraverse.BreadthFirst => new BreadthFirstEnumerator(this, from),
				BreadthDepthTraverse.DepthFirst => new DepthFirstEnumerator(this, from),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		/// <inheritdoc />
		public sealed override void Iterate(Action<T, GraphEdge<T, TWeight>> visitCallback)
		{
			if (Count == 0) return;

			int version = _version;

			foreach (T vertex in Keys)
			{
				if (version != _version) throw new VersionChangedException();
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (version != _version) throw new VersionChangedException();
					visitCallback(vertex, edge);
				}
			}
		}

		/// <inheritdoc />
		public sealed override void Iterate(Func<T, GraphEdge<T, TWeight>, bool> visitCallback)
		{
			if (Count == 0) return;

			int version = _version;

			foreach (T vertex in Keys)
			{
				if (version != _version) throw new VersionChangedException();
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (version != _version) throw new VersionChangedException();
					if (!visitCallback(vertex, edge)) return;
				}
			}
		}

		/// <inheritdoc />
		protected sealed override void Insert(T key, KeyedCollection<T, GraphEdge<T, TWeight>> collection, bool add)
		{
			if (collection != null && collection.Count > 0)
			{
				foreach (GraphEdge<T, TWeight> edge in collection)
				{
					if (ContainsKey(edge.To)) continue;
					throw new KeyNotFoundException();
				}
			}

			base.Insert(key, collection, add);
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, default(TWeight)); }

		public abstract void AddEdge([NotNull] T from, [NotNull] T to, TWeight weight);

		/// <inheritdoc />
		public sealed override void RemoveAllEdges(T value)
		{
			Remove(value);
			if (Count == 0) return;

			foreach (KeyedCollection<T, GraphEdge<T, TWeight>> collection in Values)
			{
				if (collection == null || collection.Count == 0) continue;
				collection.Remove(value);
			}
		}

		public abstract void SetWeight([NotNull] T from, [NotNull] T to, TWeight weight);

		/// <inheritdoc />
		public sealed override bool ContainsEdge(T from, T to)
		{
			return TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> edges) && edges != null && edges.Count > 0 && edges.ContainsKey(to);
		}

		[NotNull]
		public IEnumerable<T> SingleSourcePath([NotNull] T from, [NotNull] T to, SingleSourcePathAlgorithm algorithm)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");
			return algorithm switch
			{
				SingleSourcePathAlgorithm.Dijkstra => DijkstraShortestPath(from, to),
				SingleSourcePathAlgorithm.BellmanFord => BellmanFordShortestPath(from, to),
				_ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
			};
		}

		/// <inheritdoc />
		protected sealed override bool IsLoop(T value, GraphEdge<T, TWeight> edge)
		{
			return Comparer.Equals(value, edge.To);
		}

		/// <summary>
		/// Dijkstra’s algorithm, published in 1959 and named after its creator Dutch computer
		/// scientist Edsger Dijkstra, can be applied on a weighted graph. The graph can either
		/// be directed or undirected. One stipulation to using the algorithm is that the graph
		/// needs to have a non-negative weight on every edge.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns>vertices of the path in order.</returns>
		[NotNull]
		private IEnumerable<T> DijkstraShortestPath([NotNull] T from, [NotNull] T to)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			TWeight defWeight = default(TWeight);
			TWeight maxWeight = TypeHelper.MaximumOf<TWeight>();
			Dictionary<T, T> history = new Dictionary<T, T>(Comparer);
			BinomialHeap<TWeight, PathEntry> queue = new MinBinomialHeap<TWeight, PathEntry>(e => e.Weight);
			// Map each vertex to its corresponding Entry.
			Dictionary<T, BinomialNode<TWeight, PathEntry>> entries = new Dictionary<T, BinomialNode<TWeight, PathEntry>>(Comparer)
			{
				// start at the target vertex
				{ from, queue.Add(queue.MakeNode(new PathEntry(from, defWeight))) }
			};

			while (queue.Count > 0)
			{
				// edges will be extracted in ascending weight order
				BinomialNode<TWeight, PathEntry> current = queue.ExtractValue();
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[current.Value.Value];
				if (edges == null || edges.Count == 0) continue;

				// Update the priorities of all of its edges.
				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (!history.ContainsKey(current.Value.Value)) history.Add(current.Value.Value, edge.To);
					if (history.ContainsKey(edge.To)) continue;
					// detect negative edges
					if (defWeight.CompareTo(edge.Weight) > 0) throw new NotSupportedException("Negative edge detected.");
					// Compute the cost of the path from the source to this node.
					TWeight weight = current.Key.Add(edge.Weight);
					BinomialNode<TWeight, PathEntry> node;

					if (entries.ContainsKey(edge.To))
					{
						node = entries[edge.To];
					}
					else
					{
						/*
						 * add the new entry to the queue.
						 * IMPORTANT: the connected edges to this vertex will be added only here
						 * and not before the start of the while loop because this way all the next
						 * edges in the queue will be connected to this vertex. If these edges were
						 * added previously, then we would have searched for the appropriate edge
						 * again among the remaining edges which increases the time complexity.
						 */
						node = queue.Add(queue.MakeNode(new PathEntry(edge.To, maxWeight)));
						entries.Add(edge.To, node);
					}

					if (weight.CompareTo(node.Key) >= 0) continue;
					history[edge.To] = current.Value.Value;
					queue.DecreaseKey(node, weight);
				}
			}

			if (history.ContainsKey(from)) history.Remove(from);

			Stack<T> stack = new Stack<T>(history.Count + 1);

			do
			{
				stack.Push(to);
			}
			while (history.TryGetValue(to, out to));

			return stack;
		}

		/// <summary>
		/// Bellman-Ford algorithm computes the shortest paths from a source node to all other nodes in
		/// the graph, Like Dijkstra's algorithm. However, unlike Dijkstra's algorithm, the Bellman-Ford
		/// algorithm works correctly in graphs containing negative-cost edges.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns>vertices of the path in order.</returns>
		[NotNull]
		private IEnumerable<T> BellmanFordShortestPath([NotNull] T from, [NotNull] T to)
		{
			// https://www.youtube.com/watch?v=FtN3BYH2Zes
			// https://www.geeksforgeeks.org/bellman-ford-algorithm-dp-23/
			TWeight defWeight = default(TWeight);
			TWeight maxWeight = TypeHelper.MaximumOf<TWeight>();
			Dictionary<T, TWeight> destTo = new Dictionary<T, TWeight>(Keys.Count, Comparer);
			Dictionary<T, T> history = new Dictionary<T, T>(Comparer);
			
			// Step 1: Initialize distances from src to all other vertices as INFINITE
			foreach (T vertex in Keys) 
				destTo.Add(vertex, maxWeight);

			destTo[from] = defWeight;

			/*
			 * Step 2: Relax all edges |V| - 1 times. A simple shortest
			 * path from src to any other vertex can have at-most |V| - 1
			 * edges. relaxation is making a change that reduces constraints.
			 * When the Dijkstra algorithm examines an edge, it removes an edge
			 * from the pool, thereby reducing the number of constraints.
			 * In Bellman-Ford it means for vertices: u and v when they have an
			 * edge (u, v), if (d[u] + c(u, v) < d[v]) then let d[v] = d[u] + c(u, v)
			 */
			foreach (T vertex in Keys)
			{
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (destTo[vertex].CompareTo(maxWeight) == 0) continue;
					TWeight newWeight = destTo[vertex].Add(edge.Weight);
					if (newWeight.CompareTo(destTo[edge.To /* v */]) >= 0) continue;
					destTo[edge.To] = newWeight;
					history[edge.To] = vertex;
				}
			}

			/*
			 * Step 3: check for negative-weight cycles. The above step guarantees shortest
			 * distances if graph doesn't contain negative weight cycle. If we get a shorter
			 * path, then there is a cycle.
			 */
			foreach (T u in Keys)
			{
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[u];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (destTo[u].CompareTo(maxWeight) == 0) continue;
					TWeight newWeight = destTo[u].Add(edge.Weight);
					if (newWeight.CompareTo(destTo[edge.To /* v */]) >= 0) continue;
					throw new Exception("Graph contains a negative weight cycle.");
				}
			}

			// Step 4: The result
			Stack<T> stack = new Stack<T>(history.Count + 1);

			do
			{
				stack.Push(to);
			}
			while (history.TryGetValue(to, out to));

			return stack;
		}

		[NotNull]
		private IEnumerable<T> BStarShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}

		[NotNull]
		private IEnumerable<T> AStarShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}

		[NotNull]
		private IEnumerable<T> DStarShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}

		[NotNull]
		private IEnumerable<T> FloydWarshallShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}

		[NotNull]
		private IEnumerable<T> JohnsonShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}

		[NotNull]
		private IEnumerable<T> ViterbiShortestPath([NotNull] T from, [NotNull] T to)
		{
			// todo
			throw new NotImplementedException();
		}
	}

	public static class WeightedGraphList
	{
	}
}