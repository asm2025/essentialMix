using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		private struct BreadthFirstEnumerator : IGraphEnumeratorImpl<T>
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

		private struct DepthFirstEnumerator : IGraphEnumeratorImpl<T>
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

		[DebuggerDisplay("{Value}")]
		private struct PathEntry
		{
			public PathEntry([NotNull] T value, TWeight priority)
			{
				Value = value;
				Priority = priority;
			}

			[NotNull]
			public readonly T Value;
			public readonly TWeight Priority;
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
		protected override KeyedCollection<T, GraphEdge<T, TWeight>> NewEdgesContainer() { return new KeyedCollection<T, GraphEdge<T, TWeight>>(e => e.To, Comparer); }
	
		public override IGraphEnumeratorImpl<T> Enumerate(T from, GraphTraverseMethod method)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException();
			return method switch
			{
				GraphTraverseMethod.BreadthFirst => new BreadthFirstEnumerator(this, from),
				GraphTraverseMethod.DepthFirst => new DepthFirstEnumerator(this, from),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		/// <inheritdoc />
		protected override void Insert(T key, KeyedCollection<T, GraphEdge<T, TWeight>> collection, bool add)
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
		public override void RemoveAllEdges(T value)
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
		public override bool ContainsEdge(T from, T to)
		{
			return TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> edges) && edges != null && edges.Count > 0 && edges.ContainsKey(to);
		}

		[NotNull]
		public IEnumerable<T> GetShortestPath([NotNull] T from, [NotNull] T to, ShortestPathAlgorithm algorithm)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			return algorithm switch
			{
				ShortestPathAlgorithm.Dijkstra => DijkstraShortestPath(from, to),
				ShortestPathAlgorithm.BellmanFord => BellmanFordShortestPath(from, to),
				ShortestPathAlgorithm.AStar => AStarShortestPath(from, to),
				ShortestPathAlgorithm.FloydWarshall => FloydWarshallShortestPath(from, to),
				ShortestPathAlgorithm.Johnson => JohnsonShortestPath(from, to),
				ShortestPathAlgorithm.Viterbi => ViterbiShortestPath(from, to),
				_ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
			};
		}

		/// <inheritdoc />
		protected override bool IsLoop(T value, GraphEdge<T, TWeight> edge)
		{
			return Comparer.Equals(value, edge.To);
		}

		[NotNull]
		private IEnumerable<T> DijkstraShortestPath([NotNull] T from, [NotNull] T to)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			Dictionary<T, T> history = new Dictionary<T, T>();
			Dictionary<T, TWeight> weights = new Dictionary<T, TWeight>(Comparer);
			PriorityQueue<TWeight, PathEntry> queue = new MinPriorityQueue<TWeight, PathEntry>(e => e.Priority);
			HashSet<T> visited = new HashSet<T>(Comparer);
			TWeight maxWeight = TypeHelper.MaximumOf<TWeight>();

			foreach (T vertex in Keys)
				weights.Add(vertex, maxWeight);

			weights[from] = default(TWeight);
			queue.Add(new PathEntry(from, default(TWeight)));

			while (queue.Count > 0)
			{
				T current = queue.Remove().Value;
				visited.Add(current);

				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[current];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (visited.Contains(edge.To)) continue;

					TWeight weight = weights[current].Add(edge.Weight);
					if (weight.CompareTo(weights[edge.To]) >= 0) continue;
					weights[edge.To] = weight;
					history[edge.To] = current;
					queue.Add(new PathEntry(edge.To, weight));
				}
			}

			if (!visited.Contains(to)) return Enumerable.Empty<T>();

			Stack<T> stack = new Stack<T>();

			while (history.TryGetValue(to, out T previous))
			{
				stack.Push(to);
				history.Remove(to);
				to = previous;
			}

			stack.Push(to);
			return stack;
		}

		[NotNull]
		private IEnumerable<T> BellmanFordShortestPath([NotNull] T from, [NotNull] T to)
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
}