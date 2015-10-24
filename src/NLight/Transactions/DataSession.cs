// Author(s): Sébastien Lorion

using System;
using System.Threading.Tasks;

namespace NLight.Transactions
{
	public abstract class DataSession
	{
		internal DataSession(TransactionContext context, string transactionGroupName, Func<DataSession, Task> commit, Func<DataSession, Task> rollback, Func<DataSession, Task> end)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (string.IsNullOrEmpty(transactionGroupName)) throw new ArgumentNullException(nameof(transactionGroupName));
			if (commit == null) throw new ArgumentNullException(nameof(commit));
			if (rollback == null) throw new ArgumentNullException(nameof(rollback));
			if (end == null) throw new ArgumentNullException(nameof(end));

			this.TransactionContext = context;
			this.TransactionGroupName = transactionGroupName;
			this.Commit = commit;
			this.Rollback = rollback;
			this.End = end;
		}

		/// <summary>
		/// Gets the associated transaction context.
		/// </summary>
		public TransactionContext TransactionContext { get; }

		/// <summary>
		/// Gets the associated transaction group name.
		/// </summary>
		public string TransactionGroupName { get; }

		/// <summary>
		/// Gets or sets the commit transaction action.
		/// </summary>
		internal Func<DataSession, Task> Commit { get; }

		/// <summary>
		/// Gets or sets the rollback transaction action.
		/// </summary>
		internal Func<DataSession, Task> Rollback { get; }

		/// <summary>
		/// Gets or sets the end connection action.
		/// </summary>
		internal Func<DataSession, Task> End { get; }
	}

	/// <summary>
	/// Represents a data session associated with a transaction context.
	/// </summary>
	public sealed class DataSession<T>
		: DataSession
	{
		internal DataSession(TransactionContext context, string transactionGroupName, T state, Func<DataSession<T>, Task> commit, Func<DataSession<T>, Task> rollback, Func<DataSession<T>, Task> end)
			: base(context, transactionGroupName, ds => commit((DataSession<T>) ds), ds => rollback((DataSession<T>) ds), ds => end((DataSession<T>) ds))
		{
			this.State = state;
		}

		/// <summary>
		/// Gets or sets the data session state.
		/// </summary>
		public T State { get; }
	}
}