// Author(s): Sébastien Lorion

namespace NLight.Transactions
{
	/// <summary>
	/// Specifies the transaction context affinities.
	/// </summary>
	public enum TransactionContextAffinity
	{
		/// <summary>
		/// Creates the transaction context with no governing transaction.
		/// </summary>
		NotSupported = 0,

		/// <summary>
		/// Shares a transaction, if one exists, otherwise creates a new transaction.
		/// </summary>
		Required = 1,

		/// <summary>
		/// Creates the transaction context with a new transaction, regardless of the state of the current context.
		/// </summary>
		RequiresNew = 2
	}
}