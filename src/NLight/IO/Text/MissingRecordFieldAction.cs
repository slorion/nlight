// Author(s): Sébastien Lorion

namespace NLight.IO.Text
{
	/// <summary>
	/// Specifies the action to take when a field is missing.
	/// </summary>
	public enum MissingRecordFieldAction
	{
		/// <summary>
		/// Handles as a parsing error.
		/// </summary>
		HandleAsParseError = 0,

		/// <summary>
		/// Returns an empty value ("" for <see cref="string"/> or 0 for value types).
		/// </summary>
		ReturnEmptyValue = 1,

		/// <summary>
		/// Returns a <c>null</c> value. The type of the missing field must be nullable.
		/// </summary>
		ReturnNullValue = 2,
	}
}