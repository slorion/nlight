// Author(s): Sébastien Lorion

using System;

namespace NLight.Transactions
{
	/// <summary>
	/// Provides data for the <see cref="TransactionContext.Created"/> event.
	/// </summary>
	public class TransactionContextCreatedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionContextCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public TransactionContextCreatedEventArgs(TransactionContext context)
			: base()
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.NewTransactionContext = context;
		}

		/// <summary>
		/// Gets the newly created transaction context.
		/// </summary>
		public TransactionContext NewTransactionContext { get; internal set; }
	}
}