// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Linq;

namespace NLight.Tests.Unit.Collections.Trees
{
	internal class NaryNode
	{
		public NaryNode(object data, params int[] children)
		{
			this.Data = data;
			this.Children = children ?? Enumerable.Empty<int>();
		}

		public object Data { get; private set; }
		public IEnumerable<int> Children { get; private set; }

		public override string ToString()
		{
			return Convert.ToString(this.Data);
		}
	}
}