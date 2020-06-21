using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Skip_list">SkipList</see> is a probabilistic data structure that
	/// allows O(log n) search complexity as well as O(log n) insertion complexity within an ordered sequence of
	/// n elements. It might be an alternative to a <see href="https://en.wikipedia.org/wiki/Linked_list">linked list</see>
	/// to allow skipping items while searching for a value.
	/// <para>This is just a basic implementation</para>
	/// </summary>
	/// <typeparam name="T">The element type of the list</typeparam>
	/*
	 * https://ticki.github.io/blog/skip-lists-done-right/
	 * https://www.geeksforgeeks.org/skip-list/
	 * https://www.usna.edu/Users/cs/roche/courses/s16si486h/probs/skiplist.php?f=SkipList.java
	 * https://stackoverflow.com/questions/31580869/skip-lists-are-they-really-performing-as-good-as-pugh-paper-claim
	 * https://stackoverflow.com/questions/6864278/does-java-have-a-skip-list-implementation
	 * https://www.sanfoundry.com/java-program-implement-skip-list/
	 * https://stackoverflow.com/questions/12067045/random-level-function-in-skip-list
	*/
	[DebuggerDisplay("Count = {Count}, Level = {Level}")]
	[Serializable]
	public sealed class SkipList<T> : ICollection<T>, ICollection
	{
		private const int MAX_LEVEL = 32;
		private const double PROBABILITY = 0.5d;

		[DebuggerDisplay("{Value}")]
		internal class Node
		{
			[NotNull]
			internal Node[] Forward;

			public Node(T value, int level)
			{
				Value = value;
				Forward = new Node[level + 1];
			}

			public T Value { get; set; }

			/// <inheritdoc />
			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }
		}

		public struct Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		{
			private readonly SkipList<T> _list;
			private readonly int _version;
			private readonly Node _root;
			private readonly int _level;

			private Node _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] SkipList<T> list, [NotNull] Node root, int level)
				: this()
			{
				if (!level.InRange(0, list.Level)) throw new ArgumentOutOfRangeException(nameof(level));
				_list = list;
				_version = _list._version;
				_root = root;
				_level = level;
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

			public bool MoveNext()
			{
				if (_version != _list._version) throw new VersionChangedException();
				if (_list.Count == 0) _done = true;
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current = _root.Forward[_level];
				}
				else
				{
					_current = _current.Forward[_level];
				}

				_done = _current == null;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _list._version) throw new VersionChangedException();
				_current = null;
				_started = _done = false;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		internal int _version;

		private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

		private object _syncRoot;

		public SkipList()
			: this(null)
		{
		}

		public SkipList(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
			Header = new Node(default(T), MAX_LEVEL);
		}

		//public SkipList([NotNull] IEnumerable<T> collection)
		//	: this(collection, null)
		//{
		//}

		//public SkipList([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
		//	: this(collection.FastCount(), PROBABILITY, comparer)
		//{
		//	Add(collection);
		//}

		[NotNull]
		public IComparer<T> Comparer { get; }
		public int Level { get; private set; }
		[NotNull]
		internal Node Header { get; private set; }

		/// <inheritdoc cref="ICollection{T}" />
		public int Count { get; private set; }

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return Enumerate(0);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		
		public Enumerator Enumerate(int level)
		{
			return new Enumerator(this, Header, level);
		}

		/// <inheritdoc />
		public void Add(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));

			// create update array and initialize it
			Node[] update = new Node[MAX_LEVEL + 1];
			Node node = GetNode(value, update);

			/*
			 * if node is null, it means the end of the level has been reached or
			 * the node' value != the newly inserted value. So, let's insert the
			 * new value between update[0] and current node.
			 */
			if (node != null && Comparer.IsEqual(node.Value, value)) return;
			
			// Generate a random level for node
			int rLevel = RandomLevel();

			/*
			* if random level is greater than list's current level (node with
			* highest level inserted in list so far), initialize update value
			* with pointer to header for further use.
			*/
			if (rLevel > Level)
			{
				for (int i = Level + 1; i <= rLevel; i++)
					update[i] = Header;

				// Update the list current level
				Level = rLevel;
			}

			// create new node with random level generated
			Node newNode = new Node(value, rLevel);

			// insert node by rearranging pointers
			for (int i = 0; i <= rLevel; i++)
			{
				//if (update[i] == null) continue;
				newNode.Forward[i] = update[i].Forward[i];
				update[i].Forward[i] = newNode;
			}

			Count++;
			_version++;
		}

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (Count == 0) return true;

			// create update array and initialize it
			Node[] update = new Node[MAX_LEVEL + 1];
			Node node = GetNode(value, update);
			if (node == null || !Comparer.IsEqual(node.Value, value)) return false;

			/*
			 * start from lowest level and rearrange pointers just like we do in singly
			 * linked list to remove the target node.
			 */
			// if at level i, next node is not the target node; So, break the loop
			for (int i = 0; i <= Level && update[i].Forward[i] == node; i++) 
				update[i].Forward[i] = node.Forward[i];

			// Remove levels having no elements
			while (Level > 0 && Header.Forward[Level] == null) 
				Level--;

			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Array.Clear(Header.Forward, 0, Header.Forward.Length);
			Count = Level = 0;
			_version++;
		}

		/// <inheritdoc />
		public bool Contains(T value)
		{
			if (ReferenceEquals(value, null) || Count == 0) return false;
			Node node = GetNode(value);
			return node != null && Comparer.IsEqual(node.Value, value);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);

			int lo = arrayIndex, hi = lo + Count;

			foreach (T value in this)
			{
				array[lo++] = value;
				if (lo >= hi) break;
			}
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			* Catch the obvious case assignment will fail.
			* We can find all possible problems by doing the check though.
			* For example, if the element type of the Array is derived from T,
			* we can't figure out if we can successfully copy the element beforehand.
			*/
			array.Length.ValidateRange(index, Count);
			if (Count == 0) return;

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));

			int lo = index, hi = lo + Count;

			foreach (T value in this)
			{
				objects[lo++] = value;
				if (lo >= hi) break;
			}
		}

		private int RandomLevel()
		{
			/*
			 * System.Random should provide a random variable whose probability distribution is (approximately)
			 * a discrete uniform distribution. So don't use RNGRandomHelper.
			 */
			int lvl = 0;

			while (_random.NextDouble() < PROBABILITY && lvl < MAX_LEVEL)
				lvl++;

			return lvl;
		}

		private Node GetNode([NotNull] T value, Node[] update = null)
		{
			Node node = Header;
			/*
			 * start from highest level of skip list
			 * move the current pointer forward while key
			 * is greater than key of node next to current
			 * Otherwise inserted current in update and
			 * move one level down and continue search
			 */
			for (int i = Level; i >= 0; i--)
			{
				while (node.Forward[i] != null && Comparer.IsLessThan(node.Forward[i].Value, value)) 
					node = node.Forward[i];

				if (update == null) continue;
				update[i] = node;
			}

			// reached level 0 and forward pointer to right.
			return node.Forward[0];
		}
	}

	public static class SkipListExtension
	{
		public static void WriteTo<T>([NotNull] this SkipList<T> thisValue, [NotNull] TextWriter writer)
		{
			for (int i = 0; i <= thisValue.Level; i++)
			{
				writer.Write($"Level {i}: ");

				foreach (T value in thisValue.Enumerate(i))
					writer.Write($"{value} ");

				writer.WriteLine();
			}
		}
	}
}