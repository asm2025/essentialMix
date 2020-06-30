using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;
using asm.Other.Microsoft.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Graph_(abstract_data_type)">Graph (abstract data type)</see> using the <see cref="IDictionary{TKey,TValue}">adjacency list</see> representation.
	/// </summary>
	/// <typeparam name="T">The element type of the tree.</typeparam>
	/// <typeparam name="TAdjacencyList">The edges container type.</typeparam>
	/// <typeparam name="TEdge">The edge type.</typeparam>
	[DebuggerDisplay("Label = '{Label}', Count = {Count}")]
	[Serializable]
	public abstract class GraphList<T, TAdjacencyList, TEdge> : DictionaryBase<T, TAdjacencyList>
		where TAdjacencyList : class, ICollection<TEdge>
	{
		/*
		 * todo: implement iterators to do the following:
		 * 1. Walk: traversing a graph where edge and edges can be repeated.
		 *		a. open walk: when starting and ending vertices are different.
		 *		b. closed walk when starting and ending vertices are the same.
		 * 2. Trail: an open walk in which no edge is repeated.
		 * 3. Circuit: a closed walk in which no edge is repeated.
		 * 4. Path: a trail in which neither vertices nor edges are repeated.
		 * 5. Cycle: traversing a graph where neither vertices nor edges are
		 * repeated and starting and ending vertices are the same.
		 */
		/// <inheritdoc />
		protected GraphList()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected GraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected GraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public string Label { get; set; }

		/// <summary>
		/// Enumerate vertices' values in a semi recursive approach
		/// </summary>
		/// <param name="from">The starting edge's value</param>
		/// <param name="method">The traverse method</param>
		/// <returns></returns>
		[NotNull]
		public abstract IGraphEnumeratorImpl<T> Enumerate([NotNull] T from, GraphTraverseMethod method);

		public void Add([NotNull] T vertex)
		{
			Add(vertex, null);
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection) 
				Add(value, null);
		}

		public abstract void AddEdge([NotNull] T from, [NotNull] T to);

		/// <inheritdoc cref="IDictionary{TKey,TValue}" />
		public override bool Remove(T key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (!base.Remove(key)) return false;
			RemoveAllEdges(key);
			return true;
		}

		public abstract bool RemoveEdge([NotNull] T from, [NotNull] T to);

		public bool RemoveEdges([NotNull] T value)
		{
			if (!ContainsKey(value)) return false;
		 	this[value] = null;
			return true;
		}

		public abstract void RemoveAllEdges([NotNull] T value);

		public void ClearEdges()
		{
			foreach (T key in Keys)
				this[key] = null;
		}

		public abstract bool ContainsEdge([NotNull] T from, [NotNull] T to);

		public int Degree([NotNull] T value)
		{
			return TryGetValue(value, out TAdjacencyList edges) && edges != null
						? edges.Count
						: 0;
		}

		public bool GetFirstConnectedVertex(out T vertex,  out TAdjacencyList edges)
		{
			if (Values.Count > 0)
			{
				foreach (T key in Keys)
				{
					TAdjacencyList adjacencyList = this[key];
					if (adjacencyList == null || adjacencyList.Count == 0) continue;
					vertex = key;
					edges = adjacencyList;
					return true;
				}
			}

			vertex = default(T);
			edges = null;
			return false;
		}

		public virtual int GetSize()
		{
			int sum = 0;

			foreach (TAdjacencyList e in Values)
			{
				if (e == null) continue;
				sum += e.Count;
			}

			return sum;
		}

		public IEnumerable<T> FindCycle(bool ignoreLoop = false) { return FindCycle(Keys, ignoreLoop); }
		protected abstract IEnumerable<T> FindCycle([NotNull] ICollection<T> vertices, bool ignoreLoop = false);

		protected abstract bool IsLoop([NotNull] T value, [NotNull] TEdge edge);

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public bool IsInternal([NotNull] T value)
		{
			return TryGetValue(value, out TAdjacencyList edges) && edges != null && edges.Count > 1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public bool IsExternal([NotNull] T value)
		{
			return TryGetValue(value, out TAdjacencyList edges) && edges != null && edges.Count < 2;
		}

		[NotNull]
		protected abstract TAdjacencyList NewEdgesContainer();
	}

	public abstract class GraphList<T> : GraphList<T, HashSet<T>, T>
	{
		private struct BreadthFirstEnumerator : IGraphEnumeratorImpl<T>
		{
			private readonly GraphList<T> _graph;
			private readonly int _version;
			private readonly T _root;
			private readonly Queue<T> _queue;
			private readonly HashSet<T> _visited;

			private T _current;
			private bool _hasValue;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] GraphList<T> graph, T root)
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
				HashSet<T> edges = _graph[_current];
				if (edges == null || edges.Count == 0) return true;

				foreach (T edge in edges)
					_queue.Enqueue(edge);

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
			private readonly GraphList<T> _graph;
			private readonly int _version;
			private readonly T _root;
			private readonly Stack<T> _stack;
			private readonly HashSet<T> _visited;

			private T _current;
			private bool _hasValue;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] GraphList<T> graph, T root)
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
				HashSet<T> edges = _graph[_current];
				if (edges == null || edges.Count == 0) return true;

				foreach (T edge in edges)
					_stack.Push(edge);

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
		protected GraphList() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected GraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected GraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected GraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	
		/// <inheritdoc />
		protected override HashSet<T> NewEdgesContainer() { return new HashSet<T>(Comparer); }
	
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
		protected override void Insert(T key, HashSet<T> collection, bool add)
		{
			if (collection != null && collection.Count > 0 && !collection.IsSubsetOf(Keys)) throw new KeyNotFoundException();
			base.Insert(key, collection, add);
		}

		/// <inheritdoc />
		public override void RemoveAllEdges(T value)
		{
			Remove(value);
			if (Count == 0) return;

			foreach (HashSet<T> hashset in Values)
			{
				if (hashset == null || hashset.Count == 0) continue;
				hashset.RemoveWhere(e => Comparer.Equals(value, e));
			}
		}

		/// <inheritdoc />
		public override bool ContainsEdge(T from, T to)
		{
			return TryGetValue(from, out HashSet<T> edges) && edges != null && edges.Contains(to);
		}

		/// <inheritdoc />
		protected override bool IsLoop(T value, T edge)
		{
			return Comparer.Equals(value, edge);
		}
	}

	public static class GraphExtension
	{
		public static void WriteTo<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue, [NotNull] TextWriter writer)
			where TAdjacencyList : class, ICollection<TEdge>
		{
			if (thisValue.Count == 0) return;

			foreach (KeyValuePair<T, TAdjacencyList> pair in thisValue)
			{
				if (pair.Value == null || pair.Value.Count == 0) continue;
				writer.WriteLine($"{pair.Key}->[{string.Join(", ", pair.Value)}]");
			}
		}

		/// <summary>
		/// Gets the value with the maximum number of connections
		/// </summary>
		[NotNull]
		public static IEnumerable<T> Top<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue, int count = 1)
			where TAdjacencyList : class, ICollection<TEdge>
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: thisValue.Where(e => e.Value != null)
										.OrderByDescending(e => e.Value.Count)
										.Take(count)
										.Select(e => e.Key);
		}

		/// <summary>
		/// Gets the value with the minimum number of connections
		/// </summary>
		[NotNull]
		public static IEnumerable<T> Bottom<T, TAdjacencyList, TEdge>([NotNull] this GraphList<T, TAdjacencyList, TEdge> thisValue, int count = 1)
			where TAdjacencyList : class, ICollection<TEdge>
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: thisValue.Where(e => e.Value != null)
										.OrderBy(e => e.Value.Count)
										.Take(count)
										.Select(e => e.Key);
		}
	}
}