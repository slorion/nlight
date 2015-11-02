// Author(s): Sébastien Lorion

using NLight.Transactions;
using System;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace NLight.Data.Linq
{
	/// <summary>
	/// Contains the methods required to execute transacted operations against a LINQ <see cref="DataContext"/>.
	/// See <see cref="TransactionManager"/>.
	/// </summary>
	public static class LinqDataSource
	{
		/// <summary>
		/// Executes the specified operation.
		/// </summary>
		/// <typeparam name="T">The type of the operation result.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createDataContext">The function that will create the LINQ <see cref="DataContext"/>.</param>
		/// <param name="operation">The operation to execute.</param>
		public static Task<T> Execute<T>(string transactionGroupName, Func<DataContext> createDataContext, Func<DataSession<LinqDataSessionState>, Task<T>> operation)
		{
			if (createDataContext == null) throw new ArgumentNullException(nameof(createDataContext));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return TransactionManager.Execute(transactionGroupName, session => BeginSession(session, createDataContext), CommitSession, RollbackSession, EndSession, operation);
		}

		/// <summary>
		/// Executes the specified select operation.
		/// </summary>
		/// <typeparam name="T">The type of the records returned by the select operation.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createDataContext">The function that will create the LINQ <see cref="DataContext"/>.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <returns>The records returned by the select operation.</returns>
		public static Task<IQueryable<T>> ExecuteSelect<T>(string transactionGroupName, Func<DataContext> createDataContext, Func<DataSession, Task<IQueryable<T>>> operation)
		{
			if (createDataContext == null) throw new ArgumentNullException(nameof(createDataContext));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return Execute(transactionGroupName, createDataContext,
				async session =>
				{
					session.State.ConnectionHandledByReader = true;
					var queryable = await operation(session).ConfigureAwait(false);

					return (IQueryable<T>) new ConnectionAwareQueryable<T>(queryable, session);
				});
		}

		/// <summary>
		/// Execute the specified select operation.
		/// </summary>
		/// <typeparam name="T">The type of the value returned by the select operation.</typeparam>
		/// <param name="transactionGroupName">The name of the transaction group.</param>
		/// <param name="createDataContext">The function that will create the LINQ <see cref="DataContext"/>.</param>
		/// <param name="operation">The operation to execute.</param>
		/// <returns>The value returned by the select operation.</returns>
		public static Task<T> ExecuteScalar<T>(string transactionGroupName, Func<DataContext> createDataContext, Func<DataSession, Task<T>> operation)
		{
			if (createDataContext == null) throw new ArgumentNullException(nameof(createDataContext));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			return Execute(transactionGroupName, createDataContext, operation);
		}

		/// <summary>
		/// Begins the data session.
		/// </summary>
		/// <param name="transactionContext">The current transaction context.</param>
		/// <param name="createDataContext">The function that will create the LINQ <see cref="DataContext"/>.</param>
		private static async Task<LinqDataSessionState> BeginSession(TransactionContext transactionContext, Func<DataContext> createDataContext)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (createDataContext == null) throw new ArgumentNullException(nameof(createDataContext));

			var state = new LinqDataSessionState { Context = createDataContext() };
			await state.Context.Connection.OpenAsync().ConfigureAwait(false);

			if (transactionContext.Affinity != TransactionContextAffinity.NotSupported)
				state.Context.Transaction = state.Context.Connection.BeginTransaction(transactionContext.IsolationLevel);

			return state;
		}

		/// <summary>
		/// Commits the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task CommitSession(DataSession<LinqDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(
				() =>
				{
					var context = session.State.Context;
					context.Transaction.Commit();
				});
		}

		/// <summary>
		/// Rollbacks the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task RollbackSession(DataSession<LinqDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			return Task.Run(
				() =>
				{
					var context = session.State.Context;
					context.Transaction.Rollback();
				});
		}

		/// <summary>
		/// Ends the data session.
		/// </summary>
		/// <param name="session">The data session.</param>
		private static Task EndSession(DataSession<LinqDataSessionState> session)
		{
			if (session == null) throw new ArgumentNullException(nameof(session));

			var state = session.State;
			if (state.ConnectionHandledByReader)
				return Task.CompletedTask;
			else
				return Task.Run(() => state.Context.Dispose());
		}
	}
}