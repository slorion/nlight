// Author(s): Sébastien Lorion

using NLight.Tests.Unit.BCL.Data.MockDataProvider;
using NLight.Transactions;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Tests.Unit.Transactions
{
	public class MockDataSource
		: IDisposable
	{
		private class DataSessionState
		{
			public bool ConnectionClosedByReader { get; set; }
			public DbConnection Connection { get; set; }
			public DbTransaction Transaction { get; set; }
		}

		public event EventHandler<EventArgs> CommandExecuted;

		private readonly Random _random = new Random();

		public MockDataSource(string id, string transactionGroupName, TimeSpan? maxSimulatedQueryDelay = null)
		{
			this.Id = id;
			this.TransactionGroupName = transactionGroupName;
			this.MaxSimulatedQueryDelay = maxSimulatedQueryDelay;
		}

		public string Id { get; }
		public string TransactionGroupName { get; }
		public TimeSpan? MaxSimulatedQueryDelay { get; }

		protected virtual void OnCommandExecuted(EventArgs e)
		{
			this.CommandExecuted?.Invoke(this, e);
		}

		public Task ExecuteNonQuery(DbCommand command)
		{
			return TransactionManager.Execute(this.TransactionGroupName, BeginSession, CommitSession, RollbackSession, EndSession,
				async session =>
				{
					if (this.MaxSimulatedQueryDelay != null)
						await Task.Delay(_random.Next(0, (int) this.MaxSimulatedQueryDelay.Value.TotalMilliseconds)).ConfigureAwait(false);

					return 0;
				}).ContinueWith(_ => OnCommandExecuted(EventArgs.Empty));
		}

		#region Transaction handling

		private static void WriteOperationTrace(DataSessionState state, string operation)
		{
			Trace.WriteLine(string.Format("{0},{1},{2}", state.GetHashCode(), operation, Thread.CurrentThread.ManagedThreadId), "NLight.Tests.Unit.Transactions.MockDataSource.operations");
		}

		private Task<DataSessionState> BeginSession(TransactionContext transactionContext)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

			return Task.Run(
				() =>
				{
					var state = new DataSessionState();
					WriteOperationTrace(state, "beginSession");

					WriteOperationTrace(state, "createConnection");
					state.Connection = new MockConnection { ConnectionString = $"MockDataSource.Id={this.Id},TransactionGroupName={this.TransactionGroupName}" };

					WriteOperationTrace(state, "openConnection");
					state.Connection.Open();

					if (transactionContext.Affinity != TransactionContextAffinity.NotSupported)
					{
						WriteOperationTrace(state, "beginTransaction");
						state.Transaction = state.Connection.BeginTransaction(transactionContext.IsolationLevel);
					}

					return state;
				});
		}

		private Task EndSession(DataSession<DataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(
				() =>
				{
					var state = session.State;

					WriteOperationTrace(state, "endSession");

					if (!state.ConnectionClosedByReader)
					{
						WriteOperationTrace(state, "closeConnection");

						state.Connection.Close();
						state.Connection.Dispose();
					}
				});
		}

		private Task CommitSession(DataSession<DataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(
				() =>
				{
					var state = session.State;

					WriteOperationTrace(state, "commitSession");
					WriteOperationTrace(state, "commitTransaction");

					state.Transaction.Commit();
				});
		}

		private Task RollbackSession(DataSession<DataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(
				() =>
				{
					var state = session.State;

					WriteOperationTrace(state, "rollbackSession");
					WriteOperationTrace(state, "rollbackTransaction");

					// catch any further error from DB
					// in particular the case where the transaction has already been aborted by the DB itself
					try
					{
						state.Transaction.Rollback();
					}
					catch (DbException ex)
					{
						Trace.WriteLine(ex, "NLight");
					}
				});
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}