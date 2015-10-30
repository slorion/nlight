// Author(s): Sébastien Lorion

using NLight.Tests.Unit.BCL.Data.MockDataProvider;
using NLight.Transactions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NLight.Tests.Unit.Transactions
{
	public static class TransactionHandlerTest
	{
		[Test]
		public static async Task SimpleTest()
		{
			const string TraceFileName = "Transactions.TransactionHandlerTest.SimpleTest.log.csv";

			try
			{
				Trace.AutoFlush = true;
				Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(TraceFileName, false)));

				using (var dataSource = new MockDataSource("ds", "tx"))
				using (var context = new TransactionContext(TransactionContextAffinity.RequiresNew))
				{
					var command = new MockDataCommand();
					await dataSource.ExecuteNonQuery(command).ConfigureAwait(false);

					context.VoteCommit();
				}
			}
			finally
			{
				Trace.Close();
			}

			CheckTraceLog(TraceFileName);
		}

		[Test]
		public static async Task NoContextTest()
		{
			const string TraceFileName = "Transactions.TransactionHandlerTest.NoContextTest.log.csv";

			try
			{
				Trace.AutoFlush = true;
				Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(TraceFileName, false)));

				using (var dataSource = new MockDataSource("ds", "tx"))
				{
					try
					{
						TransactionContext.Created += NoContextTest_TransactionContextCreated;

						var command = new MockDataCommand();
						await dataSource.ExecuteNonQuery(command).ConfigureAwait(false);
					}
					finally
					{
						TransactionContext.Created -= NoContextTest_TransactionContextCreated;
					}
				}

				Assert.IsNull(TransactionContext.CurrentTransactionContext);
			}
			finally
			{
				Trace.Close();
			}

			CheckTraceLog(TraceFileName);
		}

		private static void NoContextTest_TransactionContextCreated(object sender, TransactionContextCreatedEventArgs e)
		{
			Assert.IsNull(TransactionContext.CurrentTransactionContext);

			var context = e.NewTransactionContext;
			Assert.IsNotNull(context);

			Assert.IsNull(context.Parent);
			Assert.AreEqual(TransactionContextAffinity.NotSupported, context.Affinity);
			Assert.AreEqual(IsolationLevel.ReadCommitted, context.IsolationLevel);
			Assert.AreEqual(TransactionContextState.Created, context.State);

			context.StateChanged +=
				(s2, e2) =>
				{
					if (e2.NewState == TransactionContextState.Entered)
						Assert.AreEqual(TransactionContext.CurrentTransactionContext, s2);
					else if (e2.NewState == TransactionContextState.Exited)
						Assert.AreEqual(TransactionContextState.ToBeCommitted, e2.OldState);
				};
		}

		[Test]
		[RequiresThread]
		[TestCaseSource(typeof(TestCaseFactory), "NestedContextCaseSource")]
		public static async Task NestedContextTest(TransactionContextTestNode testNode)
		{
			const int IterationCount = 50;
			const string TraceFileName = "Transactions.TransactionHandlerTest.NestedContextTest.log.csv";

			try
			{
				Trace.AutoFlush = true;
				Trace.UseGlobalLock = true;
				Trace.Listeners.Add(new TextWriterTraceListener(new StreamWriter(TraceFileName, false)));

				Trace.WriteLine(testNode.ToString(), "NLight.Tests.Unit.Transactions.TransactionHandlerTest");

				var tasks = new List<Task>();
				for (int i = 0; i < IterationCount; i++)
					tasks.Add(Task.Factory.StartNew(() => ExecuteNode(testNode), TaskCreationOptions.DenyChildAttach).Unwrap());

				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
			finally
			{
				Trace.Close();
			}

			CheckTraceLog(TraceFileName);
		}

		private static async Task ExecuteNode(TransactionContextTestNode node)
		{
			if (node.Parent == null)
				Assert.That(TransactionContext.CurrentTransactionContext, Is.Null);

			var tcs = new TaskCompletionSource<TransactionContextState>();

			using (var tx = new TransactionContext(node.Affinity))
			{
				Assert.That(TransactionContext.CurrentTransactionContext, Is.EqualTo(tx));
				Assert.That(tx.IsController, Is.EqualTo(node.IsController));

				if (node.IsController)
				{
					tx.StateChanged +=
						(s, e) =>
						{
							if (e.NewState == TransactionContextState.Exited)
								tcs.SetResult(e.OldState);
						};
				}

				await node.ExecuteOperation().ConfigureAwait(false);

				if (node.Children != null)
				{
					foreach (var child in node.Children)
						await ExecuteNode(child).ConfigureAwait(false);
				}

				if (node.VoteAction == VoteAction.VoteCommit)
					tx.VoteCommit();
				else if (node.VoteAction == VoteAction.VoteRollback)
					tx.VoteRollback();
			}

			if (node.Parent == null)
				Assert.That(TransactionContext.CurrentTransactionContext, Is.Null);

			if (node.IsController)
			{
				var actualCommitState = await tcs.Task.ConfigureAwait(false);
				Assert.That(actualCommitState, Is.EqualTo(node.GetExpectedCommitState()));
			}
		}

		private static void CheckTraceLog(string path)
		{
			var sessions = new Dictionary<string, string>();

			var correctOrder = new string[] {
				"beginSession",
				"createConnection",
				"openConnection",
				"beginTransaction",
				"executeSession",
				"commitSession",
				"commitTransaction",
				"rollbackSession",
				"rollbackTransaction",
				"endSession",
				"closeConnection"
			};

			int indexOfExecuteSession = Array.IndexOf(correctOrder, "executeSession");

			foreach (var line in File.ReadAllLines(path))
			{
				string[] lineValues = line.Split(',');

				string key = lineValues[0];
				if (string.IsNullOrEmpty(key) || !key.StartsWith("NLight.Data.DB.DataSource.operations"))
					continue;

				string newState = lineValues[1];

				string oldState;
				if (!sessions.TryGetValue(key, out oldState))
				{
					if (Array.IndexOf(correctOrder, newState) < 0) throw new Exception("invalid state");
					else if (newState != "beginSession") throw new Exception("session not began correctly");

					sessions[key] = newState;

					oldState = newState;
				}
				else
				{
					int oldIndex = Array.IndexOf(correctOrder, oldState);
					int newIndex = Array.IndexOf(correctOrder, newState);

					if (newIndex < 0) throw new Exception("invalid state");
					else if (newIndex == 0) throw new Exception("session began 2 times");
					else if (newIndex == oldIndex && newIndex == indexOfExecuteSession) { } // we can execute many operations inside the same data session
					else if (newIndex <= oldIndex) throw new Exception(string.Format("bad order ({0} -> {1})", oldState, newState));

					sessions[key] = newState;
				}
			}

			string last = correctOrder[correctOrder.Length - 1];
			foreach (var item in sessions)
			{
				if (item.Value != last)
					throw new Exception(string.Format("session {0} not closed properly", item.Key));
			}
		}
	}
}