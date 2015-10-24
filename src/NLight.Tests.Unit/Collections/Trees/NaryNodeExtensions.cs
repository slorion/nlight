// Author(s): Sébastien Lorion

using System.Collections.Generic;
using System.Linq;

namespace NLight.Tests.Unit.Collections.Trees
{
	internal static class NaryNodeExtensions
	{
		public static IEnumerable<NaryNode> GetChildren(this IList<NaryNode> tree, NaryNode node) { return node.Children.Select(c => tree[c]); }
	}
}