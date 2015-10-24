// Author(s): Sébastien Lorion

using System.Collections.Generic;

namespace NLight.Tests.Unit.Collections.Trees
{
	internal static class BinaryNodeExtensions
	{
		public static BinaryNode GetLeft(this IList<BinaryNode> tree, BinaryNode node) { return node.Left == null ? null : tree[node.Left.Value]; }
		public static BinaryNode GetRight(this IList<BinaryNode> tree, BinaryNode node) { return node.Right == null ? null : tree[node.Right.Value]; }
		public static BinaryNode GetParent(this IList<BinaryNode> tree, BinaryNode node) { return node.Parent == null ? null : tree[node.Parent.Value]; }
	}
}