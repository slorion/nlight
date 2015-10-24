// Author(s): Sébastien Lorion

using NLight.Transactions;
using NUnit.Framework;
using System;

namespace NLight.Tests.Unit.Transactions
{
	[TestFixture]
	public static class TransactionContextStateTransitionsTest
	{
		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_Created_Entered(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				Assert.AreEqual(TransactionContextState.Entered, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_Entered_Exited(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.StateChanged +=
					(sender, e) =>
					{
						if (e.OldState == TransactionContextState.Entered)
						{
							Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);
							Assert.AreEqual(TransactionContextState.ToBeRollbacked, e.NewState);
						}
						else
						{
							Assert.AreEqual(TransactionContextState.Exited, context.State);
							Assert.AreEqual(TransactionContextState.ToBeRollbacked, e.OldState);
							Assert.AreEqual(TransactionContextState.Exited, e.NewState);
						}
					};

				context.Exit();
				Assert.AreEqual(TransactionContextState.Exited, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_Entered_ToBeCommitted(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteCommit();
				Assert.AreEqual(TransactionContextState.ToBeCommitted, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_ToBeCommitted_ToBeCommitted(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteCommit();
				Assert.AreEqual(TransactionContextState.ToBeCommitted, context.State);

				context.StateChanged +=
					(sender, e) =>
					{
						if (e.NewState == TransactionContextState.ToBeCommitted)
							Assert.Fail("Changed event should not be raised again");
					};

				context.VoteCommit();
				Assert.AreEqual(TransactionContextState.ToBeCommitted, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_ToBeCommitted_ToBeRollbacked(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteCommit();
				Assert.AreEqual(TransactionContextState.ToBeCommitted, context.State);

				context.VoteRollback();
				Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_ToBeRollbacked_ToBeRollbacked(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteRollback();
				Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);

				context.StateChanged +=
					(sender, e) =>
					{
						if (e.NewState == TransactionContextState.ToBeCommitted)
							Assert.Fail("Changed event should not be raised again");
					};

				context.VoteRollback();
				Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_ToBeCommitted_Exited(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteCommit();
				Assert.AreEqual(TransactionContextState.ToBeCommitted, context.State);

				context.Exit();
				Assert.AreEqual(TransactionContextState.Exited, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidStateTransitionTest_ToBeRollbacked_Exited(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.VoteRollback();
				Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);

				context.Exit();
				Assert.AreEqual(TransactionContextState.Exited, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void ValidValidStateTransitionTest_Exited_Exited(TransactionContextAffinity affinity)
		{
			using (var context = new TransactionContext(affinity))
			{
				context.Exit();
				Assert.AreEqual(TransactionContextState.Exited, context.State);

				context.StateChanged +=
					(sender, e) =>
					{
						if (e.NewState == TransactionContextState.Exited)
							Assert.Fail("Changed event should not be raised again");
					};

				context.Exit();
				Assert.AreEqual(TransactionContextState.Exited, context.State);
			}
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void InvalidStateTransitionTest_ToBeRollbacked_ToBeCommitted(TransactionContextAffinity affinity)
		{
			var ex = Assert.Throws<InvalidOperationException>(
				() =>
				{
					using (var context = new TransactionContext(affinity))
					{
						context.VoteRollback();
						Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);

						context.VoteCommit();
						Assert.AreEqual(TransactionContextState.ToBeRollbacked, context.State);
					}
				});
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void InvalidStateTransitionTest_Exited_ToBeCommitted(TransactionContextAffinity affinity)
		{
			var ex = Assert.Throws<InvalidOperationException>(
				() =>
				{
					using (var context = new TransactionContext(affinity))
					{
						context.Exit();
						Assert.AreEqual(TransactionContextState.Exited, context.State);

						context.VoteCommit();
					}
				});
		}

		[Test]
		[TestCaseSource(typeof(TestCaseFactory), "AffinitySource")]
		public static void InvalidStateTransitionTest_Exited_ToBeRollbacked(TransactionContextAffinity affinity)
		{
			var ex = Assert.Throws<InvalidOperationException>(
				() =>
				{
					using (var context = new TransactionContext(affinity))
					{
						context.Exit();
						Assert.AreEqual(TransactionContextState.Exited, context.State);

						context.VoteRollback();
					}
				});
		}
	}
}