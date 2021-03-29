using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Red%E2%80%93black_tree">RedBlackTree (RBT)</see> implementation using the linked representation.</para>
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
	// https://referencesource.microsoft.com/#system/compmod/system/collections/generic/sortedset.cs
	// Udemy - Mastering Data Structures & Algorithms using C and C++ - Abdul Bari
	// https://www.geeksforgeeks.org/red-black-tree-set-1-introduction-2/
	// https://www.cs.usfca.edu/~galles/visualization/RedBlack.html
	[DebuggerTypeProxy(typeof(RedBlackTree<>.DebugView))]
	[Serializable]
	public sealed class RedBlackTree<T> : LinkedBinaryTree<RedBlackNode<T>, T>
	{
		[DebuggerNonUserCode]
		internal new sealed class DebugView
		{
			private readonly RedBlackTree<T> _tree;

			public DebugView([NotNull] RedBlackTree<T> tree)
			{
				_tree = tree;
			}

			[NotNull]
			public RedBlackNode<T> Root => _tree.Root;
		}

		/// <inheritdoc />
		public RedBlackTree()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public RedBlackTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public RedBlackTree([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public RedBlackTree([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = true;

		/// <inheritdoc />
		protected internal override RedBlackNode<T> MakeNode(T value) { return new RedBlackNode<T>(value); }

		/// <inheritdoc />
		public override int GetHeight()
		{
			if (Root == null || Root.IsLeaf) return 0;
			int height = 0;
			IterateLevels((_, _) => height++);
			return height;
		}

		public override RedBlackNode<T> FindNearestParent(T value)
		{
			int cmp;
			RedBlackNode<T> parent = null, next = Root;

			while (next != null && (cmp = Comparer.Compare(value, next.Value)) != 0)
			{
				parent = next;
				next = cmp < 0
							? next.Left
							: next.Right;
			}

			return parent;
		}

		/// <inheritdoc />
		public override void Add(T value)
		{
			if (Root == null)
			{
				Root = MakeNode(value);
				Root.Color = false;
				Count++;
				_version++;
				return;
			}

			RedBlackNode<T> current = Root,
							parent = null,
							grandParent = null,
							greatGrandParent = null;
			int order = 0;

			/*
			 * If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
			 * We split 4-nodes along the search path.
			 */
			// find a parent
			while (current != null)
			{
				order = Comparer.Compare(value, current.Value);

				// duplicate values means nodes will never be balanced!
				if (order == 0)
				{
					// We could have changed root node to red during the search process. Root needs to be set to black.
					Root.Color = false;
					throw new DuplicateKeyException();
				}

				// split a 4-node into two 2-nodes
				if (Is4Node(current))
				{
					Split4Node(current);
					// We could have introduced two consecutive red nodes after split. Fix that by rotation.
					if (IsRed(parent) && grandParent != null) Balance(current, ref parent, grandParent, greatGrandParent);
				}

				greatGrandParent = grandParent;
				grandParent = parent;
				parent = current;
				current = order < 0
							? current.Left
							: current.Right;
			}

			Debug.Assert(parent != null, "No parent node found for the new node.");

			RedBlackNode<T> node = MakeNode(value);

			if (order < 0) parent.Left = node;
			else parent.Right = node;

			// the new node will be red, so we will need to adjust the colors if parent node is also red
			if (parent.Color && grandParent != null) Balance(node, ref parent, grandParent, greatGrandParent);
			// Root node is always black
			Root.Color = false;
			Count++;
			_version++;
		}

		public override bool Remove(T value)
		{
			if (Root == null) return false;

			/*
			 * Search for a node and then find its successor then copy the item from the successor
			 * to the matching node and delete the successor.
			 *
			 * If a node doesn't have a successor, we can replace it with its left child (if not empty.)
			 * or delete the matching node.
			 *
			 * In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
			 */
			RedBlackNode<T> current = Root,
							parent = null,
							grandParent = null,
							match = null,
							parentOfMatch = null;
			bool foundMatch = false;

			while (current != null)
			{
				if (Is2Node(current))
				{
					if (parent == null)
					{
						current.Color = true;
					}
					else
					{
						RedBlackNode<T> sibling = GetSibling(current, parent);

						if (sibling.Color)
						{
							/*
							 * If parent is a 3-node, flip the orientation of the red link.
							 * We can achieve this by a single rotation. This case is converted
							 * to one of other cased below.
							 */
							Debug.Assert(!parent.Color, "parent must be a black node!");

							if (ReferenceEquals(parent.Right, sibling)) RotateLeft(parent);
							else RotateRight(parent);

							parent.Color = true;
							sibling.Color = false;
							// sibling becomes child of grandParent or root after rotation. Update link from grandParent or root.
							ReplaceChildOfNodeOrRoot(grandParent, parent, sibling);
							// sibling will become grandParent of current node 
							grandParent = sibling;
							if (ReferenceEquals(parent, match)) parentOfMatch = sibling;
							// update sibling, this is necessary for following processing
							sibling = (ReferenceEquals(parent.Left, current)
										? parent.Right
										: parent.Left) ?? throw new Exception("Sibling must not be null!");
						}

						Debug.Assert(!sibling.Color, "Sibling must be black!");

						if (Is2Node(sibling))
						{
							Merge2Nodes(parent, current, sibling);
						}
						else
						{
							/*
							 * current is a 2-node and sibling is either a 3-node or a 4-node.
							 * We can change the color of current to red by some rotation.
							 */
							RedBlackNode<T> newGrandParent;
							Debug.Assert(IsRed(sibling.Left) || IsRed(sibling.Right), "sibling must have at least one red child");

							if (IsRed(sibling.Left))
							{
								if (ReferenceEquals(parent.Left, current))
								{
									// R L case
									Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Left.Color, "Left child of sibling must be red!");
									newGrandParent = RotateRightLeft(parent);
								}
								else
								{
									// R case
									Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Left.Color, "Left child of sibling must be red!");
									sibling.Left.Color = false;
									newGrandParent = RotateRight(parent);
								}
							}
							else
							{
								if (ReferenceEquals(parent.Left, current))
								{
									// L case
									Debug.Assert(parent.Right == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Right.Color, "Right child of sibling must be red!");
									sibling.Right.Color = false;
									newGrandParent = RotateLeft(parent);
								}
								else
								{
									// L R case
									Debug.Assert(parent.Left == sibling, "sibling must be left child of parent!");
									Debug.Assert(sibling.Right.Color, "Right child of sibling must be red!");
									newGrandParent = RotateLeftRight(parent);
								}
							}

							newGrandParent.Color = parent.Color;
							parent.Color = false;
							current.Color = true;
							ReplaceChildOfNodeOrRoot(grandParent, parent, newGrandParent);
							if (parent == match) parentOfMatch = newGrandParent;
						}
					}
				}

				// we don't need to compare any more once we found the match
				int order = foundMatch
								? -1
								: Comparer.Compare(value, current.Value);

				if (order == 0)
				{
					// save the matching node
					foundMatch = true;
					match = current;
					parentOfMatch = parent;
				}

				grandParent = parent;
				parent = current;
				// continue the search in right sub-tree after we find a match
				current = order < 0
							? current.Left
							: current.Right;
			}

			// move successor to the matching node position and replace links
			if (match != null)
			{
				ReplaceNode(match, parentOfMatch, parent, grandParent);
				Count--;
				_version++;
			}

			if (Root != null) Root.Color = false;
			return true;
		}

		/// <inheritdoc />
		public override T Minimum()
		{
			return Root == null
						? default(T)
						: Root.LeftMost().Value;
		}

		/// <inheritdoc />
		public override T Maximum()
		{
			return Root == null
						? default(T)
						: Root.RightMost().Value;
		}

		/// <summary>
		/// Validates the node and its children.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public override bool Validate(RedBlackNode<T> node)
		{
			if (node == null) return true;

			T previous = default(T);
			bool isValid = true, started = false;
			Iterate(node, e =>
			{
				if (!started)
				{
					started = true;
					previous = e.Value;
					return true;
				}

				isValid = Comparer.IsLessThan(previous, e.Value);
				return isValid;
			});
			return isValid;
		}

		public override bool IsBalanced()
		{
			if (Root == null || Root.IsLeaf) return true;
			
			// using levelOrder traversal
			// Root-Left-Right (Queue)
			Queue<RedBlackNode<T>> queue = new Queue<RedBlackNode<T>>(GetCapacityForQueueing(this));
			// Start at the root
			queue.Enqueue(Root);

			while (queue.Count > 0)
			{
				RedBlackNode<T> current = queue.Dequeue();

				// Queue the next nodes
				if (current.Left != null)
				{
					if (!IsBalancedLocal(current.Left, current)) return false;
					queue.Enqueue(current.Left);
				}

				if (current.Right != null)
				{
					if (!IsBalancedLocal(current.Right, current)) return false;
					queue.Enqueue(current.Right);
				}
			}
			return true;

			static bool IsBalancedLocal(RedBlackNode<T> node, RedBlackNode<T> parent)
			{
				if (node == null) return true;

				// node is red => its parent must be either null or black and all its direct children must be either null or black.
				if (node.Color)
				{
					return (parent == null || !parent.Color)
							&& !IsRed(node.Left)
							&& !IsRed(node.Right);
				}

				// node is black
				// has no red left
				return !IsRed(node.Left)
						// or left is red and all its direct children are either null or black
						|| !IsRed(node.Left.Left) && !IsRed(node.Left.Right)
					// and has no red right
					&& (!IsRed(node.Right)
						// or right is red and all its direct children are either null or black
						|| !IsRed(node.Right.Left) && !IsRed(node.Right.Right));
			}
		}

		private void Balance([NotNull] RedBlackNode<T> node, [NotNull] ref RedBlackNode<T> parent, [NotNull] RedBlackNode<T> grandParent, RedBlackNode<T> greatGrandParent)
		{
			/*
			* After calling Balance, we need to make sure current and parent up-to-date.
			* It doesn't matter if we keep grandParent and greatGrantParent up-to-date
			* because we won't need to split again in the next node.
			* By the time we need to split again, everything will be correctly set.
			*/
			bool parentIsRight = ReferenceEquals(grandParent.Right, parent);
			bool isRight = ReferenceEquals(parent.Right, node);

			if (parentIsRight == isRight)
			{
				// same orientation, single rotation
				node = isRight
							? RotateLeft(grandParent)
							: RotateRight(grandParent);
			}
			else
			{
				// different orientation, double rotation
				node = isRight
							? RotateLeftRight(grandParent)
							: RotateRightLeft(grandParent);
				// current node now becomes the child of greatGrandParent 
				parent = greatGrandParent;
			}

			// grandParent will become a child of either parent of current.
			grandParent.Color = true;
			node.Color = false;
			ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, node);
		}

		private void ReplaceChildOfNodeOrRoot(RedBlackNode<T> parent, RedBlackNode<T> child, RedBlackNode<T> newChild)
		{
			if (parent == null)
			{
				Root = newChild;
				return;
			}

			if (ReferenceEquals(parent.Left, child))
				parent.Left = newChild;
			else
				parent.Right = newChild;
		}

		private void ReplaceNode([NotNull] RedBlackNode<T> match, RedBlackNode<T> parentOfMatch, RedBlackNode<T> successor, RedBlackNode<T> parentOfSuccessor)
		{
			// Replace the matching node with its successor.
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
				Debug.Assert(successor.Right == null && successor.Color || IsRed(successor.Right) && !successor.Color, "Successor must be in valid state");
				if (successor.Right != null) successor.Right.Color = false;

				if (parentOfSuccessor != match)
				{
					// detach successor from its parent and set its right child
					parentOfSuccessor.Left = successor.Right;
					successor.Right = match.Right;
				}

				successor.Left = match.Left;
			}

			if (successor != null) successor.Color = match.Color;
			ReplaceChildOfNodeOrRoot(parentOfMatch, match, successor);
		}

		[NotNull]
		private static RedBlackNode<T> RotateLeft([NotNull] RedBlackNode<T> node)
		{
			RedBlackNode<T> newRoot = node.Right;
			node.Right = newRoot.Left;
			newRoot.Left = node;
			return newRoot;
		}

		[NotNull]
		private static RedBlackNode<T> RotateLeftRight([NotNull] RedBlackNode<T> node)
		{
			RedBlackNode<T> oldLeft = node.Left;
			RedBlackNode<T> newRoot = oldLeft.Right;

			node.Left = newRoot.Right;
			newRoot.Right = node;
			oldLeft.Right = newRoot.Left;
			newRoot.Left = oldLeft;
			return newRoot;
		}

		[NotNull]
		private static RedBlackNode<T> RotateRight([NotNull] RedBlackNode<T> node)
		{
			RedBlackNode<T> newRoot = node.Left;
			node.Left = newRoot.Right;
			newRoot.Right = node;
			return newRoot;
		}

		[NotNull]
		private static RedBlackNode<T> RotateRightLeft([NotNull] RedBlackNode<T> node)
		{
			RedBlackNode<T> oldRight = node.Right;
			RedBlackNode<T> newRoot = oldRight.Left;

			node.Right = newRoot.Left;
			newRoot.Left = node;
			oldRight.Left = newRoot.Right;
			newRoot.Right = oldRight;
			return newRoot;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static RedBlackNode<T> GetSibling([NotNull] RedBlackNode<T> node, [NotNull] RedBlackNode<T> parent)
		{
			return ReferenceEquals(parent.Left, node)
						? parent.Right
						: parent.Left;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool IsRed(RedBlackNode<T> node)
		{
			return node != null && node.Color;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool IsBlack(RedBlackNode<T> node)
		{
			return node != null && !node.Color;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool IsNullOrBlack(RedBlackNode<T> node)
		{
			return node == null || !node.Color;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool Is4Node(RedBlackNode<T> node)
		{
			return node != null && IsRed(node.Left) && IsRed(node.Right);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool Is2Node(RedBlackNode<T> node)
		{
			return IsBlack(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static void Split4Node([NotNull] RedBlackNode<T> node)
		{
			node.Color = true;
			if (node.Left != null) node.Left.Color = false;
			if (node.Right != null) node.Right.Color = false;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static void Merge2Nodes([NotNull] RedBlackNode<T> parent, [NotNull] RedBlackNode<T> child1, [NotNull] RedBlackNode<T> child2)
		{
			Debug.Assert(IsRed(parent), "parent must be be red");
			parent.Color = false;
			child1.Color = child2.Color = true;
		}
	}
}