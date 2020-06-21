using System;
using System.Collections.Generic;
using System.Diagnostics;
using asm.Other.Microsoft.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Trie">Trie</see> generic implementation using a dictionary (hashMap).
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
	// Udemy - Code With Mosh - Data Structures & Algorithms part 2
	// https://www.geeksforgeeks.org/types-of-tries/?ref=leftbar-rightbar
	[Serializable]
	public class Trie<T>
		where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		/*
		 * For a few entries, this will perform good but this will be heavy on memory for
		 * millions of records, even when using an array or dictionary. An array with 26
		 * (English alphabet without numbers) slots will be fast. In real world it's not
		 * practical. A static trie with linked list might be better.
		 *
		 * https://www.geeksforgeeks.org/trie-memory-optimization-using-hash-map/
		 * https://stackoverflow.com/questions/39519288/which-is-a-better-implementation-to-implement-a-trie-nodes-children-array-or
		 * https://www.techiedelight.com/memory-efficient-trie-implementation-using-map-insert-search-delete/
		 */
		[DebuggerDisplay("{Value}, Count = {Count}")]
		[Serializable]
		protected sealed class Node : KeyedDictionaryBase<T, Node>, IComparable<Node>, IComparable, IEquatable<Node>
		{
			public Node(T value, IEqualityComparer<T> comparer)
				: base(comparer)
			{
				Value = value;
			}

			public T Value { get; }

			public bool EndOfToken { get; set; }

			/// <inheritdoc />
			public override string ToString() { return Value.ToString(); }

			/// <inheritdoc />
			public override bool Equals(object obj)
			{
				return obj is Node other && Equals(other);
			}

			/// <inheritdoc />
			public override int GetHashCode()
			{
				return Value.GetHashCode();
			}

			/// <inheritdoc />
			public int CompareTo(Node other)
			{
				return ReferenceEquals(other, null)
							? 1
							: ReferenceEquals(other, this)
								? 0
								: Value.CompareTo(other.Value);
			}

			/// <inheritdoc />
			public int CompareTo(object obj)
			{
				return CompareTo(obj as Node);
			}

			/// <inheritdoc />
			public bool Equals(Node other) { return !ReferenceEquals(other, null) && Value.Equals(other.Value); }

			[NotNull]
			public Node GetOrAdd(T key, bool endOfToken = false)
			{
				if (!TryGetValue(key, out Node node))
				{
					node = new Node(key, Comparer);
					Add(node);
				}

				if (endOfToken && !node.EndOfToken) node.EndOfToken = true;
				return node;
			}

			/// <inheritdoc />
			protected override T GetKeyForItem(Node item) { return item.Value; }

			public static implicit operator T([NotNull] Node node) { return node.Value; }

			public static bool operator >(Node x, Node y)
			{
				return !ReferenceEquals(x, y) && !ReferenceEquals(x, null) && x.CompareTo(y) > 0;
			}

			public static bool operator <(Node x, Node y)
			{
				return !ReferenceEquals(x, y) && !ReferenceEquals(x, null) && x.CompareTo(y) < 0;
			}

			public static bool operator >=(Node x, Node y)
			{
				return !ReferenceEquals(x, y) && !ReferenceEquals(x, null) && x.CompareTo(y) >= 0;
			}

			public static bool operator <=(Node x, Node y)
			{
				return !ReferenceEquals(x, y) && !ReferenceEquals(x, null) && x.CompareTo(y) <= 0;
			}
		}

		private Node _root;

		public Trie() 
			: this(null)
		{
		}

		public Trie(IEqualityComparer<T> comparer) 
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
		}

		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		[NotNull]
		protected Node Root => _root ??= NewNode(default(T));

		[NotNull]
		protected Node NewNode(T value) { return new Node(value, Comparer); }

		public void Add(T value)
		{
			Root.GetOrAdd(value, true);
		}

		public void Add([NotNull] IEnumerable<T> token)
		{
			Node node = Root;
			
			foreach (T key in token) 
				node = node.GetOrAdd(key);

			if (!ReferenceEquals(node, Root)) node.EndOfToken = true;
		}

		public bool Remove(T value)
		{
			return Root.RemoveByKey(value);
		}

		public bool Remove([NotNull] IEnumerable<T> token)
		{
			Node parent = Root, node = null;
			Stack<(Node Node, Node Parent)> stack = new Stack<(Node Node, Node Parent)>();

			foreach (T value in token)
			{
				if (!parent.TryGetValue(value, out node)) return false;
				stack.Push((node, parent));
				parent = node;
			}

			if (node == null || !node.EndOfToken) return false;

			do
			{
				(node, parent) = stack.Pop();

				if (node.Count > 0)
				{
					node.EndOfToken = false;
					break;
				}

				parent.Remove(node);
			}
			while (stack.Count > 0 && !parent.EndOfToken && parent.Count == 0);

			return true;
		}

		public bool Contains(T value)
		{
			return Root.TryGetValue(value, out Node node) && node.EndOfToken;
		}

		public bool Contains([NotNull] IEnumerable<T> token)
		{
			Node node = Root;

			foreach (T value in token)
			{
				if (!node.TryGetValue(value, out node)) return false;
			}

			return node.EndOfToken;
		}

		[ItemNotNull]
		public IEnumerable<IEnumerable<T>> Find([NotNull] IEnumerable<T> token, int maximum = 10)
		{
			if (maximum < 1) throw new ArgumentOutOfRangeException(nameof(maximum));

			Node node = Root;
			List<T> prefix = new List<T>();

			foreach (T value in token)
			{
				if (!node.TryGetValue(value, out node)) yield break;
				prefix.Add(node.Value);
				// did we reach a complete token yet?
				if (!node.EndOfToken) continue;
				// return the already matching token
				yield return prefix;
				// adjust the maximum required results
				maximum--;
				if (maximum == 0) yield break;
			}

			/*
			 * next step is to keep returning tokens starting from the current node as they
			 * share the same prefix until maximum number of tokens is fulfilled.
			 */
			List<IEnumerable<T>> results = new List<IEnumerable<T>>(maximum);
			Backtrack(node, prefix, prefix.Count, results, maximum);

			foreach (IEnumerable<T> result in results)
			{
				yield return result;
			}

			static void Backtrack(Node node, List<T> prefix, int last, List<IEnumerable<T>> results, int max)
			{
				// prefix: the original prefix.
				// last: keeps track of the original prefix's length.
				// tmp: temp list to build the new tokens.
				// max: the maximum number of results.

				// base case
				if (results.Count >= max) return;

				using (IEnumerator<Node> enumerator = node.Values.GetEnumerator())
				{
					// 2. restriction: move next (has items) and results < max
					while (enumerator.MoveNext() && results.Count < max)
					{
						// 1. choice / decision
						Node child = enumerator.Current;
						if (child == null) break;
						prefix.Add(child.Value);

						// 3. the goal: results of matching words
						// did we reach a complete token yet?
						if (child.EndOfToken) results.Add(new List<T>(prefix));
						// recursive call
						Backtrack(child, prefix, prefix.Count, results, max);
						// and the backtrack: remove the last added letter(s) to navigate for more
						prefix.RemoveRange(last, prefix.Count - last);
					}
				}
			}
		}
	}
}