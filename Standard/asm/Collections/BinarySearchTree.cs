using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Patterns.Collections;
using asm.Patterns.Direction;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><strong>Binary search tree implementation.</strong></para>
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
	[Serializable]
	public sealed class BinarySearchTree<T> : LinkedBinaryTree<T>
	{
		private const int BALANCE_FACTOR = 1;

		/// <inheritdoc />
		public BinarySearchTree()
			: this(Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(T value)
			: this(value, Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(T value, IComparer<T> comparer)
			: base(comparer)
		{
			Add(value);
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(comparer)
		{
			foreach (T value in collection) 
				Add(value);
		}

		/// <inheritdoc />
		internal BinarySearchTree(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override Node FindNearestLeaf(T value)
		{
			Node parent = null, next = Root;

			while (next != null)
			{
				parent = next;
				int cmp = Comparer.Compare(value, next.Value);
				next = cmp == 0
							? null
							: cmp < 0
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
				// no parent means there is no root currently
				Root = new Node(value);
				Count++;
				_version++;
				return;
			}

			// find a parent
			Node parent = Root, next = Root;
			Stack<Node> stack = new Stack<Node>();

			while (next != null)
			{
				parent = next;
				stack.Push(parent);
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			Node node = new Node(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			// update nodes
			while (stack.Count > 0)
			{
				parent = stack.Pop();
				SetHeight(parent);
			}

			Count++;
			_version++;
		}

		/// <inheritdoc />
		public override bool Remove(Node node)
		{
			Node parent = node.Parent;
			Node child, leftMostParent = null;

			// case 1: node has no right child
			if (node.Right == null)
			{
				child = node.Left;
			}
			// case 2: node has a right child which doesn't have a left child
			else if (node.Right.Left == null)
			{
				// move the left to the right child's left
				node.Right.Left = node.Left;
				child = node.Right;
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				Node leftmost = node.Right.Minimum();
				// move the left-most right to the parent's left
				leftMostParent = leftmost.Parent;
				leftMostParent.Left = leftmost.Right;
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				leftmost.Right = node.Right;
				child = leftmost;
			}

			if (parent == null)
			{
				if (child != null) child.Parent = null;
				Root = child;
			}
			else if (Comparer.IsLessThan(node.Value, parent.Value))
			{
				// if node < parent, move the left to the parent's left
				parent.Left = child;
			}
			else
			{
				// else, move the left to the parent's right
				parent.Right = child;
			}

			Node update = child != null
							? leftMostParent ?? child
							: parent;

			// update nodes
			while (update != null)
			{
				SetHeight(update);
				update = update.Parent;
			}

			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public override bool Validate(Node node)
		{
			if (node == null) return true;

			bool isValid = true;
			Iterate(node, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
			{
				if (e.IsLeft)
				{
					if (Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Value)) isValid = false;
					if (isValid && e.Parent.IsRight && Comparer.IsLessThan(e.Value, e.Parent.Parent.Value)) isValid = false;
				}

				if (isValid && e.IsRight)
				{
					if (Comparer.IsLessThan(e.Value, e.Parent.Value)) isValid = false;
					if (isValid && e.Parent.IsLeft && Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Parent.Value)) isValid = false;
				}

				return isValid;
			});
			return isValid;
		}

		/// <inheritdoc />
		public override bool IsBalanced(Node node)
		{
			return node == null
					|| node.IsLeaf
					|| Math.Abs(node.BalanceFactor) <= BALANCE_FACTOR
					&& (node.Left == null 
						|| node.Left.IsLeaf 
						|| Math.Abs(node.Left.BalanceFactor) <= BALANCE_FACTOR)
					&& (node.Right == null 
						|| node.Right.IsLeaf 
						|| Math.Abs(node.Right.BalanceFactor) <= BALANCE_FACTOR);
		}

		/// <summary>
		/// <inheritdoc />
		/// <para><strong>This works the same as the <see cref="AVLTree{T}"/></strong></para>
		/// </summary>
		public override void Balance()
		{
			if (Root == null || Root.IsLeaf) return;

			// find all unbalanced nodes
			Queue<Node> unbalancedNodes = new Queue<Node>();
			Iterate(Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
			{
				if (IsBalanced(e)) return;
				unbalancedNodes.Enqueue(e);
			});

			while (unbalancedNodes.Count > 0)
			{
				Node node = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(node)) continue;
				Balance(node);
			}
		}

		private void Balance(Node node)
		{
			/*
			 * balance the tree
			 * There are 4 cases for unbalanced nodes
			 * 1. Left-Left Case: y is left child of z and x is left child of y
			 *
			 *	       z                                      y 
			 *	      / \                                   /   \
			 *	     y   T4      Right Rotate(z) ->        x      z
			 *	    / \                                  /  \    /  \ 
			 *	   x   T3                               T1  T2  T3  T4
			 *	  / \
			 *	T1   T2
			 *
			 * 2. Left-Right Case: y is left child of z and x is right child of y
			 *
			 *	     z                               z                           x
			 *	    / \                            /   \                        /  \ 
			 *	   y   T4  Left Rotate(y) ->      x    T4  Right Rotate(z) -> y     z
			 *	  / \                            /  \                        / \   / \
			 *	T1   x                          y   T3                     T1  T2 T3  T4
			 *	    / \                        / \
			 *	  T2   T3                    T1   T2
			 *
			 * 3. Right-Right Case: y is right child of z and x is right child of y
			 *
			 *	  z                                y
			 *	 /  \                            /   \ 
			 *	T1   y     Left Rotate(z)       z      x
			 *	    /  \   - - - - - - - ->    / \    / \
			 *	   T2   x                     T1  T2 T3  T4
			 *	       / \
			 *	     T3  T4
			 *
			 * 4. Right-Left Case: y is right child of z and x is left child of y
			 *
			 *	   z                            z                            x
			 *	  / \                          / \                          /  \ 
			 *	T1   y   Right Rotate (y)    T1   x      Left Rotate(z)   z      y
			 *	    / \  - - - - - - - - ->     /  \   - - - - - - - ->  / \    / \
			 *	   x   T4                      T2   y                  T1  T2  T3  T4
			 *	  / \                              /  \
			 *	T2   T3                           T3   T4
			 */
			if (node == null || IsBalanced(node)) return;

			Queue<Node> queue = new Queue<Node>();
			queue.Enqueue(node);

			while (queue.Count > 0)
			{
				node = queue.Dequeue();
				if (Math.Abs(node.BalanceFactor) <= BALANCE_FACTOR) continue;

				Node parent = node.Parent;
				bool changed = false;

				if (node.BalanceFactor > 1) // left heavy
				{
					if (node.Left.BalanceFactor < 0)
					{
						// duplicate values can make life miserable for us here because it will never be balanced!
						if (Comparer.IsEqual(node.Value, node.Left.Value)) throw new DuplicateKeyException();
						RotateLeft(node.Left);
					}

					node = RotateRight(node);
					changed = true;
				}
				else if (node.BalanceFactor < -1) // right heavy
				{
					if (node.Right.BalanceFactor > 0) RotateRight(node.Right);
					// duplicate values can make life miserable for us here because it will never be balanced!
					if (Comparer.IsEqual(node.Value, node.Right.Value)) throw new DuplicateKeyException();
					node = RotateLeft(node);
					changed = true;
				}

				if (!changed) continue;

				if (!IsBalanced(node)) queue.Enqueue(node);
				if (node.Left != null && !IsBalanced(node.Left)) queue.Enqueue(node.Left);
				if (node.Right != null && !IsBalanced(node.Right)) queue.Enqueue(node.Right);

				Node update = parent;

				// update parents and find unbalanced ones
				while (update != null)
				{
					SetHeight(update);
					if (!IsBalanced(update)) queue.Enqueue(update);
					update = update.Parent;
				}
			}
		}
	}

	public static class BinarySearchTreeExtension
	{
		/// <summary>
		/// Fill a <see cref="BinarySearchTree{T}"/> from the LevelOrder <see cref="collection"/>.
		/// <para>
		/// LevelOrder => Root-Left-Right (Queue)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="collection"></param>
		public static void FromLevelOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		{
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(thisValue, list)) return;

			int index = 0;
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(list[index++]);
			thisValue.Root = node;

			IComparer<T> comparer = thisValue.Comparer;
			Queue<BinarySearchTree<T>.Node> queue = new Queue<BinarySearchTree<T>.Node>();
			queue.Enqueue(node);

			// not all queued items will be parents, it's expected the queue will contain enough nodes
			while (index < list.Count)
			{
				int oldIndex = index;
				BinarySearchTree<T>.Node root = queue.Dequeue();

				// add left node
				if (comparer.IsLessThan(list[index], root.Value))
				{
					node = new BinarySearchTree<T>.Node(list[index]);
					root.Left = node;
					queue.Enqueue(node);
					index++;
				}

				// add right node
				if (index < list.Count && comparer.IsGreaterThanOrEqual(list[index], root.Value))
				{
					node = new BinarySearchTree<T>.Node(list[index]);
					root.Right = node;
					queue.Enqueue(node);
					index++;
				}

				if (oldIndex == index) index++;
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from the PreOrder <see cref="collection"/>.
		/// <para>
		/// PreOrder => Root-Left-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="collection"></param>
		public static void FromPreOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		{
			// https://www.geeksforgeeks.org/construct-bst-from-given-preorder-traversal-set-2/
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(thisValue, list)) return;

			// first node of PreOrder will be root of tree
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(list[0]);
			thisValue.Root = node;

			Stack<BinarySearchTree<T>.Node> stack = new Stack<BinarySearchTree<T>.Node>();
			// Push root of the BST to the stack i.e, first element of the array.
			stack.Push(node);

			/*
			 * Keep popping nodes while the stack is not empty.
			 * When the value is greater than stack’s top value, make it the right
			 * child of the last popped node and push it to the stack.
			 * If the next value is less than the stack’s top value, make it the left
			 * child of the stack’s top node and push it to the stack.
			 */
			IComparer<T> comparer = thisValue.Comparer;

			// Traverse from second node
			for (int i = 1; i < list.Count; i++)
			{
				BinarySearchTree<T>.Node root = null;

				// Keep popping nodes while top of stack is greater.
				while (stack.Count > 0 && comparer.IsGreaterThan(list[i], stack.Peek()))
					root = stack.Pop();

				node = new BinarySearchTree<T>.Node(list[i]);

				if (root != null) root.Right = node;
				else if (stack.Count > 0) stack.Peek().Left = node;

				stack.Push(node);
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from the InOrder <see cref="collection"/>.
		/// <para>
		/// Note that it is not possible to construct a unique binary tree from InOrder collection alone.
		/// </para>
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="collection"></param>
		public static void FromInOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		{
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(thisValue, list)) return;

			int start = 0;
			int end = list.Count - 1;
			int index = IndexMid(start, end);
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(list[index]);
			thisValue.Root = node;

			Queue<(int Index, int Start, int End, BinarySearchTree<T>.Node Node)> queue = new Queue<(int Index, int Start, int End, BinarySearchTree<T>.Node Node)>();
			queue.Enqueue((index, start, end, node));

			while (queue.Count > 0)
			{
				(int Index, int Start, int End, BinarySearchTree<T>.Node Node) tuple = queue.Dequeue();

				// get the next left index
				start = tuple.Start;
				end = tuple.Index - 1;
				int nodeIndex = IndexMid(start, end);

				// add left node
				if (nodeIndex > -1)
				{
					node = new BinarySearchTree<T>.Node(list[nodeIndex]);
					tuple.Node.Left = node;
					queue.Enqueue((nodeIndex, start, end, node));
				}

				// get the next right index
				start = tuple.Index + 1;
				end = tuple.End;
				nodeIndex = IndexMid(start, end);

				// add right node
				if (nodeIndex > -1)
				{
					node = new BinarySearchTree<T>.Node(list[nodeIndex]);
					tuple.Node.Right = node;
					queue.Enqueue((nodeIndex, start, end, node));
				}
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);

			static int IndexMid(int start, int end)
			{
				return start > end
							? -1
							: start + (end - start) / 2;
			}
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from the PostOrder <see cref="collection"/>.
		/// <para>
		/// PostOrder => Left-Right-Root (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="collection"></param>
		public static void FromPostOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		{
			// https://www.geeksforgeeks.org/construct-a-bst-from-given-postorder-traversal-using-stack/
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(thisValue, list)) return;

			// last node of PostOrder will be root of tree
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(list[list.Count - 1]);
			thisValue.Root = node;

			Stack<BinarySearchTree<T>.Node> stack = new Stack<BinarySearchTree<T>.Node>();
			// Push root of the BST to the stack i.e, last element of the array.
			stack.Push(node);

			/*
			 * The idea is to traverse the array in reverse.
			 * If next element is > the element at the top of the stack then,
			 * set this element as the right child of the element at the top
			 * of the stack and also push it to the stack.
			 * Else if, next element is < the element at the top of the stack then,
			 * start popping all the elements from the stack until either the stack
			 * is empty or the current element becomes > the element at the top of
			 * the stack.
			 * Make this element left child of the last popped node and repeat until
			 * the array is traversed completely.
			 */
			IComparer<T> comparer = thisValue.Comparer;

			// Traverse from second last node
			for (int i = list.Count - 2; i >= 0; i--)
			{
				BinarySearchTree<T>.Node root = null;

				// Keep popping nodes while top of stack is greater.
				while (stack.Count > 0 && comparer.IsLessThan(list[i], stack.Peek()))
					root = stack.Pop();

				node = new BinarySearchTree<T>.Node(list[i]);

				if (root != null) root.Left = node;
				else if (stack.Count > 0) stack.Peek().Right = node;

				stack.Push(node);
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="levelOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// LevelOrder => Root-Left-Right (Queue)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="inOrderCollection"></param>
		/// <param name="levelOrderCollection"></param>
		public static void FromInOrderAndLevelOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> levelOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			// Root-Left-Right
			IReadOnlyList<T> levelOrder = new Lister<T>(levelOrderCollection);
			if (inOrder.Count != levelOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
			if (levelOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], levelOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
			// try simple cases first
			if (FromSimpleList(thisValue, levelOrder)) return;

			/*
			 * using the facts that:
			 * 1. LevelOrder is organized in the form Root-Left-Right.
			 * 2. InOrder is organized in the form Left-Root-Right,
			 * from 1 and 2, the LevelOrder list can be used to identify
			 * the root and other elements locations in the InOrder list.
			 */
			// the lookup will enhance the speed of looking for the index of the item to O(1)
			IDictionary<T, int> lookup = new Dictionary<T, int>(thisValue.Comparer.AsEqualityComparer());

			// add all InOrder items to the lookup
			for (int i = 0; i < inOrder.Count; i++)
			{
				T key = inOrder[i];
				if (lookup.ContainsKey(key)) continue;
				lookup.Add(key, i);
			}

			int index = 0;
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(levelOrder[index++]);
			thisValue.Root = node;

			Queue<(int Start, int End, BinarySearchTree<T>.Node Node)> queue = new Queue<(int Start, int End, BinarySearchTree<T>.Node Node)>();
			queue.Enqueue((0, inOrder.Count - 1, node));

			while (index < levelOrder.Count && queue.Count > 0)
			{
				(int Start, int End, BinarySearchTree<T>.Node Node) tuple = queue.Dequeue();

				// get the root index (the current node index in the InOrder collection)
				int rootIndex = lookup[tuple.Node.Value];
				// find out the index of the next entry of LevelOrder in the InOrder collection
				int levelIndex = lookup[levelOrder[index]];

				// add left node
				if (levelIndex >= tuple.Start && levelIndex <= rootIndex - 1)
				{
					node = new BinarySearchTree<T>.Node(inOrder[levelIndex]);
					tuple.Node.Left = node;
					queue.Enqueue((tuple.Start, rootIndex - 1, node));
					index++;
					// index and node changed, so will need to get the next entry of LevelOrder in the InOrder collection
					levelIndex = index < levelOrder.Count
									? lookup[levelOrder[index]]
									: -1;
				}

				// add right node
				if (levelIndex >= rootIndex + 1 && levelIndex <= tuple.End)
				{
					node = new BinarySearchTree<T>.Node(inOrder[levelIndex]);
					tuple.Node.Right = node;
					queue.Enqueue((rootIndex + 1, tuple.End, node));
					index++;
				}
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="preOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// PreOrder => Root-Left-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="inOrderCollection"></param>
		/// <param name="preOrderCollection"></param>
		public static void FromInOrderAndPreOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> preOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			IReadOnlyList<T> preOrder = new Lister<T>(preOrderCollection);
			if (inOrder.Count != preOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
			if (preOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], preOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
			// try simple cases first
			if (FromSimpleList(thisValue, preOrder)) return;

			/*
			 * https://stackoverflow.com/questions/48352513/construct-binary-tree-given-its-inorder-and-preorder-traversals-without-recursio#48364040
			 * 
			 * The idea is to keep tree nodes in a stack from PreOrder traversal, till their counterpart is not found in InOrder traversal.
			 * Once a counterpart is found, all children in the left sub-tree of the node must have been already visited.
			 */
			int preIndex = 0;
			int inIndex = 0;
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(preOrder[preIndex++]);
			thisValue.Root = node;

			IComparer<T> comparer = thisValue.Comparer;
			Stack<BinarySearchTree<T>.Node> stack = new Stack<BinarySearchTree<T>.Node>();
			stack.Push(node);

			while (stack.Count > 0)
			{
				BinarySearchTree<T>.Node root = stack.Peek();

				if (comparer.IsEqual(root.Value, inOrder[inIndex]))
				{
					stack.Pop();
					inIndex++;

					// if all the elements in inOrder have been visited, we are done
					if (inIndex == inOrder.Count) break;
					// if there are still some unvisited nodes in the left, skip
					if (stack.Count > 0 && comparer.IsEqual(stack.Peek().Value, inOrder[inIndex])) continue;

					/*
					 * As top node in stack, still has not encountered its counterpart
					 * in inOrder, so next element in preOrder must be right child of
					 * the removed node
					 */
					node = new BinarySearchTree<T>.Node(preOrder[preIndex++]);
					root.Right = node;
					stack.Push(node);
				}
				else
				{
					/*
					 * Top node in the stack has not encountered its counterpart
					 * in inOrder, so next element in preOrder must be left child
					 * of this node
					 */
					node = new BinarySearchTree<T>.Node(preOrder[preIndex++]);
					root.Left = node;
					stack.Push(node);
				}
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		/// <summary>
		/// Constructs a <see cref="BinarySearchTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="postOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// PostOrder => Left-Right-Root (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="inOrderCollection"></param>
		/// <param name="postOrderCollection"></param>
		public static void FromInOrderAndPostOrder<T>([NotNull] this BinarySearchTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> postOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			IReadOnlyList<T> postOrder = new Lister<T>(postOrderCollection);
			if (inOrder.Count != postOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
			if (postOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], postOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
			if (FromSimpleList(thisValue, postOrder)) return;

			// the lookup will enhance the speed of looking for the index of the item to O(1)
			IDictionary<T, int> lookup = new Dictionary<T, int>(thisValue.Comparer.AsEqualityComparer());

			// add all InOrder items to the lookup
			for (int i = 0; i < inOrder.Count; i++)
			{
				T key = inOrder[i];
				if (lookup.ContainsKey(key)) continue;
				lookup.Add(key, i);
			}

			// Traverse postOrder in reverse
			int postIndex = postOrder.Count - 1;
			BinarySearchTree<T>.Node node = new BinarySearchTree<T>.Node(postOrder[postIndex--]);
			thisValue.Root = node;

			Stack<BinarySearchTree<T>.Node> stack = new Stack<BinarySearchTree<T>.Node>();
			// Push root of the BST to the stack i.e, last element of the array.
			stack.Push(node);

			IComparer<T> comparer = thisValue.Comparer;

			while (postIndex >= 0 && stack.Count > 0)
			{
				BinarySearchTree<T>.Node root = stack.Peek();
				// get the root index (the current node index in the InOrder collection)
				int rootIndex = lookup[root.Value];
				// find out the index of the next entry of PostOrder in the InOrder collection
				int index = lookup[postOrder[postIndex]];

				// add right node
				if (index > rootIndex)
				{
					node = new BinarySearchTree<T>.Node(inOrder[index]);
					root.Right = node;
					stack.Push(node);
					postIndex--;

					// index and node changed, so will need to get the next entry of PostOrder in the InOrder collection
					index = postIndex > -1
								? lookup[postOrder[postIndex]]
								: -1;
				}

				// add left node
				if (index > -1 && index < rootIndex)
				{
					if (comparer.IsLessThan(postOrder[postIndex], root.Value))
					{
						// Keep popping nodes while top of stack is greater.
						while (stack.Count > 0 && comparer.IsLessThan(postOrder[postIndex], stack.Peek().Value))
							root = stack.Pop();
					}

					node = new BinarySearchTree<T>.Node(inOrder[index]);
					root.Left = node;
					stack.Push(node);
					postIndex--;
				}
			}

			thisValue.Iterate(thisValue.Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, LinkedBinaryTree<T>.SetHeight);
		}

		private static bool FromSimpleList<T>([NotNull] BinarySearchTree<T> tree, [NotNull] IReadOnlyList<T> list)
		{
			bool result;
			tree.Clear();

			switch (list.Count)
			{
				case 0:
					result = true;
					break;
				case 1:
					tree.Root = new BinarySearchTree<T>.Node(list[0]);
					result = true;
					break;
				default:
					result = false;
					break;
			}

			return result;
		}

		private static void ThrowNotFormingATree(string collection1Name, string collection2Name)
		{
			throw new ArgumentException($"{collection1Name} and {collection2Name} do not form a binary tree.");
		}
	}
}