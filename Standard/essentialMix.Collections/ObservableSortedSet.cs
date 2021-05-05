using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using essentialMix.Exceptions;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Numeric;
using essentialMix.Threading;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	// based on https://referencesource.microsoft.com/#system/compmod/system/collections/generic/sortedset.cs
	//
	// A binary search tree is a red-black tree if it satisfies the following red-black properties:
	// 1. Every node is either red or black
	// 2. Every leaf (nil node) is black
	// 3. If a node is red, then both its children are black
	// 4. Every simple path from a node to a descendant leaf contains the same number of black nodes
	// 
	// The basic idea of red-black tree is to represent 2-3-4 trees as standard BSTs but to add one extra bit of information  
	// per node to encode 3-nodes and 4-nodes. 
	// 4-nodes will be represented as:          B
	//                                                              R            R
	// 3 -node will be represented as:           B             or         B     
	//                                                              R          B               B       R
	// 
	// For a detailed description of the algorithm, take a look at "Algorithms" by Robert Sedgewick.
	//
	[DebuggerTypeProxy(typeof(ObservableSortedSet<>.DebugView))]
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class ObservableSortedSet<T> : ISet<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback, INotifyPropertyChanged, INotifyCollectionChanged
	{
		private const int STACK_ALLOC_THRESHOLD = 100;

		private const string ITEMS_NAME = "Item[]";
		private const string VERSION_NAME = "Version";

		//needed for enumerator
		private const string TREE_NAME = "Tree";
		private const string NODE_VALUE_NAME = "Item";
		private const string ENUM_START_NAME = "EnumStarted";
		private const string REVERSE_NAME = "Reverse";
		private const string ENUM_VERSION_NAME = "EnumVersion";

		//needed for TreeSubset
		private const string MIN_NAME = "Min";
		private const string MAX_NAME = "Max";
		private const string L_BOUND_ACTIVE_NAME = "lBoundActive";
		private const string U_BOUND_ACTIVE_NAME = "uBoundActive";

		internal delegate bool TreeWalkPredicate(Node node);

		/// <summary>
		///     This class represents a subset view into the tree. Any changes to this view
		///     are reflected in the actual tree. Uses the Comparator of the underlying tree.
		/// </summary>
		[Serializable]
		internal sealed class TreeSubSet : ObservableSortedSet<T>, ISerializable, IDeserializationCallback
		{
			private ObservableSortedSet<T> _underlying;
			private T _min, _max;

			//these exist for unbounded collections
			//for instance, you could allow this subset to be defined for i>10. The set will throw if
			//anything <=10 is added, but there is no upper-bound. These features Head(), Tail(), were punted
			//in the spec, and are not available, but the framework is there to make them available at some point.
			private bool _lBoundActive, _uBoundActive;
			//used to see if the count is out of date            

			public TreeSubSet([NotNull] ObservableSortedSet<T> underlying, T min, T max, bool lowerBoundActive, bool upperBoundActive)
				: base(underlying.Comparer)
			{
				_underlying = underlying;
				_min = min;
				_max = max;
				_lBoundActive = lowerBoundActive;
				_uBoundActive = upperBoundActive;
				_root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive); // root is first element within range                                
				_count = 0;
				_version = -1;
				VersionCheckImpl();
			}

			/// <summary>
			///     For serialization and deserialization
			/// </summary>
			private TreeSubSet()
			{
			}

			private TreeSubSet(SerializationInfo info, StreamingContext context)
			{
				_sinfo = info;
				OnDeserializationImpl();
			}

			/// <summary>
			///     Additions to this tree need to be added to the underlying tree as well
			/// </summary>
			internal override bool AddIfNotPresent(T item)
			{
				if (!IsWithinRange(item)) throw new ArgumentOutOfRangeException(nameof(item));
				bool ret = _underlying.AddIfNotPresent(item);
				VersionCheck();
				return ret;
			}

			public override bool Contains(T item)
			{
				VersionCheck();
				return base.Contains(item);
			}

			internal override bool DoRemove(T item)
			{
				if (!IsWithinRange(item)) return false;
				bool ret = _underlying.Remove(item);
				VersionCheck();
				return ret;
			}

			public override void Clear()
			{
				if (Count == 0) return;

				List<T> toRemove = new List<T>();
				BreadthFirstTreeWalk(delegate(Node n)
				{
					toRemove.Add(n.Value);
					return true;
				});

				while (toRemove.Count != 0)
				{
					_underlying.Remove(toRemove[toRemove.Count - 1]);
					toRemove.RemoveAt(toRemove.Count - 1);
				}

				_root = null;
				_count = 0;
				_version = _underlying._version;
			}

			internal override bool IsWithinRange(T item)
			{
				int comp = _lBoundActive
								? Comparer.Compare(_min, item)
								: -1;
				if (comp > 0) return false;
				comp = _uBoundActive
							? Comparer.Compare(_max, item)
							: 1;
				return comp >= 0;
			}

			internal override bool InOrderTreeWalk(TreeWalkPredicate action, bool reverse)
			{
				VersionCheck();

				if (_root == null) return true;

				// The maximum height of a red-black tree is 2*lg(n+1).
				// See page 264 of "Introduction to algorithms" by Thomas H. Cormen
				Stack<Node> stack = new Stack<Node>(2 * (int)Math2.Log2(Count + 1)); //this is not exactly right if count is out of date, but the stack can grow
				Node current = _root;

				while (current != null)
				{
					if (IsWithinRange(current.Value))
					{
						stack.Push(current);
						current = reverse
									? current.Right
									: current.Left;
					}
					else
					{
						current = _lBoundActive && Comparer.Compare(_min, current.Value) > 0
									? current.Right
									: current.Left;
					}
				}

				while (stack.Count != 0)
				{
					current = stack.Pop();
					if (!action(current)) return false;

					Node node = reverse
									? current.Left
									: current.Right;
					while (node != null)
					{
						if (IsWithinRange(node.Value))
						{
							stack.Push(node);
							node = reverse
										? node.Right
										: node.Left;
						}
						else
						{
							node = _lBoundActive && Comparer.Compare(_min, node.Value) > 0
										? node.Right
										: node.Left;
						}
					}
				}

				return true;
			}

			internal override bool BreadthFirstTreeWalk(TreeWalkPredicate action)
			{
				VersionCheck();
				if (_root == null) return true;

				List<Node> processQueue = new List<Node>
				{
					_root
				};

				while (processQueue.Count != 0)
				{
					Node current = processQueue[0];
					processQueue.RemoveAt(0);
					if (IsWithinRange(current.Value) && !action(current)) return false;
					if (current.Left != null && (!_lBoundActive || Comparer.Compare(_min, current.Value) < 0)) processQueue.Add(current.Left);
					if (current.Right != null && (!_uBoundActive || Comparer.Compare(_max, current.Value) > 0)) processQueue.Add(current.Right);
				}

				return true;
			}

			internal override Node FindNode(T item)
			{
				if (!IsWithinRange(item)) return null;
				VersionCheck();
				return base.FindNode(item);
			}

			//this does indexing in an inefficient way compared to the actual sorted-set, but it saves a
			//lot of space
			internal override int InternalIndexOf(T item)
			{
				int count = -1;

				foreach (T i in this)
				{
					count++;
					if (Comparer.Compare(item, i) == 0) return count;
				}

				return -1;
			}

			/// <summary>
			///     checks whether this subset is out of date. updates if necessary.
			/// </summary>
			internal override void VersionCheck() { VersionCheckImpl(); }

			private void VersionCheckImpl()
			{
				Debug.Assert(_underlying != null, "Underlying set no longer exists");
				if (_version == _underlying._version) return;
				_root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive);
				_version = _underlying._version;
				_count = 0;
				InOrderTreeWalk(delegate
				{
					_count++;
					return true;
				});
			}

			//This passes functionality down to the underlying tree, clipping edges if necessary
			//There's nothing gained by having a nested subset. May as well draw it from the base
			//Cannot increase the bounds of the subset, can only decrease it
			public override ObservableSortedSet<T> GetViewBetween(T lowerValue, T upperValue)
			{
				if (_lBoundActive && Comparer.Compare(_min, lowerValue) > 0)
				{
					//lBound = min;
					throw new ArgumentOutOfRangeException(nameof(lowerValue));
				}

				if (_uBoundActive && Comparer.Compare(_max, upperValue) < 0)
				{
					//uBound = max;
					throw new ArgumentOutOfRangeException(nameof(upperValue));
				}

				TreeSubSet ret = (TreeSubSet)_underlying.GetViewBetween(lowerValue, upperValue);
				return ret;
			}

			internal override void IntersectWithEnumerable(IEnumerable<T> other)
			{
				List<T> toSave = new List<T>(Count);

				foreach (T item in other)
				{
					if (!Contains(item)) continue;
					toSave.Add(item);
					Remove(item);
				}

				Clear();
				AddAllElements(toSave);
			}

			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { GetObjectData(info, context); }

			protected override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue(MAX_NAME, _max, typeof(T));
				info.AddValue(MIN_NAME, _min, typeof(T));
				info.AddValue(L_BOUND_ACTIVE_NAME, _lBoundActive);
				info.AddValue(U_BOUND_ACTIVE_NAME, _uBoundActive);
				base.GetObjectData(info, context);
			}

			void IDeserializationCallback.OnDeserialization(object sender)
			{
				//don't do anything here as its already been done by the constructor
				//OnDeserialization(sender);
			}

			protected override void OnDeserialization(object sender) { OnDeserializationImpl(); }

			private void OnDeserializationImpl()
			{
				if (_sinfo == null) throw new SerializationException();

				Comparer = (IComparer<T>)_sinfo.GetValue(nameof(Comparer), typeof(IComparer<T>));
				int savedCount = _sinfo.GetInt32(nameof(Count));
				_max = (T)_sinfo.GetValue(MAX_NAME, typeof(T));
				_min = (T)_sinfo.GetValue(MIN_NAME, typeof(T));
				_lBoundActive = _sinfo.GetBoolean(L_BOUND_ACTIVE_NAME);
				_uBoundActive = _sinfo.GetBoolean(U_BOUND_ACTIVE_NAME);
				_underlying = new ObservableSortedSet<T>();

				if (savedCount != 0)
				{
					T[] items = (T[])_sinfo.GetValue(ITEMS_NAME, typeof(T[]));
					if (items == null) throw new SerializationDataMissingException();

					foreach (T item in items)
						_underlying.Add(item);
				}

				_underlying._version = _sinfo.GetInt32(VERSION_NAME);
				_count = _underlying.Count;
				_version = _underlying._version - 1;
				VersionCheck(); //this should update the count to be right and update root to be right
				if (Count != savedCount) throw new SerializationDataMissingException();
				_sinfo = null;
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback
		{
			private ObservableSortedSet<T> _set;
			private int _version;
			private Stack<Node> _stack;
			
			private Node _current;
			private bool _reverse;
			private SerializationInfo _sinfo;

			internal Enumerator(ObservableSortedSet<T> set)
				: this(set, false)
			{
			}

			internal Enumerator([NotNull] ObservableSortedSet<T> set, bool reverse)
			{
				_set = set;
				_version = set._version;
				_current = null;
				_reverse = reverse;
				_stack = new Stack<Node>(2 * (int)Math2.Log2(set.Count + 1));
				_sinfo = null;
				Initialize();
			}

			private Enumerator(SerializationInfo info, StreamingContext context) 
			{
				_set = null;
				_version = -1;
				_current = null;
				_reverse = false;
				_stack = null;
				_sinfo = info;
			}

			public void Dispose() { }

			public T Current => _current != null
									? _current.Value
									: default(T);

			object IEnumerator.Current
			{
				get
				{
					if (_current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			internal bool NotStartedOrEnded => _current == null;

			/// <inheritdoc />
			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { GetObjectData(info); }
			private void GetObjectData([NotNull] SerializationInfo info)
			{
				info.AddValue(TREE_NAME, _set, typeof(ObservableSortedSet<T>));
				info.AddValue(ENUM_VERSION_NAME, _version);
				info.AddValue(REVERSE_NAME, _reverse);
				info.AddValue(ENUM_START_NAME, !NotStartedOrEnded);
				info.AddValue(NODE_VALUE_NAME, _current == null
													? default(T)
													: _current.Value, typeof(T));
			}

			/// <inheritdoc />
			void IDeserializationCallback.OnDeserialization(object sender) { OnDeserialization(); }
			private void OnDeserialization()
			{
				if (_sinfo == null) throw new SerializationException();
				_set = (ObservableSortedSet<T>)_sinfo.GetValue(TREE_NAME, typeof(ObservableSortedSet<T>));
				_version = _sinfo.GetInt32(ENUM_VERSION_NAME);
				_reverse = _sinfo.GetBoolean(REVERSE_NAME);
				bool enumStarted = _sinfo.GetBoolean(ENUM_START_NAME);
				_stack = new Stack<Node>(2 * (int)Math2.Log2(_set.Count + 1));
				_current = null;
				if (!enumStarted) return;
				T item = (T)_sinfo.GetValue(NODE_VALUE_NAME, typeof(T));
				Initialize();
				
				//go until it reaches the value we want
				while (MoveNext())
				{
					if (_set.Comparer.Compare(Current, item) == 0)
						break;
				}
			}

			public bool MoveNext()
			{
				if (_version != _set._version) throw new VersionChangedException();

				if (_stack.Count == 0)
				{
					_current = null;
					return false;
				}

				_current = _stack.Pop();
				Node node = _reverse
								? _current.Left
								: _current.Right;

				while (node != null)
				{
					Node next = _reverse
									? node.Right
									: node.Left;
					Node other = _reverse
									? node.Left
									: node.Right;

					if (_set.IsWithinRange(node.Value))
					{
						_stack.Push(node);
						node = next;
					}
					else
					{
						node = other == null || !_set.IsWithinRange(other.Value)
									? next
									: other;
					}
				}

				return true;
			}

			void IEnumerator.Reset() { Reset(); }
			internal void Reset()
			{
				if (_version != _set._version) throw new VersionChangedException();

				_stack.Clear();
				Initialize();
			}

			private void Initialize()
			{
				_current = null;
				Node node = _set._root;

				while (node != null)
				{
					Node next = _reverse
									? node.Right
									: node.Left;
					Node other = _reverse
									? node.Left
									: node.Right;

					if (_set.IsWithinRange(node.Value))
					{
						_stack.Push(node);
						node = next;
					}
					else
					{
						node = next == null || !_set.IsWithinRange(next.Value)
									? other
									: next;
					}
				}
			}
		}

		internal class Node
		{
			public Node(T value)
				: this(value, true)
			{
			}

			public Node(T value, bool isRed)
			{
				// The default color will be red, we never need to create a black node directly.
				Value = value;
				IsRed = isRed;
			}

			public bool IsRed;
			public T Value;
			public Node Left;
			public Node Right;
		}

		internal struct ElementCount
		{
			internal int uniqueCount;
			internal int unfoundCount;
		}

		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			public DebugView([NotNull] ObservableSortedSet<T> set)
			{
				Items = set.ToArray();
			}

			[NotNull]
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Items { get; }
		}

		private Node _root;
		private int _version;
		private object _syncRoot;
		private SerializationInfo _sinfo; //A temporary variable which we need during deserialization. 
		private SimpleMonitor _monitor = new SimpleMonitor();
		private int _count;

		public ObservableSortedSet()
			: this((IComparer<T>)null)
		{
		}

		public ObservableSortedSet(IComparer<T> comparer) 
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		public ObservableSortedSet([NotNull] IEnumerable<T> enumerable) 
			: this(enumerable, null)
		{
		}

		public ObservableSortedSet([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer) 
			: this(comparer)
		{
			ObservableSortedSet<T> treeSubSet = enumerable as TreeSubSet;
			
			if (enumerable is ObservableSortedSet<T> set && treeSubSet == null && AreComparersEqual(this, set))
			{
				//breadth first traversal to recreate nodes
				if (set.Count == 0)
				{
					_count = 0;
					_version = 0;
					_root = null;
					return;
				}

				//pre order way to replicate nodes
				Stack<Node> theirStack = new Stack<Node>(2 * (int)Math2.Log2(set.Count) + 2);
				Stack<Node> myStack = new Stack<Node>(2 * (int)Math2.Log2(set.Count) + 2);
				Node theirCurrent = set._root;
				Node myCurrent = theirCurrent != null
									? new Node(theirCurrent.Value, theirCurrent.IsRed)
									: null;
				_root = myCurrent;

				while (theirCurrent != null)
				{
					theirStack.Push(theirCurrent);
					myStack.Push(myCurrent);

					if (myCurrent != null)
					{
						myCurrent.Left = theirCurrent.Left != null
											? new Node(theirCurrent.Left.Value, theirCurrent.Left.IsRed)
											: null;
					}
					else
					{
						myCurrent = theirCurrent.Left != null
											? new Node(theirCurrent.Left.Value, theirCurrent.Left.IsRed)
											: null;
					}

					theirCurrent = theirCurrent.Left;
					myCurrent = myCurrent?.Left;
				}

				while (theirStack.Count != 0)
				{
					theirCurrent = theirStack.Pop();
					myCurrent = myStack.Pop();
					Node theirRight = theirCurrent.Right;
					Node myRight = null;
					if (theirRight != null) myRight = new Node(theirRight.Value, theirRight.IsRed);
					myCurrent.Right = myRight;

					while (theirRight != null)
					{
						theirStack.Push(theirRight);
						myStack.Push(myRight);

						if (myRight != null)
						{
							myRight.Left = theirRight.Left != null
												? new Node(theirRight.Left.Value, theirRight.Left.IsRed)
												: null;
						}
						else
						{
							myRight = theirRight.Left != null
												? new Node(theirRight.Left.Value, theirRight.Left.IsRed)
												: null;
						}

						theirRight = theirRight.Left;
						myRight = myRight?.Left;
					}
				}

				_count = set.Count;
				_version = 0;
			}
			else
			{
				//As it stands, you're doing an NlogN sort of the collection
				List<T> els = new List<T>(enumerable);
				els.Sort(Comparer);

				for (int i = 1; i < els.Count; i++)
				{
					if (comparer.Compare(els[i], els[i - 1]) != 0) continue;
					els.RemoveAt(i);
					i--;
				}

				_root = ConstructRootFromSortedArray(els.ToArray(), 0, els.Count - 1, null);
				_count = els.Count;
				_version = 0;
			}
		}

		protected ObservableSortedSet(SerializationInfo info, StreamingContext context) 
		{
			_sinfo = info;
		}
		
		// LinkDemand here is unnecessary as this is a methodImpl and linkDemand from the interface should suffice
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { GetObjectData(info, context); }
		protected virtual void GetObjectData([NotNull] SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Count), Count); //This is the length of the bucket array.
			info.AddValue(nameof(Comparer), Comparer, typeof(IComparer<T>));
			info.AddValue(VERSION_NAME, _version);
			if (_root == null) return;
			T[] items = new T[Count];
			CopyTo(items, 0);
			info.AddValue(ITEMS_NAME, items, typeof(T[]));
		}

		void IDeserializationCallback.OnDeserialization(object sender) { OnDeserialization(sender); }
		protected virtual void OnDeserialization(object sender)
		{
			if (Comparer != null) return; //Somebody had a dependency on this class and fixed us up before the ObjectManager got to it.
			if (_sinfo == null) throw new SerializationException();
			Comparer = (IComparer<T>)_sinfo.GetValue(nameof(Comparer), typeof(IComparer<T>));
			
			int savedCount = _sinfo.GetInt32(nameof(Count));

			if (savedCount != 0)
			{
				T[] items = (T[])_sinfo.GetValue(ITEMS_NAME, typeof(T[]));
				if (items == null) throw new SerializationDataMissingException();
				SuppressCollectionEvents = true;

				try
				{
					foreach (T item in items) 
						Add(item);
				}
				finally
				{
					SuppressCollectionEvents = false;
				}
			}

			_version = _sinfo.GetInt32(VERSION_NAME);
			if (Count != savedCount) throw new SerializationDataMissingException();
			_sinfo = null;
		}

		public int Count
		{
			get
			{
				VersionCheck();
				return _count;
			}
		}

		public IComparer<T> Comparer { get; private set; }

		public T Min
		{
			get
			{
				T ret = default(T);
				InOrderTreeWalk(delegate(Node n)
				{
					ret = n.Value;
					return false;
				});
				return ret;
			}
		}

		public T Max
		{
			get
			{
				T ret = default(T);
				InOrderTreeWalk(delegate(Node n)
				{
					ret = n.Value;
					return false;
				}, true);
				return ret;
			}
		}

		bool ICollection<T>.IsReadOnly => false;

		bool ICollection.IsSynchronized => false;

		protected bool SuppressCollectionEvents { get; set; }

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add => PropertyChanged += value;
			remove => PropertyChanged -= value;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public Enumerator GetEnumerator() { return new Enumerator(this); }

		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return new Enumerator(this); }

		IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

		/// <summary>
		///     Add the value ITEM to the tree, returns true if added, false if duplicate
		/// </summary>
		/// <param name="item">item to be added</param>
		public bool Add(T item) { return AddIfNotPresent(item); }

		void ICollection<T>.Add(T item) { AddIfNotPresent(item); }

		/// <summary>
		///     Remove the T ITEM from this ObservableSortedSet. Returns true if successfully removed.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{
			return DoRemove(item); // hack so it can be made non-virtual
		}

		public virtual void Clear()
		{
			CheckReentrancy();
			_root = null;
			_count = 0;
			++_version;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		public virtual bool Contains(T item) { return FindNode(item) != null; }

		public void CopyTo([NotNull] T[] array) { CopyTo(array, 0, Count); }

		public void CopyTo(T[] array, int index) { CopyTo(array, index, Count); }

		public void CopyTo([NotNull] T[] array, int index, int count)
		{
			array.Length.ValidateRange(index, count);
			if (Count < count) throw new ArgumentOutOfRangeException(nameof(count));
			//upper bound
			count += index;

			InOrderTreeWalk(delegate(Node node)
			{
				if (index >= count) return false;
				array[index++] = node.Value;
				return true;
			});
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array is T[] arr)
			{
				CopyTo(arr, index);
				return;
			}
			
			array.Length.ValidateRange(index, Count);
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new LBoundLargerThanZeroException();
			
			object[] objects = array as object[];
			if (objects == null) throw new ArrayTypeMismatchException();
			InOrderTreeWalk(delegate(Node node)
			{
				objects[index++] = node.Value;
				return true;
			});
		}

		/// <summary>
		///     Transform this set into its union with the IEnumerable OTHER
		///     Attempts to insert each element and rejects it if it exists.
		///     NOTE: The caller object is important as UnionWith uses the Comparator
		///     associated with THIS to check equality
		///     Throws ArgumentNullException if OTHER is null
		/// </summary>
		/// <param name="other"></param>
		public void UnionWith(IEnumerable<T> other)
		{
			CheckReentrancy();
			if (other == null) throw new ArgumentNullException(nameof(other));

			ObservableSortedSet<T> s = other as ObservableSortedSet<T>;
			TreeSubSet t = this as TreeSubSet;

			if (t != null) VersionCheck();

			if (s != null && t == null && Count == 0)
			{
				ObservableSortedSet<T> dummy = new ObservableSortedSet<T>(s, Comparer);
				_root = dummy._root;
				_count = dummy.Count;
				_version++;
				OnPropertyChanged(nameof(Count));
				OnPropertyChanged(ITEMS_NAME);
				OnCollectionChanged();
				return;
			}

			if (s != null && t == null && AreComparersEqual(this, s) && s.Count > Count / 2)
			{
				//this actually hurts if N is much greater than M the /2 is arbitrary
				//first do a merge sort to an array.
				T[] merged = new T[s.Count + Count];
				int c = 0;
				Enumerator mine = GetEnumerator();
				Enumerator theirs = s.GetEnumerator();
				bool mineEnded = !mine.MoveNext(), theirsEnded = !theirs.MoveNext();

				while (!mineEnded && !theirsEnded)
				{
					int comp = Comparer.Compare(mine.Current, theirs.Current);

					switch (comp)
					{
						case < 0:
							merged[c++] = mine.Current;
							mineEnded = !mine.MoveNext();
							break;
						case 0:
							merged[c++] = theirs.Current;
							mineEnded = !mine.MoveNext();
							theirsEnded = !theirs.MoveNext();
							break;
						default:
							merged[c++] = theirs.Current;
							theirsEnded = !theirs.MoveNext();
							break;
					}
				}

				if (!mineEnded || !theirsEnded)
				{
					Enumerator remaining = mineEnded
												? theirs
												: mine;
					do 
						merged[c++] = remaining.Current;
					while (remaining.MoveNext());
				}

				//now merged has all c elements
				//safe to gc the root, we  have all the elements
				_root = null;
				_root = ConstructRootFromSortedArray(merged, 0, c - 1, null);
				_count = c;
				_version++;
				OnPropertyChanged(nameof(Count));
				OnPropertyChanged(ITEMS_NAME);
				OnCollectionChanged();
			}
			else
			{
				AddAllElements(other);
			}
		}

		/// <summary>
		///     Transform this set into its intersection with the IEnumerable OTHER
		///     NOTE: The caller object is important as IntersectionWith uses the
		///     comparator associated with THIS to check equality
		///     Throws ArgumentNullException if OTHER is null
		/// </summary>
		/// <param name="other"></param>
		public virtual void IntersectWith(IEnumerable<T> other)
		{
			if (Count == 0) return;

			//HashSet<T> optimizations can't be done until equality comparers and comparers are related
			//Technically, this would work as well with an ISorted<T>
			ObservableSortedSet<T> s = other as ObservableSortedSet<T>;
			TreeSubSet t = this as TreeSubSet;
			if (t != null) VersionCheck();

			//only let this happen if i am also a ObservableSortedSet, not a SubSet
			if (s != null && t == null && AreComparersEqual(this, s))
			{
				CheckReentrancy();
				//first do a merge sort to an array.
				T[] merged = new T[Count];
				int c = 0;
				Enumerator mine = GetEnumerator();
				Enumerator theirs = s.GetEnumerator();
				bool mineEnded = !mine.MoveNext(), theirsEnded = !theirs.MoveNext();
				T max = Max;

				try
				{
					while (!mineEnded && !theirsEnded && Comparer.Compare(theirs.Current, max) <= 0)
					{
						int comp = Comparer.Compare(mine.Current, theirs.Current);

						switch (comp)
						{
							case < 0:
								mineEnded = !mine.MoveNext();
								break;
							case 0:
								merged[c++] = theirs.Current;
								mineEnded = !mine.MoveNext();
								theirsEnded = !theirs.MoveNext();
								break;
							default:
								theirsEnded = !theirs.MoveNext();
								break;
						}
					}
				}
				finally
				{
					mine.Dispose();
					theirs.Dispose();
				}

				//now merged has all c elements
				//safe to gc the root, we  have all the elements
				_root = null;
				_root = ConstructRootFromSortedArray(merged, 0, c - 1, null);
				_count = c;
				_version++;
				OnPropertyChanged(nameof(Count));
				OnPropertyChanged(ITEMS_NAME);
				OnCollectionChanged();
			}
			else
			{
				IntersectWithEnumerable(other);
			}
		}

		/// <summary>
		///     Searches the set for a given value and returns the equal value it finds, if any.
		/// </summary>
		/// <param name="equalValue">The value to search for.</param>
		/// <param name="actualValue">
		///     The value from the set that the search found, or the default value of
		///     <typeparamref name="T" /> when the search yielded no match.
		/// </param>
		/// <returns>A value indicating whether the search was successful.</returns>
		/// <remarks>
		///     This can be useful when you want to reuse a previously stored reference instead of
		///     a newly constructed one (so that more sharing of references can occur) or to look up
		///     a value that has more complete data than the value you currently have, although their
		///     comparer functions indicate they are equal.
		/// </remarks>
		public bool TryGetValue(T equalValue, out T actualValue)
		{
			Node node = FindNode(equalValue);
			
			if (node != null)
			{
				actualValue = node.Value;
				return true;
			}

			actualValue = default(T);
			return false;
		}

		/// <summary>
		///     Transform this set into its complement with the IEnumerable OTHER
		///     NOTE: The caller object is important as ExceptWith uses the
		///     comparator associated with THIS to check equality
		///     Throws ArgumentNullException if OTHER is null
		/// </summary>
		/// <param name="other"></param>
		public void ExceptWith(IEnumerable<T> other)
		{
			if (Count == 0) return;

			if (Equals(other, this))
			{
				Clear();
				return;
			}

			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(this, asSorted))
			{
				//outside range, no point doing anything               
				if (Comparer.Compare(asSorted.Max, Min) < 0 || Comparer.Compare(asSorted.Min, Max) > 0) return;
				CheckReentrancy();
			
				T min = Min;
				T max = Max;
				SuppressCollectionEvents = true;

				try
				{
					foreach (T item in other)
					{
						if (Comparer.Compare(item, min) < 0) continue;
						if (Comparer.Compare(item, max) > 0) break;
						Remove(item);
					}
				}
				finally
				{
					SuppressCollectionEvents = false;
				}

				OnPropertyChanged(nameof(Count));
				OnPropertyChanged(ITEMS_NAME);
				OnCollectionChanged();
			}
			else
			{
				RemoveAllElements(other);
			}
		}

		/// <summary>
		///     Transform this set so it contains elements in THIS or OTHER but not both
		///     NOTE: The caller object is important as SymmetricExceptWith uses the
		///     comparator associated with THIS to check equality
		///     Throws ArgumentNullException if OTHER is null
		/// </summary>
		/// <param name="other"></param>
		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			if (Count == 0)
			{
				UnionWith(other);
				return;
			}

			if (Equals(other, this))
			{
				Clear();
				return;
			}

			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(this, asSorted))
			{
				SymmetricExceptWithSameEc(asSorted);
			}
			else
			{
				//need perf improvement on this
				List<T> elements = new List<T>(other);
				elements.Sort(Comparer);
				SymmetricExceptWithSameEc(elements);
			}
		}

		/// <summary>
		///     Checks whether this Tree is a subset of the IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			if (Count == 0) return true;
			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(this, asSorted))
				return Count <= asSorted.Count && IsSubsetOfSortedSetWithSameEc(asSorted);

			//worst case: mark every element in my set and see if i've counted all O(MlogN)
			ElementCount result = CheckUniqueAndUnfoundElements(other, false);
			return result.uniqueCount == Count && result.unfoundCount >= 0;
		}

		/// <summary>
		///     Checks whether this Tree is a proper subset of the IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			if (other is ICollection collection && Count == 0) return collection.Count > 0;

			//another for sorted sets with the same comparer
			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(this, asSorted))
				return Count < asSorted.Count && IsSubsetOfSortedSetWithSameEc(asSorted);

			//worst case: mark every element in my set and see if i've counted all O(MlogN).
			ElementCount result = CheckUniqueAndUnfoundElements(other, false);
			return result.uniqueCount == Count && result.unfoundCount > 0;
		}

		/// <summary>
		///     Checks whether this Tree is a super set of the IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			if (other is ICollection { Count: 0 }) return true;
			//another for sorted sets with the same comparer
			if (other is not ObservableSortedSet<T> asSorted || !AreComparersEqual(this, asSorted)) return ContainsAllElements(other);
			if (Count < asSorted.Count) return false;
			
			ObservableSortedSet<T> pruned = GetViewBetween(asSorted.Min, asSorted.Max);
				
			foreach (T item in asSorted)
			{
				if (!pruned.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		///     Checks whether this Tree is a proper super set of the IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			if (Count == 0) return false;
			if (other is ICollection { Count: 0 }) return true;

			//another way for sorted sets
			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(asSorted, this))
			{
				if (asSorted.Count >= Count) return false;
				ObservableSortedSet<T> pruned = GetViewBetween(asSorted.Min, asSorted.Max);
				
				foreach (T item in asSorted)
				{
					if (!pruned.Contains(item))
						return false;
				}

				return true;
			}

			//worst case: mark every element in my set and see if i've counted all O(MlogN)
			//slight optimization, put it into a HashSet and then check can do it in O(N+M)
			//but slower in better cases + wastes space
			ElementCount result = CheckUniqueAndUnfoundElements(other, true);
			return result.uniqueCount < Count && result.unfoundCount == 0;
		}

		/// <summary>
		///     Checks whether this Tree has all elements in common with IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public bool SetEquals(IEnumerable<T> other)
		{
			if (other is ObservableSortedSet<T> asSorted && AreComparersEqual(this, asSorted))
			{
				IEnumerator<T> mine = GetEnumerator();
				IEnumerator<T> theirs = asSorted.GetEnumerator();

				try
				{
					bool mineEnded = !mine.MoveNext();
					bool theirsEnded = !theirs.MoveNext();
				
					while (!mineEnded && !theirsEnded)
					{
						if (Comparer.Compare(mine.Current, theirs.Current) != 0) return false;
						mineEnded = !mine.MoveNext();
						theirsEnded = !theirs.MoveNext();
					}

					return mineEnded && theirsEnded;
				}
				finally
				{
					mine.Dispose();
					theirs.Dispose();
				}
			}

			//worst case: mark every element in my set and see if i've counted all
			//O(N) by size of other            
			ElementCount result = CheckUniqueAndUnfoundElements(other, true);
			return result.uniqueCount == Count && result.unfoundCount == 0;
		}

		/// <summary>
		///     Checks whether this Tree has any elements in common with IEnumerable other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Overlaps(IEnumerable<T> other)
		{
			if (Count == 0) return false;
			if (other is ICollection<T> { Count: 0 }) return false;
			if (other is ObservableSortedSet<T> asSorted 
				&& AreComparersEqual(this, asSorted) 
				&& (Comparer.Compare(Min, asSorted.Max) > 0 || Comparer.Compare(Max, asSorted.Min) < 0)) return false;

			foreach (T item in other)
			{
				if (Contains(item))
					return true;
			}

			return false;
		}

		public int RemoveWhere([NotNull] Predicate<T> match)
		{
			CheckReentrancy();

			List<T> matches = new List<T>(Count);

			BreadthFirstTreeWalk(delegate(Node n)
			{
				if (match(n.Value)) matches.Add(n.Value);
				return true;
			});

			// reverse breadth first to (try to) incur low cost
			int actuallyRemoved = 0;
			SuppressCollectionEvents = true;

			try
			{
				for (int i = matches.Count - 1; i >= 0; i--)
				{
					if (Remove(matches[i]))
						actuallyRemoved++;
				}
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			return actuallyRemoved;
		}

		public IEnumerable<T> Reverse()
		{
			Enumerator e = new Enumerator(this, true);
			while (e.MoveNext()) yield return e.Current;
		}

		/// <summary>
		///     Returns a subset of this tree ranging from values lBound to uBound
		///     Any changes made to the subset reflect in the actual tree
		/// </summary>
		/// <param name="lowerValue">Lowest Value allowed in the subset</param>
		/// <param name="upperValue">Highest Value allowed in the subset</param>
		[NotNull]
		public virtual ObservableSortedSet<T> GetViewBetween(T lowerValue, T upperValue)
		{
			if (Comparer.Compare(lowerValue, upperValue) > 0) throw new ArgumentException("lowerBound is greater than upperBound");
			return new TreeSubSet(this, lowerValue, upperValue, true, true);
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (SuppressCollectionEvents || PropertyChanged == null) return;
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
		{
			if (SuppressCollectionEvents) return;
			PropertyChanged?.Invoke(this, e);
		}

		protected void OnCollectionChanged()
		{
			if (SuppressCollectionEvents || CollectionChanged == null) return;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
		{
			if (SuppressCollectionEvents || CollectionChanged == null) return;

			using (BlockReentrancy())
			{
				CollectionChanged?.Invoke(this, e);
			}
		}

		[NotNull]
		protected IDisposable BlockReentrancy()
		{
			_monitor.Enter();
			return _monitor;
		}

		protected void CheckReentrancy()
		{
			if (!_monitor.Busy) return;
			// we can allow changes if there's only one listener - the problem
			// only arises if reentrant changes make the original event args
			// invalid for later listeners.  This keeps existing code working
			// (e.g. Selector.SelectedItems).
			if (CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
				throw new InvalidOperationException("Reentrancy not allowed.");
		}
		
		internal virtual void VersionCheck() { }

		//virtual function for subclass that needs to do range checks
		internal virtual bool IsWithinRange(T item) { return true; }

		/// <summary>
		///     Adds ITEM to the tree if not already present. Returns TRUE if value was successfully added
		///     or FALSE if it is a duplicate
		/// </summary>
		internal virtual bool AddIfNotPresent(T item)
		{
			CheckReentrancy();

			if (_root == null)
			{
				// empty tree
				_root = new Node(item, false);
				_count = 1;
				_version++;
				OnPropertyChanged(nameof(Count));
				OnPropertyChanged(ITEMS_NAME);
				OnCollectionChanged();
				return true;
			}

			//
			// Search for a node at bottom to insert the new node. 
			// If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
			// We split 4-nodes along the search path.
			Node current = _root;
			Node parent = null;
			Node grandParent = null;
			Node greatGrandParent = null;

			//even if we don't actually add to the set, we may be altering its structure (by doing rotations
			//and such). so update version to disable any enumerators/subsets working on it
			_version++;

			int order = 0;

			while (current != null)
			{
				order = Comparer.Compare(item, current.Value);

				if (order == 0)
				{
					// We could have changed root node to red during the search process.
					// We need to set it to black before we return.
					_root.IsRed = false;
					return false;
				}

				// split a 4-node into two 2-nodes                
				if (Is4Node(current))
				{
					Split4Node(current);
					// We could have introduced two consecutive red nodes after split. Fix that by rotation.
					if (IsRedNode(parent)) InsertionBalance(current, ref parent, grandParent, greatGrandParent);
				}

				greatGrandParent = grandParent;
				grandParent = parent;
				parent = current;
				current = order < 0
							? current.Left
							: current.Right;
			}

			Debug.Assert(parent != null, "Parent node cannot be null here!");
			// ready to insert the new node
			Node node = new Node(item);

			if (order > 0) parent!.Right = node;
			else parent!.Left = node;

			// the new node will be red, so we will need to adjust the colors if parent node is also red
			if (parent.IsRed) InsertionBalance(node, ref parent, grandParent, greatGrandParent);

			// Root node is always black
			_root.IsRed = false;
			++_count;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
			return true;
		}

		internal virtual bool DoRemove(T item)
		{
			CheckReentrancy();
			if (_root == null) return false;

			// Search for a node and then find its successor. 
			// Then copy the item from the successor to the matching node and delete the successor. 
			// If a node doesn't have a successor, we can replace it with its left child (if not empty.) 
			// or delete the matching node.
			// 
			// In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
			// Following code will make sure the node on the path is not a 2 Node. 

			//even if we don't actually remove from the set, we may be altering its structure (by doing rotations
			//and such). so update version to disable any enumerators/subsets working on it
			_version++;

			Node current = _root;
			Node parent = null;
			Node grandParent = null;
			Node match = null;
			Node parentOfMatch = null;
			bool foundMatch = false;

			while (current != null)
			{
				if (Is2Node(current))
				{
					// fix up 2-Node
					if (parent == null)
					{
						// current is root. Mark it as red
						current.IsRed = true;
					}
					else
					{
						Node sibling = GetSibling(current, parent);

						if (sibling.IsRed)
						{
							// If parent is a 3-node, flip the orientation of the red link. 
							// We can achieve this by a single rotation        
							// This case is converted to one of other cased below.
							Debug.Assert(!parent.IsRed, "parent must be a black node!");

							if (parent.Right == sibling) RotateLeft(parent);
							else RotateRight(parent);

							parent.IsRed = true;
							sibling.IsRed = false; // parent's color
							// sibling becomes child of grandParent or root after rotation. Update link from grandParent or root
							ReplaceChildOfNodeOrRoot(grandParent, parent, sibling);
							// sibling will become grandParent of current node 
							grandParent = sibling;
							if (parent == match) parentOfMatch = sibling;

							// update sibling, this is necessary for following processing
							sibling = parent.Left == current
										? parent.Right
										: parent.Left;
						}

						Debug.Assert(sibling is { IsRed: false }, "sibling must not be null and it must be black!");

						if (Is2Node(sibling))
						{
							Merge2Nodes(parent, current, sibling);
						}
						else
						{
							// current is a 2-node and sibling is either a 3-node or a 4-node.
							// We can change the color of current to red by some rotation.
							TreeRotation rotation = RotationNeeded(parent, current, sibling);
							Node newGrandParent = null;

							switch (rotation)
							{
								case TreeRotation.Right:
									Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Left.IsRed, "Left child of sibling must be red!");
									sibling.Left.IsRed = false;
									newGrandParent = RotateRight(parent);
									break;
								case TreeRotation.Left:
									Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Right.IsRed, "Right child of sibling must be red!");
									sibling.Right.IsRed = false;
									newGrandParent = RotateLeft(parent);
									break;

								case TreeRotation.RightLeft:
									Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Left.IsRed, "Left child of sibling must be red!");
									newGrandParent = RotateRightLeft(parent);
									break;

								case TreeRotation.LeftRight:
									Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Right.IsRed, "Right child of sibling must be red!");
									newGrandParent = RotateLeftRight(parent);
									break;
							}

							if (newGrandParent != null) newGrandParent.IsRed = parent.IsRed;
							parent.IsRed = false;
							current.IsRed = true;
							ReplaceChildOfNodeOrRoot(grandParent, parent, newGrandParent);
							if (parent == match) parentOfMatch = newGrandParent;
						}
					}
				}

				// we don't need to compare any more once we found the match
				int order = foundMatch
								? -1
								: Comparer.Compare(item, current.Value);
				
				if (order == 0)
				{
					// save the matching node
					foundMatch = true;
					match = current;
					parentOfMatch = parent;
				}

				grandParent = parent;
				parent = current;
				current = order < 0
							? current.Left
							: current.Right;
			}

			// move successor to the matching node position and replace links
			if (match != null)
			{
				ReplaceNode(match, parentOfMatch, parent, grandParent);
				--_count;
			}

			if (_root != null) _root.IsRed = false;
			if (!foundMatch) return false;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
			return true;
		}

		//
		// Do a in order walk on tree and calls the delegate for each node.
		// If the action delegate returns false, stop the walk.
		// 
		// Return true if the entire tree has been walked. 
		// Otherwise returns false.
		//
		internal bool InOrderTreeWalk(TreeWalkPredicate action) { return InOrderTreeWalk(action, false); }
		// Allows for the change in traversal direction. Reverse visits nodes in descending order 
		internal virtual bool InOrderTreeWalk(TreeWalkPredicate action, bool reverse)
		{
			if (_root == null) return true;

			// The maximum height of a red-black tree is 2*lg(n+1).
			// See page 264 of "Introduction to algorithms" by Thomas H. Cormen
			// note: this should be logbase2, but since the stack grows itself, we 
			// don't want the extra cost
			Stack<Node> stack = new Stack<Node>(2 * (int)Math2.Log2(Count + 1));
			Node current = _root;

			while (current != null)
			{
				stack.Push(current);
				current = reverse
							? current.Right
							: current.Left;
			}

			while (stack.Count != 0)
			{
				current = stack.Pop();
				if (!action(current)) return false;

				Node node = reverse
								? current.Left
								: current.Right;

				while (node != null)
				{
					stack.Push(node);
					node = reverse
								? node.Right
								: node.Left;
				}
			}

			return true;
		}

		//
		// Do a left to right breadth first walk on tree and 
		// calls the delegate for each node.
		// If the action delegate returns false, stop the walk.
		// 
		// Return true if the entire tree has been walked. 
		// Otherwise returns false.
		//
		internal virtual bool BreadthFirstTreeWalk(TreeWalkPredicate action)
		{
			if (_root == null) return true;

			List<Node> processQueue = new List<Node>
			{
				_root
			};

			while (processQueue.Count != 0)
			{
				Node current = processQueue[0];
				processQueue.RemoveAt(0);
				if (!action(current)) return false;
				if (current.Left != null) processQueue.Add(current.Left);
				if (current.Right != null) processQueue.Add(current.Right);
			}

			return true;
		}

		internal virtual Node FindNode(T item)
		{
			Node current = _root;

			while (current != null)
			{
				int order = Comparer.Compare(item, current.Value);
				if (order == 0) return current;
				current = order < 0
							? current.Left
							: current.Right;
			}

			return null;
		}

		//used for bit-helpers. Note that this implementation is completely different 
		//from the Subset's. The two should not be mixed. This indexes as if the tree were an array.
		//http://en.wikipedia.org/wiki/Binary_Tree#Methods_for_storing_binary_trees
		internal virtual int InternalIndexOf(T item)
		{
			Node current = _root;
			int count = 0;

			while (current != null)
			{
				int order = Comparer.Compare(item, current.Value);
				if (order == 0) return count;
				current = order < 0
							? current.Left
							: current.Right;
				count = 2 * count +
						(order < 0
							? 1
							: 2);
			}

			return -1;
		}

		internal Node FindRange(T from, T to) { return FindRange(from, to, true, true); }

		internal Node FindRange(T from, T to, bool lowerBoundActive, bool upperBoundActive)
		{
			Node current = _root;
			
			while (current != null)
			{
				if (lowerBoundActive && Comparer.Compare(from, current.Value) > 0)
				{
					current = current.Right;
				}
				else
				{
					if (upperBoundActive && Comparer.Compare(to, current.Value) < 0) current = current.Left;
					else return current;
				}
			}

			return null;
		}

		internal virtual void IntersectWithEnumerable([NotNull] IEnumerable<T> other)
		{
			CheckReentrancy();
			List<T> toSave = new List<T>(Count);
			SuppressCollectionEvents = true;

			try
			{
				foreach (T item in other)
				{
					if (!Contains(item)) continue;
					toSave.Add(item);
					Remove(item);
				}

				Clear();
				AddAllElements(toSave);
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		//OTHER must be a set
		internal void SymmetricExceptWithSameEc([NotNull] ISet<T> other)
		{
			CheckReentrancy();
			SuppressCollectionEvents = true;

			try
			{
				foreach (T item in other) 
				{
					//yes, it is classier to say
					//if (!this.Remove(item)) this.Add(item);
					//but this ends up saving on rotations                    
					if (Contains(item)) Remove(item);
					else Add(item);
				}
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		//OTHER must be a sorted array
		internal void SymmetricExceptWithSameEc([NotNull] IList<T> other)
		{
			if (other.Count == 0) return;
			CheckReentrancy();

			T last = other[0];
			SuppressCollectionEvents = true;

			try
			{
				for (int i = 0; i < other.Count; i++)
				{
					while (i < other.Count && i != 0 && Comparer.Compare(other[i], last) == 0) 
						i++;

					if (i >= other.Count) break;

					if (Contains(other[i])) Remove(other[i]);
					else Add(other[i]);

					last = other[i];
				}
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		private void AddAllElements([NotNull] IEnumerable<T> collection)
		{
			CheckReentrancy();
			SuppressCollectionEvents = true;

			try
			{
				foreach (T item in collection)
				{
					if (!Contains(item))
						Add(item);
				}
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		private void RemoveAllElements([NotNull] IEnumerable<T> collection)
		{
			CheckReentrancy();
			T min = Min;
			T max = Max;
			SuppressCollectionEvents = true;

			try
			{
				foreach (T item in collection)
				{
					if (!(Comparer.Compare(item, min) < 0 || Comparer.Compare(item, max) > 0) && Contains(item))
						Remove(item);
				}
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged();
		}

		private bool ContainsAllElements([NotNull] IEnumerable<T> collection)
		{
			foreach (T item in collection)
			{
				if (!Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		///     Copies this to an array. Used for DebugView
		/// </summary>
		/// <returns></returns>
		[NotNull]
		internal T[] ToArray()
		{
			T[] newArray = new T[Count];
			CopyTo(newArray);
			return newArray;
		}

		private bool IsSubsetOfSortedSetWithSameEc([NotNull] ObservableSortedSet<T> asSorted)
		{
			ObservableSortedSet<T> prunedOther = asSorted.GetViewBetween(Min, Max);

			foreach (T item in this)
			{
				if (!prunedOther.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		///     This works similar to HashSet's CheckUniqueAndUnfound (description below), except that the bit
		///     array maps differently than in the HashSet. We can only use this for the bulk boolean checks.
		///     Determines counts that can be used to determine equality, subset, and superset. This
		///     is only used when other is an IEnumerable and not a HashSet. If other is a HashSet
		///     these properties can be checked faster without use of marking because we can assume
		///     other has no duplicates.
		///     The following count checks are performed by callers:
		///     1. Equals: checks if unfoundCount = 0 and uniqueFoundCount = Count; i.e. everything
		///     in other is in this and everything in this is in other
		///     2. Subset: checks if unfoundCount >= 0 and uniqueFoundCount = Count; i.e. other may
		///     have elements not in this and everything in this is in other
		///     3. Proper subset: checks if unfoundCount > 0 and uniqueFoundCount = Count; i.e
		///     other must have at least one element not in this and everything in this is in other
		///     4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
		///     than Count; i.e. everything in other was in this and this had at least one element
		///     not contained in other.
		///     An earlier implementation used delegates to perform these checks rather than returning
		///     an ElementCount struct; however this was changed due to the perf overhead of delegates.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="returnIfUnfound">
		///     Allows us to finish faster for equals and proper superset
		///     because unfoundCount must be 0.
		/// </param>
		/// <returns></returns>
		// <SecurityKernel Critical="True" Ring="0">
		// <UsesUnsafeCode Name="Local bitArrayPtr of type: Int32*" />
		// <ReferencesCritical Name="Method: BitHelper..ctor(System.Int32*,System.Int32)" Ring="1" />
		// <ReferencesCritical Name="Method: BitHelper.IsMarked(System.Int32):System.Boolean" Ring="1" />
		// <ReferencesCritical Name="Method: BitHelper.MarkBit(System.Int32):System.Void" Ring="1" />
		// </SecurityKernel>
		[SecurityCritical]
		private unsafe ElementCount CheckUniqueAndUnfoundElements([NotNull] IEnumerable<T> other, bool returnIfUnfound)
		{
			ElementCount result;

			// need special case in case this has no elements. 
			if (Count == 0)
			{
				int numElementsInOther = 0;
				
				foreach (T _ in other)
				{
					numElementsInOther++;
					// break right away, all we want to know is whether other has 0 or 1 elements
					break;
				}

				result.uniqueCount = 0;
				result.unfoundCount = numElementsInOther;
				return result;
			}

			int originalLastIndex = Count;
			int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

			BitHelper bitHelper;
			
			if (intArrayLength <= STACK_ALLOC_THRESHOLD)
			{
				int* bitArrayPtr = stackalloc int[intArrayLength];
				bitHelper = new BitHelper(bitArrayPtr, intArrayLength);
			}
			else
			{
				int[] bitArray = new int[intArrayLength];
				bitHelper = new BitHelper(bitArray, intArrayLength);
			}

			// count of items in other not found in this
			int unfoundCount = 0;
			// count of unique items in other found in this
			int uniqueFoundCount = 0;

			foreach (T item in other)
			{
				int index = InternalIndexOf(item);

				if (index >= 0)
				{
					if (bitHelper.IsMarked(index)) continue;
					// item hasn't been seen yet
					bitHelper.MarkBit(index);
					uniqueFoundCount++;
				}
				else
				{
					unfoundCount++;
					if (returnIfUnfound) break;
				}
			}

			result.uniqueCount = uniqueFoundCount;
			result.unfoundCount = unfoundCount;
			return result;
		}

		// After calling InsertionBalance, we need to make sure current and parent up-to-date.
		// It doesn't matter if we keep grandParent and greatGrantParent up-to-date 
		// because we won't need to split again in the next node.
		// By the time we need to split again, everything will be correctly set.
		private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
		{
			Debug.Assert(grandParent != null, "Grand parent cannot be null here!");
			bool parentIsOnRight = grandParent.Right == parent;
			bool currentIsOnRight = parent.Right == current;

			Node newChildOfGreatGrandParent;
			if (parentIsOnRight == currentIsOnRight)
			{
				// same orientation, single rotation
				newChildOfGreatGrandParent = currentIsOnRight
												? RotateLeft(grandParent)
												: RotateRight(grandParent);
			}
			else
			{
				// different orientation, double rotation
				newChildOfGreatGrandParent = currentIsOnRight
												? RotateLeftRight(grandParent)
												: RotateRightLeft(grandParent);
				// current node now becomes the child of great-grandparent 
				parent = greatGrandParent;
			}

			// grand parent will become a child of either parent of current.
			grandParent.IsRed = true;
			newChildOfGreatGrandParent.IsRed = false;

			ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
		}

		// Replace the child of a parent node. 
		// If the parent node is null, replace the root.        
		private void ReplaceChildOfNodeOrRoot(Node parent, Node child, Node newChild)
		{
			if (parent != null)
			{
				if (parent.Left == child) parent.Left = newChild;
				else parent.Right = newChild;
			}
			else
			{
				_root = newChild;
			}
		}

		// Replace the matching node with its successor.
		private void ReplaceNode([NotNull] Node match, Node parentOfMatch, Node successor, Node parentOfSuccessor)
		{
			if (successor == match)
			{
				// this node has no successor, should only happen if right child of matching node is null.
				Debug.Assert(match.Right == null, "Right child must be null!");
				successor = match.Left;
			}
			else
			{
				Debug.Assert(parentOfSuccessor != null, "parent of successor cannot be null!");
				Debug.Assert(successor.Left == null, "Left child of successor must be null!");
				Debug.Assert(successor.Right == null && successor.IsRed || !successor.IsRed && successor.Right is { IsRed: true }, "Successor must be in valid state");
				if (successor.Right != null) successor.Right.IsRed = false;

				if (parentOfSuccessor != match)
				{
					// detach successor from its parent and set its right child
					parentOfSuccessor.Left = successor.Right;
					successor.Right = match.Right;
				}

				successor.Left = match.Left;
			}

			if (successor != null) successor.IsRed = match.IsRed;

			ReplaceChildOfNodeOrRoot(parentOfMatch, match, successor);
		}

		/// <summary>
		///     Used for deep equality of ObservableSortedSet testing
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IEqualityComparer<ObservableSortedSet<T>> CreateSetComparer() { return new ObservableSortedSetEqualityComparer<T>(); }

		/// <summary>
		///     Create a new set comparer for this set, where this set's members' equality is defined by the
		///     memberEqualityComparer. Note that this equality comparer's definition of equality must be the
		///     same as this set's Comparer's definition of equality
		/// </summary>
		[NotNull]
		public static IEqualityComparer<ObservableSortedSet<T>> CreateSetComparer(IEqualityComparer<T> memberEqualityComparer)
		{
			return new ObservableSortedSetEqualityComparer<T>(memberEqualityComparer);
		}

		/// <summary>
		///     Decides whether these sets are the same, given the comparer. If the EC's are the same, we can
		///     just use SetEquals, but if they aren't then we have to manually check with the given comparer
		/// </summary>
		internal static bool SetEquals(ObservableSortedSet<T> set1, ObservableSortedSet<T> set2, IComparer<T> comparer)
		{
			// handle null cases first
			if (set1 == null) return set2 == null;
			if (set2 == null) return false;
			if (AreComparersEqual(set1, set2)) return set1.Count == set2.Count && set1.SetEquals(set2);

			foreach (T item1 in set1)
			{
				bool found = false;

				foreach (T item2 in set2)
				{
					if (comparer.Compare(item1, item2) != 0) continue;
					found = true;
					break;
				}

				if (!found) return false;
			}

			return true;
		}

		//This is a little frustrating because we can't support more sorted structures
		private static bool AreComparersEqual([NotNull] ObservableSortedSet<T> set1, [NotNull] ObservableSortedSet<T> set2) { return set1.Comparer.Equals(set2.Comparer); }

		private static bool Is2Node([NotNull] Node node)
		{
			Debug.Assert(node != null, "node cannot be null!");
			return IsBlackNode(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
		}

		private static bool Is4Node([NotNull] Node node) { return IsRedNode(node.Left) && IsRedNode(node.Right); }

		private static bool IsRedNode(Node node) { return node is { IsRed: true }; }

		private static bool IsBlackNode(Node node) { return node is { IsRed: false }; }

		private static bool IsNullOrBlack(Node node) { return node == null || !node.IsRed; }

		private static void Merge2Nodes([NotNull] Node parent, [NotNull] Node child1, [NotNull] Node child2)
		{
			Debug.Assert(IsRedNode(parent), "parent must be be red");
			// combing two 2-nodes into a 4-node
			parent.IsRed = false;
			child1.IsRed = true;
			child2.IsRed = true;
		}

		private static void Split4Node([NotNull] Node node)
		{
			node.IsRed = true;
			node.Left.IsRed = false;
			node.Right.IsRed = false;
		}

		/// <summary>
		///     Testing counter that can track rotations
		/// </summary>
		private static TreeRotation RotationNeeded([NotNull] Node parent, Node current, [NotNull] Node sibling)
		{
			Debug.Assert(IsRedNode(sibling.Left) || IsRedNode(sibling.Right), "sibling must have at least one red child");
			
			if (IsRedNode(sibling.Left))
			{
				return parent.Left == current
							? TreeRotation.RightLeft
							: TreeRotation.Right;
			}

			return parent.Left == current
						? TreeRotation.Left
						: TreeRotation.LeftRight;
		}

		[NotNull]
		private static Node RotateLeft([NotNull] Node node)
		{
			Node x = node.Right;
			node.Right = x.Left;
			x.Left = node;
			return x;
		}

		[NotNull]
		private static Node RotateLeftRight([NotNull] Node node)
		{
			Node child = node.Left;
			Node grandChild = child.Right;

			node.Left = grandChild.Right;
			grandChild.Right = node;
			child.Right = grandChild.Left;
			grandChild.Left = child;
			return grandChild;
		}

		[NotNull]
		private static Node RotateRight([NotNull] Node node)
		{
			Node x = node.Left;
			node.Left = x.Right;
			x.Right = node;
			return x;
		}

		[NotNull]
		private static Node RotateRightLeft([NotNull] Node node)
		{
			Node child = node.Right;
			Node grandChild = child.Left;

			node.Right = grandChild.Left;
			grandChild.Left = node;
			child.Left = grandChild.Right;
			grandChild.Right = child;
			return grandChild;
		}

		private static Node ConstructRootFromSortedArray(T[] arr, int startIndex, int endIndex, Node redNode)
		{
			//what does this do?
			//you're given a sorted array... say 1 2 3 4 5 6 
			//2 cases:
			//    If there are odd # of elements, pick the middle element (in this case 4), and compute
			//    its left and right branches
			//    If there are even # of elements, pick the left middle element, save the right middle element
			//    and call the function on the rest
			//    1 2 3 4 5 6 -> pick 3, save 4 and call the fn on 1,2 and 5,6
			//    now add 4 as a red node to the lowest element on the right branch
			//             3                       3
			//         1       5       ->     1        5
			//           2       6             2     4   6            
			//    As we're adding to the leftmost of the right branch, nesting will not hurt the red-black properties
			//    Leaf nodes are red if they have no sibling (if there are 2 nodes or if a node trickles
			//    down to the bottom

			//the iterative way to do this ends up wasting more space than it saves in stack frames (at
			//least in what i tried)
			//so we're doing this recursively
			//base cases are described below
			int size = endIndex - startIndex + 1;
			if (size == 0) return null;
			
			Node root;

			switch (size)
			{
				case 1:
				{
					root = new Node(arr[startIndex], false);
					if (redNode != null) root.Left = redNode;
					break;
				}
				case 2:
				{
					root = new Node(arr[startIndex], false)
					{
						Right = new Node(arr[endIndex], false)
						{
							IsRed = true
						}
					};
					if (redNode != null) root.Left = redNode;
					break;
				}
				case 3:
				{
					root = new Node(arr[startIndex + 1], false)
					{
						Left = new Node(arr[startIndex], false),
						Right = new Node(arr[endIndex], false)
					};
					if (redNode != null) root.Left.Left = redNode;
					break;
				}
				default:
				{
					int midPt = (startIndex + endIndex) / 2;
					root = new Node(arr[midPt], false)
					{
						Left = ConstructRootFromSortedArray(arr, startIndex, midPt - 1, redNode),
						Right = size % 2 == 0
									? ConstructRootFromSortedArray(arr, midPt + 2, endIndex, new Node(arr[midPt + 1], true))
									: ConstructRootFromSortedArray(arr, midPt + 1, endIndex, null)
					};
					break;
				}
			}

			return root;
		}

		private static Node GetSibling(Node node, [NotNull] Node parent)
		{
			return parent.Left == node
						? parent.Right
						: parent.Left;
		}
	}
}