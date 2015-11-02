// Author(s): Sébastien Lorion

using NLight.Transactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace NLight.Data.Db
{
	/// <summary>
	/// Contains the methods required to execute transacted operations against a database.
	/// See <see cref="TransactionManager"/>.
	/// </summary>
	public static class DbDataSource
	{
		/// <summary>
		/// Executes the specified operation.
		/// </summary>
		/// <typeparam name="T">The type of the operation result.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		/// <param name="operation">The operation to execute.</param>
		public static Task<T> Execute<T>(string transactionGroupName, Func<DbConnection> createConnection, Func<DataSession<DbDataSessionState>, Task<T>> operation)
		{
			if (createConnection == null) throw new ArgumentNullException(nameof(createConnection));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return TransactionManager.Execute(transactionGroupName, context => BeginSession(context, createConnection), CommitSession, RollbackSession, EndSession, operation);
		}

		//TODO: replace by or add support for IObservable<T> ?
		/// <summary>
		/// Executes the specified select operation.
		/// </summary>
		/// <typeparam name="T">The type of the records returned by the select operation.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <param name="recordConverter">The function that will convert the records to <typeparamref name="T"/>.</param>
		/// <returns>The records returned by the select operation.</returns>
		public static async Task<IEnumerable<T>> ExecuteSelect<T>(string transactionGroupName, Func<DbConnection> createConnection, Func<DataSession<DbDataSessionState>, Task<DbDataReader>> operation, Func<IDataRecord, T> recordConverter)
		{
			if (recordConverter == null) throw new ArgumentNullException(nameof(recordConverter));

			using (DbDataReader reader = await ExecuteSelect(transactionGroupName, createConnection, operation).ConfigureAwait(false))
				return ReadRecords(reader, recordConverter);
		}

		private static IEnumerable<T> ReadRecords<T>(DbDataReader reader, Func<IDataRecord, T> recordConverter)
		{
			while (reader.Read())
				yield return recordConverter(reader);
		}

		/// <summary>
		/// Executes the specified select operation.
		/// </summary>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <returns>The <see cref="DbDataReader"/> returned by the select operation.</returns>
		public static Task<DbDataReader> ExecuteSelect(string transactionGroupName, Func<DbConnection> createConnection, Func<DataSession<DbDataSessionState>, Task<DbDataReader>> operation)
		{
			if (createConnection == null) throw new ArgumentNullException(nameof(createConnection));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return Execute(transactionGroupName, createConnection,
				session =>
				{
					session.State.ConnectionHandledByReader = true;
					return operation(session);
				});
		}

		/// <summary>
		/// Executes the specified select operation.
		/// </summary>
		/// <typeparam name="T">The type of the records returned by the select operation.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <returns>The records returned by the select operation.</returns>
		public static Task<IEnumerable<T>> ExecuteSelect<T>(string transactionGroupName, Func<DbConnection> createConnection, Func<DataSession<DbDataSessionState>, Task<IEnumerable<T>>> operation)
		{
			if (createConnection == null) throw new ArgumentNullException(nameof(createConnection));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return Execute(transactionGroupName, createConnection,
				session =>
				{
					session.State.ConnectionHandledByReader = true;
					return operation(session);
				});
		}

		/// <summary>
		/// Execute the specified select operation.
		/// </summary>
		/// <typeparam name="T">The type of the value returned by the select operation.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <returns>The value returned by the select operation.</returns>
		public static Task<T> ExecuteScalar<T>(string transactionGroupName, Func<DbConnection> createConnection, Func<DataSession<DbDataSessionState>, Task<T>> operation)
		{
			if (createConnection == null) throw new ArgumentNullException(nameof(createConnection));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return Execute(transactionGroupName, createConnection, operation);
		}

		/// <summary>
		/// Begins the data session.
		/// </summary>
		/// <param name="transactionContext">The current transaction context.</param>
		/// <param name="createConnection">The function that will create the database connection.</param>
		private static async Task<DbDataSessionState> BeginSession(TransactionContext transactionContext, Func<DbConnection> createConnection)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (createConnection == null) throw new ArgumentNullException(nameof(createConnection));

			var state = new DbDataSessionState();

			state.Connection = createConnection();
			await state.Connection.OpenAsync().ConfigureAwait(false);

			if (transactionContext.Affinity != TransactionContextAffinity.NotSupported)
				state.Transaction = state.Connection.BeginTransaction(transactionContext.IsolationLevel);

			return state;
		}

		/// <summary>
		/// Commits the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task CommitSession(DataSession<DbDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(() => session.State.Transaction.Commit());
		}

		/// <summary>
		/// Rollbacks the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task RollbackSession(DataSession<DbDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(() => session.State.Transaction.Rollback());
		}

		/// <summary>
		/// Ends the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task EndSession(DataSession<DbDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			if (session.State.ConnectionHandledByReader)
				return Task.CompletedTask;
			else
				return Task.Run(
					() =>
					{
						session.State.Connection.Close();
						session.State.Connection.Dispose();
					});
		}
	}
}