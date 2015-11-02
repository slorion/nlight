// Author(s): Sébastien Lorion

using NLight.Collections.Trees;
using NLight.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLight.Tests.Unit.Transactions
{
	public class TransactionContextTestNode
	{
		private readonly List<TransactionContextTestNode> _children = new List<TransactionContextTestNode>();

		public string Id { get; set; }
		public TransactionContextTestNode Parent { get; private set; }
		public IEnumerable<TransactionContextTestNode> Children => _children;

		public TransactionContextAffinity Affinity { get; set; }
		public VoteAction VoteAction { get; set; }
		public Func<TransactionContextTestNode, Task> Operation { get; set; }

		public bool IsController => this.Affinity != TransactionContextAffinity.Required || this.Parent == null || this.Parent.Affinity == TransactionContextAffinity.NotSupported;

		public TransactionContextTestNode GetController()
		{
			if (this.IsController)
				return this;
			else
				return this.Parent.GetController();
		}

		public TransactionContextState GetExpectedCommitState()
		{
			if (Traversals.LevelOrder(this.GetController(), node => node.Children.Where(child => !child.IsController)).All(node => node.VoteAction == VoteAction.VoteCommit))
				return TransactionContextState.ToBeCommitted;
			else
				return TransactionContextState.ToBeRollbacked;
		}

		public Task ExecuteOperation()
		{
			return this.Operation?.Invoke(this) ?? Task.CompletedTask;
		}

		public TransactionContextTestNode AddChild(TransactionContextTestNode child)
		{
			child.Parent = this;
			_children.Add(child);

			return this;
		}

		public override string ToString()
		{
			return string.Join(" | ", Traversals.PreOrder(this, node => node.Children).Select(node => $"{{Id={node.Id},Affinity={node.Affinity},VoteAction={node.VoteAction}}}"));
		}
	}
}