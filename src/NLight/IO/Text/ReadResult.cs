// Author(s): Sébastien Lorion

namespace NLight.IO.Text
{
	/// <summary>
	/// Represents the result of a read operation.
	/// </summary>
	public enum ReadResult
	{
		/// <summary>
		/// The read operation was successful.
		/// </summary>
		Success = 0,

		/// <summary>
		/// No data has been read because the end of the file is reached.
		/// </summary>
		EndOfFile = 1,

		/// <summary>
		/// A parsing error occurred.
		/// </summary>
		ParseError = 2
	}
}