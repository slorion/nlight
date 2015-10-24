// Author(s): Sébastien Lorion

using System;

namespace NLight.Transactions
{
	/// <summary>
	/// Provides data for the <see cref="TransactionContext.StateChanged"/> event.
	/// </summary>
	public class TransactionContextStateChangedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionContextStateChangedEventArgs"/> class.
		/// </summary>
		/// <param name="oldState">The old transaction context state.</param>
		/// <param name="newState">The new transaction context state.</param>
		public TransactionContextStateChangedEventArgs(TransactionContextState oldState, TransactionContextState newState)
			: base()
		{
			this.OldState = oldState;
			this.NewState = newState;
		}

		/// <summary>
		/// Gets the old transaction context state.
		/// </summary>
		public TransactionContextState OldState { get; internal set; }

		/// <summary>
		/// Gets the new transaction context state.
		/// </summary>
		public TransactionContextState NewState { get; internal set; }
	}
}