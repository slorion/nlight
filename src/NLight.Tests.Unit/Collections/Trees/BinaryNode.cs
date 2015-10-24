// Author(s): Sébastien Lorion

using System;

namespace NLight.Tests.Unit.Collections.Trees
{
	internal class BinaryNode
	{
		public BinaryNode(object data, int? left, int? right, int? parent)
		{
			this.Data = data;
			this.Left = left;
			this.Right = right;
			this.Parent = parent;
		}

		public object Data { get; private set; }

		public int? Left { get; private set; }
		public int? Right { get; private set; }
		public int? Parent { get; private set; }

		public override string ToString()
		{
			return Convert.ToString(this.Data);
		}
	}
}