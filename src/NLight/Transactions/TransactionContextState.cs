// Author(s): Sébastien Lorion

namespace NLight.Transactions
{
	/// <summary>
	/// Specifies the states in which a transaction context can be.
	/// </summary>
	public enum TransactionContextState
	{
		/// <summary>
		/// The transaction context has been created, but has not yet registered itself as current context.
		/// </summary>
		Created = 0,

		/// <summary>
		/// The transaction context is active.
		/// </summary>
		Entered = 1,

		/// <summary>
		/// The transaction context has been voted for commit.
		/// </summary>
		ToBeCommitted = 2,

		/// <summary>
		/// The transaction context has been voted for rollback.
		/// </summary>
		ToBeRollbacked = 3,

		/// <summary>
		/// The transaction context has exited.
		/// </summary>
		Exited = 4
	}
}