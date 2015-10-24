// Author(s): Sébastien Lorion

using NLight.Collections.Trees;
using NUnit.Framework;
using System.Linq;

namespace NLight.Tests.Unit.Collections.Trees
{
	public class BinaryTreeTraversalsTests
	{
		private static BinaryNode[][] _trees = new BinaryNode[][] {
			new BinaryNode[] {
				new BinaryNode(data:0,  left:null, right:null, parent:null)
			},
			new BinaryNode[] {
				new BinaryNode(data:0,  left:1,    right:null, parent:null),
				new BinaryNode(data:1,  left:null, right:null, parent:0)
			},
			new BinaryNode[] {
				new BinaryNode(data:0,  left:null, right:1,    parent:null),
				new BinaryNode(data:1,  left:null, right:null, parent:0)
			},
			new BinaryNode[] {
				new BinaryNode(data:0,  left:1,    right:2,    parent:null),
				new BinaryNode(data:1,  left:null, right:null, parent:0),
				new BinaryNode(data:2,  left:null, right:null, parent:0)
			},
			new BinaryNode[] {
				new BinaryNode(data:0,  left:1,    right:6,    parent:null),
				new BinaryNode(data:1,  left:2,    right:3,    parent:0),
				new BinaryNode(data:2,  left:null, right:null, parent:1),
				new BinaryNode(data:3,  left:4,    right:5,    parent:1),
				new BinaryNode(data:4,  left:null, right:null, parent:3),
				new BinaryNode(data:5,  left:null, right:null, parent:3),
				new BinaryNode(data:6,  left:null, right:7,    parent:0),
				new BinaryNode(data:7,  left:8,    right:null, parent:6),
				new BinaryNode(data:8,  left:null, right:null, parent:7)
			},
			new BinaryNode[] {
				new BinaryNode(data:0,  left:1,    right:5,    parent:null),
				new BinaryNode(data:1,  left:2,    right:4,    parent:0),
				new BinaryNode(data:2,  left:3,    right:null, parent:1),
				new BinaryNode(data:3,  left:null, right:null, parent:2),
				new BinaryNode(data:4,  left:null, right:null, parent:1),
				new BinaryNode(data:5,  left:6,    right:7,    parent:0),
				new BinaryNode(data:6,  left:null, right:null, parent:5),
				new BinaryNode(data:7,  left:8,    right:9,    parent:5),
				new BinaryNode(data:8,  left:null, right:null, parent:7),
				new BinaryNode(data:9,  left:null, right:10,   parent:7),
				new BinaryNode(data:10, left:null, right:null, parent:9)
			}
		};

		[TestCase(0, "0")]
		[TestCase(1, "0,1")]
		[TestCase(2, "0,1")]
		[TestCase(3, "0,1,2")]
		[TestCase(4, "0,1,2,3,4,5,6,7,8")]
		[TestCase(5, "0,1,2,3,4,5,6,7,8,9,10")]
		public void StatelessPreOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.StatelessPreOrder(tree[0], tree.GetLeft, tree.GetRight, tree.GetParent).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "0,1")]
		[TestCase(2, "0,1")]
		[TestCase(3, "0,1,2")]
		[TestCase(4, "0,1,2,3,4,5,6,7,8")]
		[TestCase(5, "0,1,2,3,4,5,6,7,8,9,10")]
		public void PreOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.PreOrder(tree[0], tree.GetLeft, tree.GetRight).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "1,0")]
		[TestCase(2, "0,1")]
		[TestCase(3, "1,0,2")]
		[TestCase(4, "2,1,4,3,5,0,6,8,7")]
		[TestCase(5, "3,2,1,4,0,6,5,8,7,9,10")]
		public void StatelessInOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.StatelessInOrder(tree[0], tree.GetLeft, tree.GetRight, tree.GetParent).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "1,0")]
		[TestCase(2, "0,1")]
		[TestCase(3, "1,0,2")]
		[TestCase(4, "2,1,4,3,5,0,6,8,7")]
		[TestCase(5, "3,2,1,4,0,6,5,8,7,9,10")]
		public void InOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.InOrder(tree[0], tree.GetLeft, tree.GetRight).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "1,0")]
		[TestCase(2, "1,0")]
		[TestCase(3, "1,2,0")]
		[TestCase(4, "2,4,5,3,1,8,7,6,0")]
		[TestCase(5, "3,2,4,1,6,8,10,9,7,5,0")]
		public void StatelessPostOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.StatelessPostOrder(tree[0], tree.GetLeft, tree.GetRight, tree.GetParent).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "1,0")]
		[TestCase(2, "1,0")]
		[TestCase(3, "1,2,0")]
		[TestCase(4, "2,4,5,3,1,8,7,6,0")]
		[TestCase(5, "3,2,4,1,6,8,10,9,7,5,0")]
		public void PostOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.PostOrder(tree[0], tree.GetLeft, tree.GetRight).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "0,1")]
		[TestCase(2, "0,1")]
		[TestCase(3, "0,1,2")]
		[TestCase(4, "0,1,6,2,3,7,4,5,8")]
		[TestCase(5, "0,1,5,2,4,6,7,3,8,9,10")]
		public void LevelOrderTest(int treeIndex, string expected)
		{
			BinaryNode[] tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.LevelOrder(tree[0], tree.GetLeft, tree.GetRight).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}
	}
}