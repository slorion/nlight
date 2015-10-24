// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	/// <summary>
	/// Provides data for a record parsing error event.
	/// </summary>
	public class ParseErrorEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the ParseErrorEventArgs class.
		/// </summary>
		/// <param name="error">The error that occurred.</param>
		/// <param name="action">The action to take.</param>
		public ParseErrorEventArgs(MalformedRecordException error, ParseErrorAction action)
			: base()
		{
			this.Error = error;
			this.Action = action;
		}

		/// <summary>
		/// Gets the error that occurred.
		/// </summary>
		public MalformedRecordException Error { get; }

		/// <summary>
		/// Gets or sets the action to take.
		/// </summary>
		public ParseErrorAction Action { get; set; }
	}
}