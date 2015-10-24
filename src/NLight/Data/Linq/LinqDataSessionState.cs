// Author(s): Sébastien Lorion

using System.Data.Linq;

namespace NLight.Data.Linq
{
	/// <summary>
	/// Contains the data session state for a LINQ to SQL operation.
	/// </summary>
	public class LinqDataSessionState
	{
		/// <summary>
		/// Gets or sets a value indicating whether the connection is handled by the reader.
		/// </summary>
		internal bool ConnectionHandledByReader { get; set; }

		/// <summary>
		/// Gets the LINQ <see cref="DataContext"/>.
		/// </summary>
		public DataContext Context { get; internal set; }
	}
}