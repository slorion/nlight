// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Linq;

namespace NLight.Collections.Trees
{
	public static partial class Traversals
	{
		public static IEnumerable<T> PreOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getLeftChild == null) throw new ArgumentNullException(nameof(getLeftChild));
			if (getRightChild == null) throw new ArgumentNullException(nameof(getRightChild));

			var stack = new Stack<T>();
			stack.Push(root);

			while (stack.Count > 0)
			{
				T current = stack.Pop();
				yield return current;

				T right = getRightChild(current);
				if (right != null)
					stack.Push(right);

				T left = getLeftChild(current);
				if (left != null)
					stack.Push(left);
			}
		}

		public static IEnumerable<T> PreOrder<T>(T root, Func<T, IEnumerable<T>> getChildren)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

			var stack = new Stack<T>();
			stack.Push(root);

			while (stack.Count > 0)
			{
				T current = stack.Pop();
				yield return current;

				foreach (var child in getChildren(current).Reverse())
					stack.Push(child);
			}
		}

		public static IEnumerable<T> InOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getLeftChild == null) throw new ArgumentNullException(nameof(getLeftChild));
			if (getRightChild == null) throw new ArgumentNullException(nameof(getRightChild));

			var stack = new Stack<T>();
			var current = root;

			while (current != null || stack.Count > 0)
			{
				if (current == null)
				{
					current = stack.Pop();
					yield return current;
					current = getRightChild(current);
				}
				else
				{
					stack.Push(current);
					current = getLeftChild(current);
				}
			}
		}

		//TODO: InOrder useful for n-ary tree ?

		public static IEnumerable<T> PostOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getLeftChild == null) throw new ArgumentNullException(nameof(getLeftChild));
			if (getRightChild == null) throw new ArgumentNullException(nameof(getRightChild));

			var stack = new Stack<T>();
			var current = root;

			while (current != null || stack.Count > 0)
			{
				if (current == null)
				{
					while (stack.Count > 0 && current == getRightChild(stack.Peek()))
					{
						current = stack.Pop();
						yield return current;
					}

					current = stack.Count == 0 ? null : getRightChild(stack.Peek());
				}
				else
				{
					stack.Push(current);
					current = getLeftChild(current);
				}
			}
		}

		public static IEnumerable<T> PostOrder<T>(T root, Func<T, IEnumerable<T>> getChildren)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

			//TODO: is there a way to do n-ary post-order without using 2 stacks ?

			var stack = new Stack<T>();
			var output = new Stack<T>();

			stack.Push(root);

			while (stack.Count > 0)
			{
				var current = stack.Peek();
				output.Push(current);
				stack.Pop();

				foreach (var child in getChildren(current))
					stack.Push(child);
			}

			while (output.Count > 0)
				yield return output.Pop();
		}

		public static IEnumerable<T> LevelOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getLeftChild == null) throw new ArgumentNullException(nameof(getLeftChild));
			if (getRightChild == null) throw new ArgumentNullException(nameof(getRightChild));

			var queue = new Queue<T>();
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				T current = queue.Dequeue();
				yield return current;

				T left = getLeftChild(current);
				if (left != null)
					queue.Enqueue(left);

				T right = getRightChild(current);
				if (right != null)
					queue.Enqueue(right);
			}
		}

		public static IEnumerable<T> LevelOrder<T>(T root, Func<T, IEnumerable<T>> getChildren)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

			var queue = new Queue<T>();
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				T current = queue.Dequeue();
				yield return current;

				foreach (var child in getChildren(current))
					queue.Enqueue(child);
			}
		}

		public static IEnumerable<T> StatelessPreOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild, Func<T, T> getParent)
			where T : class
		{
			return TraverseTree(root, getLeftChild, getRightChild, getParent, TraversalKind.PreOrder);
		}

		public static IEnumerable<T> StatelessInOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild, Func<T, T> getParent)
			where T : class
		{
			return TraverseTree(root, getLeftChild, getRightChild, getParent, TraversalKind.InOrder);
		}

		public static IEnumerable<T> StatelessPostOrder<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild, Func<T, T> getParent)
			where T : class
		{
			return TraverseTree(root, getLeftChild, getRightChild, getParent, TraversalKind.PostOrder);
		}

		private static IEnumerable<T> TraverseTree<T>(T root, Func<T, T> getLeftChild, Func<T, T> getRightChild, Func<T, T> getParent, TraversalKind traversalKind)
			where T : class
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getLeftChild == null) throw new ArgumentNullException(nameof(getLeftChild));
			if (getRightChild == null) throw new ArgumentNullException(nameof(getRightChild));
			if (getParent == null) throw new ArgumentNullException(nameof(getParent));

			T previous = getParent(root);
			T current = root;

			while (current != null)
			{
				if (previous == getParent(current))
				{
					if (traversalKind == TraversalKind.PreOrder)
						yield return current;

					T left = getLeftChild(current);

					if (left == null)
						previous = left;
					else
					{
						previous = current;
						current = left;
						continue;
					}
				}

				if (previous == getLeftChild(current))
				{
					if (traversalKind == TraversalKind.InOrder)
						yield return current;

					T right = getRightChild(current);

					if (right == null)
						previous = right;
					else
					{
						previous = current;
						current = right;
						continue;
					}
				}

				if (previous == getRightChild(current))
				{
					if (traversalKind == TraversalKind.PostOrder)
						yield return current;

					previous = current;
					current = getParent(current);
				}
			}
		}
	}
}