// Author(s): Sébastien Lorion

using System.Data.Common;

namespace NLight.Data.Db
{
	/// <summary>
	/// Contains the data session state for a database.
	/// </summary>
	public class DbDataSessionState
	{
		/// <summary>
		/// Gets a value indicating whether the database connection will be closed by a data reader.
		/// </summary>
		internal bool ConnectionHandledByReader { get; set; }

		/// <summary>
		/// Gets the database connection.
		/// </summary>
		public DbConnection Connection { get; internal set; }

		/// <summary>
		/// Gets the database transaction.
		/// </summary>
		public DbTransaction Transaction { get; internal set; }
	}
}
