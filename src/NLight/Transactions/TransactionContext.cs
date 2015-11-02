// Author(s): Sébastien Lorion

using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace NLight.Transactions
{
	/// <summary>
	/// Represents a transaction context.
	/// </summary>
	public class TransactionContext
		: IDisposable
	{
		private static readonly AsyncLocal<TransactionContext> _current = new AsyncLocal<TransactionContext>();

		/// <summary>
		/// Occurs when a new transaction context has been created.
		/// </summary>
		public static event EventHandler<TransactionContextCreatedEventArgs> Created;

		/// <summary>
		/// Occurs when a transaction context state has changed.
		/// </summary>
		public event EventHandler<TransactionContextStateChangedEventArgs> StateChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionContext"/> class.
		/// </summary>
		/// <param name="affinity">The transaction context affinity.</param>
		public TransactionContext(TransactionContextAffinity affinity)
		{
#if DEBUG
			_allocStack = new StackTrace();
#endif

			TransactionHandler.EnsureInitialize();

			this.Affinity = affinity;
			this.State = TransactionContextState.Created;
			this.StateFromChildren = TransactionContextState.ToBeCommitted;
			this.IsolationLevel = IsolationLevel.ReadCommitted;

			Created?.Invoke(this, new TransactionContextCreatedEventArgs(this));

			this.Enter();
		}

		/// <summary>
		/// Gets the current transaction context.
		/// </summary>
		public static TransactionContext CurrentTransactionContext
		{
			get { return _current.Value; }
			private set { _current.Value = value; }
		}

		/// <summary>
		/// Gets the affinity of the transaction context.
		/// </summary>
		public TransactionContextAffinity Affinity { get; }

		/// <summary>
		/// Gets or sets the transaction locking behavior for the connection.
		/// </summary>
		public IsolationLevel IsolationLevel { get; set; }

		/// <summary>
		/// Gets the current state of the transaction context.
		/// </summary>
		public TransactionContextState State { get; private set; }

		/// <summary>
		/// Gets or sets the state from children.
		/// </summary>
		private TransactionContextState StateFromChildren { get; set; }

		/// <summary>
		/// Gets the parent of the transaction context.
		/// </summary>
		public TransactionContext Parent { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is a controlling transaction context.
		/// </summary>
		public bool IsController => this.Affinity != TransactionContextAffinity.Required || this.Parent == null || this.Parent.Affinity == TransactionContextAffinity.NotSupported;

		/// <summary>
		/// Gets the controlling transaction context.
		/// </summary>
		/// <returns>The controlling transaction context.</returns>
		public TransactionContext GetController()
		{
			if (this.IsController)
				return this;
			else
			{
				Debug.Assert(this.Parent != null);
				return this.Parent.GetController();
			}
		}

		/// <summary>
		/// Raises the <see cref="StateChanged"/> event.
		/// </summary>
		/// <param name="previousState">The previous transaction context state.</param>
		/// <param name="newState">The new transaction context state.</param>
		private void OnStateChanged(TransactionContextState previousState, TransactionContextState newState)
		{
			this.StateChanged?.Invoke(this, new TransactionContextStateChangedEventArgs(previousState, newState));
		}

		/// <summary>
		/// Enters the transaction context. It will become the new current transaction context.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///	The requested transaction context state change is invalid.
		/// </exception>
		private void Enter()
		{
			if (this.State != TransactionContextState.Created && this.State != TransactionContextState.Exited)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Transactions_IncoherentContextTransition, this.State, TransactionContextState.Entered));

			this.Parent = CurrentTransactionContext;

			if (this.Parent != null && !this.IsController)
				this.Parent.StateChanged += Parent_TransactionContextStateChanged;

			CurrentTransactionContext = this;

			var previousState = this.State;
			this.State = TransactionContextState.Entered;
			OnStateChanged(previousState, TransactionContextState.Entered);
		}

		/// <summary>
		/// Votes commit for the currently executing operation. A rollback must not have been voted before.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///	The requested transaction context state change is invalid.
		/// </exception>
		public void VoteCommit()
		{
			if (this.State != TransactionContextState.Entered && this.State != TransactionContextState.ToBeCommitted)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Transactions_IncoherentContextTransition, this.State, TransactionContextState.ToBeCommitted));

			if (this.State == TransactionContextState.ToBeCommitted)
				return;

			var controlling = this.GetController();

			if (controlling != this)
				controlling.VoteCommitFromChild();

			var previousState = this.State;
			this.State = TransactionContextState.ToBeCommitted;
			OnStateChanged(previousState, TransactionContextState.ToBeCommitted);
		}

		/// <summary>
		/// Casts the commit vote on parent from a child.
		/// </summary>
		private void VoteCommitFromChild()
		{
			if (this.StateFromChildren == TransactionContextState.Entered)
				this.StateFromChildren = TransactionContextState.ToBeCommitted;
		}

		/// <summary>
		/// Votes rollback for the currently executing operation.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///	The requested transaction context state change is invalid.
		/// </exception>
		public void VoteRollback()
		{
			if (this.State != TransactionContextState.Entered && this.State != TransactionContextState.ToBeCommitted && this.State != TransactionContextState.ToBeRollbacked)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Transactions_IncoherentContextTransition, this.State, TransactionContextState.ToBeRollbacked));

			if (this.State == TransactionContextState.ToBeRollbacked)
				return;

			var controlling = this.GetController();

			if (controlling != this)
				controlling.VoteRollbackFromChild();

			var previousState = this.State;
			this.State = TransactionContextState.ToBeRollbacked;
			OnStateChanged(previousState, TransactionContextState.ToBeRollbacked);
		}

		/// <summary>
		/// Casts the rollback vote on parent from a child.
		/// </summary>
		private void VoteRollbackFromChild()
		{
			this.StateFromChildren = TransactionContextState.ToBeRollbacked;
		}

		/// <summary>
		/// Exits the transaction context. Its commit or rollback status becomes final.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///	The requested transaction context state change is invalid.
		/// </exception>
		public void Exit()
		{
			if (this.State == TransactionContextState.Created)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Transactions_IncoherentContextTransition, this.State, TransactionContextState.Exited));

			if (this.State == TransactionContextState.Exited)
				return;

			if (this.State == TransactionContextState.Entered)
				this.VoteRollback();
			else if (this.State == TransactionContextState.ToBeCommitted && this.StateFromChildren == TransactionContextState.ToBeRollbacked)
				this.VoteRollback();

			Debug.Assert(this.State == TransactionContextState.ToBeCommitted || this.State == TransactionContextState.ToBeRollbacked);

			if (this.Parent != null && !this.IsController)
				this.Parent.StateChanged -= Parent_TransactionContextStateChanged;

			CurrentTransactionContext = this.Parent;

			var previousState = this.State;
			this.State = TransactionContextState.Exited;
			OnStateChanged(previousState, TransactionContextState.Exited);
		}

		/// <summary>
		/// Handles the parent transaction context's <see cref="StateChanged"/> event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The <see cref="TransactionContextStateChangedEventArgs"/> that contains the event data.</param>
		private void Parent_TransactionContextStateChanged(object sender, TransactionContextStateChangedEventArgs e)
		{
			if (sender == null) throw new ArgumentNullException(nameof(sender));
			if (e == null) throw new ArgumentNullException(nameof(e));

			if (((TransactionContext) sender).State == TransactionContextState.Exited)
				Dispose();
		}

		#region IDisposable members

#pragma warning disable CS0649
		private readonly StackTrace _allocStackTrace;
#pragma warning restore CS0649

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> if this instance is disposed manually; <c>false</c> if it is disposed during finalization.</param>
		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (this.State != TransactionContextState.Exited && this.State != TransactionContextState.Created)
					this.Exit();
			}
			catch (Exception ex) when (!disposing)
			{
				Log.Source.TraceEvent(TraceEventType.Error, 0, Resources.LogMessages.Shared_ExceptionDuringFinalization, ex);
			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the instance is reclaimed by garbage collection.
		/// </summary>
		~TransactionContext()
		{
			Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
			Dispose(false);
		}

		#endregion
	}
}