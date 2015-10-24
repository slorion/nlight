// Author(s): Sébastien Lorion

using NLight.Collections.Trees;
using NUnit.Framework;
using System.Linq;

namespace NLight.Tests.Unit.Collections.Trees
{
	public class NaryTreeTraversalsTests
	{
		//TODO: add real n-ary tests

		private static NaryNode[][] _trees = new NaryNode[][] {
			new NaryNode[] {
				new NaryNode(0)
			},
			new NaryNode[] {
				new NaryNode(0, 1),
				new NaryNode(1)
			},
			new NaryNode[] {
				new NaryNode(0, 1, 2),
				new NaryNode(1),
				new NaryNode(2)
			},
			new NaryNode[] {
				new NaryNode(0, 1, 6),
				new NaryNode(1, 2, 3),
				new NaryNode(2),
				new NaryNode(3, 4, 5),
				new NaryNode(4),
				new NaryNode(5),
				new NaryNode(6, 7),
				new NaryNode(7, 8),
				new NaryNode(8)
			},
			new NaryNode[] {
				new NaryNode(0, 1, 5),
				new NaryNode(1, 2, 4),
				new NaryNode(2, 3),
				new NaryNode(3),
				new NaryNode(4),
				new NaryNode(5, 6, 7),
				new NaryNode(6),
				new NaryNode(7, 8, 9),
				new NaryNode(8),
				new NaryNode(9, 10),
				new NaryNode(10)
			}
		};

		[TestCase(0, "0")]
		[TestCase(1, "0,1")]
		[TestCase(2, "0,1,2")]
		[TestCase(3, "0,1,2,3,4,5,6,7,8")]
		[TestCase(4, "0,1,2,3,4,5,6,7,8,9,10")]
		public void PreOrderTest(int treeIndex, string expected)
		{
			var tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.PreOrder(tree[0], tree.GetChildren).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "1,0")]
		[TestCase(2, "1,2,0")]
		[TestCase(3, "2,4,5,3,1,8,7,6,0")]
		[TestCase(4, "3,2,4,1,6,8,10,9,7,5,0")]
		public void PostOrderTest(int treeIndex, string expected)
		{
			var tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.PostOrder(tree[0], tree.GetChildren).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}

		[TestCase(0, "0")]
		[TestCase(1, "0,1")]
		[TestCase(2, "0,1,2")]
		[TestCase(3, "0,1,6,2,3,7,4,5,8")]
		[TestCase(4, "0,1,5,2,4,6,7,3,8,9,10")]
		public void LevelOrderTest(int treeIndex, string expected)
		{
			var tree = _trees[treeIndex];
			string result = string.Join(",", Traversals.LevelOrder(tree[0], tree.GetChildren).Select(node => node.ToString()));
			Assert.AreEqual(expected, result);
		}
	}
}