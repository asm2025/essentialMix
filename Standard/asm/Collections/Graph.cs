using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using asm.Patterns.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	// https://en.wikipedia.org/wiki/List_of_data_structures
	public class Graph<T> : Microsoft.Collections.Dictionary<T, ISet<T>>, IGraph<T>//, IEnumerable<T>
		where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		//[Serializable]
		//public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		//{

		//}

		/// <inheritdoc />
		public Graph() 
			: this(0, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public Graph(int capacity)
			: this(capacity, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public Graph(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public Graph(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public Graph([NotNull] IDictionary<T, ISet<T>> dictionary)
			: this(dictionary, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public Graph([NotNull] IDictionary<T, ISet<T>> dictionary, IEqualityComparer<T> comparer)
			: base(dictionary, comparer)
		{
		}

		/// <inheritdoc />
		protected Graph(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		protected override bool OnInserting(T key, ISet<T> item)
		{
			return !ContainsKey(key);
		}

		public void AddEdge(T x, T y)
		{
			if (!TryGetValue(x, out ISet<T> xSet)) throw new ArgumentOutOfRangeException(nameof(x));
			if (!TryGetValue(y, out ISet<T> ySet)) throw new ArgumentOutOfRangeException(nameof(y));
			xSet.Add(y);
			ySet.Add(x);
		}

		/// <summary>
		/// Search the <see cref="Graph{T}"/> using <see cref="TraverseMethod"/> algorithm
		/// </summary>
		/// <param name="start"></param>
		/// <param name="algorithm"></param>
		/// <param name="preVisit"></param>
		/// <returns></returns>
		[NotNull]
		public ICollection<T> Search(T start, TraverseMethod algorithm, Action<T> preVisit = null)
		{
			ISet<T> set = new HashSet<T>(Comparer);
			if (!ContainsKey(start)) return set;

			switch (algorithm)
			{
				case TraverseMethod.LevelOrder:
					return BFSearch(set, start, preVisit);
				case TraverseMethod.PreOrder:
					return DFSearch(set, start, preVisit);
				//case TraverseMethod.DepthFirstInOrder:
				//	??
				default:
					throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
			}

			ICollection<T> BFSearch(ISet<T> visited, T value, Action<T> onVisit)
			{
				Queue<T> queue = new Queue<T>();
				queue.Enqueue(value);

				while (queue.Count > 0)
				{
					T vertex = queue.Dequeue();
					if (visited.Contains(vertex)) continue;
					onVisit?.Invoke(vertex);
					visited.Add(vertex);

					foreach (T neighbor in this[vertex].Where(e => !visited.Contains(e)))
					{
						queue.Enqueue(neighbor);
					}
				}

				return visited;
			}

			ICollection<T> DFSearch(ISet<T> visited, T value, Action<T> onVisit)
			{
				Stack<T> stack = new Stack<T>();
				stack.Push(value);

				while (stack.Count > 0)
				{
					T vertex = stack.Pop();
					if (visited.Contains(vertex)) continue;
					onVisit?.Invoke(vertex);
					visited.Add(vertex);

					foreach (T neighbor in this[vertex].Where(e => !visited.Contains(e)))
					{
						stack.Push(neighbor);
					}
				}

				return visited;
			}
		}

		/// <summary>
		/// Prepare a snapshot for <see cref="GetShortestPath"/> using <see cref="TraverseMethod"/>
		/// </summary>
		/// <param name="start"></param>
		/// <param name="algorithm"></param>
		/// <returns></returns>
		[NotNull]
		public GraphSnapshot<T> GetSnapshot(T start, TraverseMethod algorithm)
		{
			GraphSnapshot<T> snapshot = new GraphSnapshot<T>(Comparer)
			{
				Algorithm = algorithm
			};

			if (!ContainsKey(start)) return snapshot;

			switch (algorithm)
			{
				case TraverseMethod.LevelOrder:
					return BFSnapshot(snapshot, start);
				case TraverseMethod.PreOrder:
					return DFSnapshot(snapshot, start);
				default:
					throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
			}

			GraphSnapshot<T> BFSnapshot(GraphSnapshot<T> visited, T value)
			{
				Queue<T> queue = new Queue<T>();
				queue.Enqueue(value);

				while (queue.Count > 0)
				{
					T vertex = queue.Dequeue();

					foreach (T neighbor in this[vertex].Where(e => !visited.ContainsKey(e)))
					{
						visited[neighbor] = vertex;
						queue.Enqueue(neighbor);
					}
				}

				return visited;
			}

			GraphSnapshot<T> DFSnapshot(GraphSnapshot<T> visited, T value)
			{
				Stack<T> stack = new Stack<T>();
				stack.Push(value);

				while (stack.Count > 0)
				{
					T vertex = stack.Pop();

					foreach (T neighbor in this[vertex].Where(e => !visited.ContainsKey(e)))
					{
						visited[neighbor] = vertex;
						stack.Push(neighbor);
					}
				}

				return visited;
			}
		}

		/// <summary>
		/// Finds the shortest path using a prepared snapshot dictionary from <see cref="GetSnapshot"/>
		/// </summary>
		/// <param name="snapshot"></param>
		/// <param name="vertex"></param>
		/// <returns></returns>
		[NotNull]
		public ICollection<T> GetShortestPath([NotNull] GraphSnapshot<T> snapshot, T vertex) 
		{
			List<T> path = new List<T>();
			T current = vertex;
			IEqualityComparer<T> comparer = snapshot.Comparer;

			while (!comparer.Equals(current, vertex))
			{
				path.Add(current);
				current = snapshot[current];
			}

			path.Add(vertex);
			path.Reverse();
			return path;
		}
	}
}