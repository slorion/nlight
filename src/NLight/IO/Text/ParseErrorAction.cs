// Author(s): Sébastien Lorion

namespace NLight.IO.Text
{
	/// <summary>
	/// Specifies the action to take when a parsing error has occurred.
	/// </summary>
	public enum ParseErrorAction
	{
		/// <summary>
		/// Raises a parsing error event.
		/// </summary>
		RaiseEvent = 0,

		/// <summary>
		/// Tries to advance to next line.
		/// </summary>
		SkipToNextLine = 1,

		/// <summary>
		/// Throws an exception.
		/// </summary>
		ThrowException = 2,
	}
}