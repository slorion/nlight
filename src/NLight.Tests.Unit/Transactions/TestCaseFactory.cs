// Author(s): Sébastien Lorion

using NLight.Core;
using NLight.Tests.Unit.BCL.Data.MockDataProvider;
using NLight.Transactions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NLight.Tests.Unit.Transactions
{
	public static class TestCaseFactory
	{
		public static IEnumerable<TransactionContextAffinity> AffinitySource() => EnumHelper.GetValues<TransactionContextAffinity>();

		public static IEnumerable<TestCaseData> NestedContextCaseSource()
		{
			TimeSpan maxQueryDelay = TimeSpan.FromMilliseconds(100);

			Func<TransactionContextTestNode, Task> operation =
				async node =>
				{
					using (var ds = new MockDataSource($"ds-{node.Id}", $"tx-{node.Id}", maxQueryDelay))
					{
						await ds.ExecuteNonQuery(new MockDataCommand()).ConfigureAwait(false);
					}
				};

			foreach (var parentAffinity in EnumHelper.GetValues<TransactionContextAffinity>())
				foreach (var childAffinity in EnumHelper.GetValues<TransactionContextAffinity>())
					foreach (var parentVote in EnumHelper.GetValues<VoteAction>())
						foreach (var childVote in EnumHelper.GetValues<VoteAction>())
							yield return new TestCaseData(
								new TransactionContextTestNode { Id = "parent", Affinity = parentAffinity, VoteAction = parentVote, Operation = operation }
									.AddChild(new TransactionContextTestNode { Id = "child", Affinity = childAffinity, VoteAction = childVote, Operation = operation }));
		}
	}
}