// Author(s): Sébastien Lorion

using NLight.Transactions;
using NUnit.Framework;
using System;
using System.Data;

namespace NLight.Tests.Unit.Transactions
{
	public class TransactionContextTest
	{
		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public void CreateTransactionContextTest(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				Assert.AreEqual(affinity, context.Affinity);
				Assert.AreEqual(TransactionContextState.Entered, context.State);
				Assert.AreEqual(IsolationLevel.ReadCommitted, context.IsolationLevel);
			}
		}
	}
}