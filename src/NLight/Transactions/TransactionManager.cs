// Author(s): Sébastien Lorion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Transactions
{
	/// <summary>
	/// Represents the transaction manager responsible for executing all transacted operations.
	/// </summary>
	public static class TransactionManager
	{
		private static readonly ConcurrentDictionary<TransactionContext, ConcurrentDictionary<string, Lazy<Task<DataSession>>>> _pending = new ConcurrentDictionary<TransactionContext, ConcurrentDictionary<string, Lazy<Task<DataSession>>>>();

		static TransactionManager()
		{
			TransactionContext.Created += OnTransactionContextCreated;
		}

		/// <summary>
		/// A call to this method before any transaction context creation is required to ensure that the transaction context creation event is hooked properly in a thread-safe manner.
		/// </summary>
		internal static void EnsureInitialize()
		{
		}

		/// <summary>
		/// Executes the specified operation inside the current transaction context. If there is no context, a new one will be created with a <see cref="TransactionContextAffinity.NotSupported"/> affinity.
		/// </summary>
		/// <typeparam name="TState">The type of the data session state.</typeparam>
		/// <typeparam name="TResult">The type of the operation result.</typeparam>
		/// <param name="transactionGroupName">The transaction group name.</param>
		/// <param name="begin">The action to execute when the transaction context begins (before any operation).</param>
		/// <param name="commit">The action to execute when committing the transaction context.</param>
		/// <param name="rollback">The action to execute when rollbacking the transaction context.</param>
		/// <param name="end">The action to execute when the transaction context ends (after all operations).</param>
		/// <param name="operation">The operation to execute inside the current transaction context.</param>
		/// <returns>The result of the operation.</returns>
		public static async Task<TResult> Execute<TState, TResult>(string transactionGroupName, Func<TransactionContext, Task<TState>> begin, Func<DataSession<TState>, Task> commit, Func<DataSession<TState>, Task> rollback, Func<DataSession<TState>, Task> end, Func<DataSession<TState>, Task<TResult>> operation)
		{
			if (string.IsNullOrEmpty(transactionGroupName)) throw new ArgumentNullException(nameof(transactionGroupName));
			if (begin == null) throw new ArgumentNullException(nameof(begin));
			if (commit == null) throw new ArgumentNullException(nameof(commit));
			if (rollback == null) throw new ArgumentNullException(nameof(rollback));
			if (end == null) throw new ArgumentNullException(nameof(end));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			var current = TransactionContext.CurrentTransactionContext;

			bool ownerOfCurrent = false;
			if (current == null)
			{
				current = new TransactionContext(TransactionContextAffinity.NotSupported);
				ownerOfCurrent = true;
			}

			if (current.State == TransactionContextState.Exited)
				throw new InvalidOperationException(string.Format(Resources.ExceptionMessages.Transactions_InvalidTransactionContextState, string.Join(",", new string[] { TransactionContextState.Entered.ToString(), TransactionContextState.ToBeCommitted.ToString(), TransactionContextState.ToBeRollbacked.ToString() })));

			if (current.Affinity == TransactionContextAffinity.NotSupported)
			{
				try
				{
					var state = await begin(current).ConfigureAwait(false);
					var session = new DataSession<TState>(current, transactionGroupName, state, commit, rollback, end);

					try
					{
						var result = await operation(session).ConfigureAwait(false);

						if (ownerOfCurrent)
							current.VoteCommit();

						return result;
					}
					finally
					{
						await end(session).ConfigureAwait(false);
					}
				}
				finally
				{
					if (ownerOfCurrent && current != null)
						current.Exit();
				}
			}
			else
			{
				var controller = current.GetController();
				Debug.Assert(controller != null);

				var sessionsByTransactionGroup = _pending.GetOrAdd(controller, new ConcurrentDictionary<string, Lazy<Task<DataSession>>>());
				var session = await sessionsByTransactionGroup.GetOrAdd(transactionGroupName,
					new Lazy<Task<DataSession>>(
						async () =>
						{
							// begin the session here so that it is guaranteed to run only once
							var state = await begin(current).ConfigureAwait(false);
							return new DataSession<TState>(current, transactionGroupName, state, commit, rollback, end);
						}, LazyThreadSafetyMode.ExecutionAndPublication)).Value.ConfigureAwait(false);

				return await operation((DataSession<TState>) session).ConfigureAwait(false);
			}
		}

		private static void OnTransactionContextCreated(object sender, TransactionContextCreatedEventArgs e)
		{
			if (sender == null) throw new ArgumentNullException(nameof(sender));
			if (e == null) throw new ArgumentNullException(nameof(e));

			if (e.NewTransactionContext.Affinity != TransactionContextAffinity.NotSupported)
				e.NewTransactionContext.StateChanged += OnTransactionContextStateChanged;
		}

		private static void OnTransactionContextStateChanged(object sender, TransactionContextStateChangedEventArgs e)
		{
			if (sender == null) throw new ArgumentNullException(nameof(sender));
			if (e == null) throw new ArgumentNullException(nameof(e));

			var context = (TransactionContext) sender;

			if (!context.IsController)
				return;

			if (e.NewState == TransactionContextState.Exited && (e.OldState == TransactionContextState.ToBeCommitted || e.OldState == TransactionContextState.ToBeRollbacked))
			{
				ConcurrentDictionary<string, Lazy<Task<DataSession>>> sessionsByTransactionGroup;
				if (_pending.TryRemove(context, out sessionsByTransactionGroup))
				{
					Task.Run(
						async () =>
						{
							// In the face of error(s), we try to go on and rollback as many data sessions as we can
							// then we throw an AggregateException encapsulating all exceptions that occurred.
							// Data sessions committed before the first error will be left as is
							// and must be handled by the application using a compensating action for example

							var exceptions = new ConcurrentBag<Exception>();

							var tasks = sessionsByTransactionGroup
								.Values
								.Where(ls => ls.IsValueCreated)
								.Select(
									async lazySession =>
									{
										var session = await lazySession.Value.ConfigureAwait(false);

										try
										{
											if (exceptions.Count > 0 || e.OldState == TransactionContextState.ToBeRollbacked)
												await session.Rollback(session).ConfigureAwait(false);
											else if (e.OldState == TransactionContextState.ToBeCommitted)
												await session.Commit(session).ConfigureAwait(false);
										}
										catch (Exception ex)
										{
											exceptions.Add(ex);
										}
										finally
										{
											try
											{
												await session.End(session).ConfigureAwait(false);
											}
											catch (Exception ex)
											{
												exceptions.Add(ex);
											}
										}
									});

							await Task.WhenAll(tasks).ConfigureAwait(false);

							if (exceptions.Count > 0)
								throw new AggregateException(exceptions);
						});
				}
			}
		}
	}
}