// Author(s): Sébastien Lorion

using System;
using System.Runtime.Serialization;

namespace NLight.IO.Text
{
	/// <summary>
	/// Represents the exception that is thrown when there is a missing field in a record.
	/// </summary>
	/// <remarks>
	/// MissingFieldException would have been a better name, but there is already a <see cref="System.MissingFieldException"/>.
	/// </remarks>
	[Serializable]
	public class MissingRecordFieldException
		: MalformedRecordException
	{
		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class.
		/// </summary>
		public MissingRecordFieldException()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public MissingRecordFieldException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public MissingRecordFieldException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class.
		/// </summary>
		/// <param name="rawData">The raw data when the error occurred.</param>
		/// <param name="currentPosition">The current position in the raw data.</param>
		/// <param name="currentRecordIndex">The current record index.</param>
		/// <param name="currentFieldIndex">The current field index.</param>
		public MissingRecordFieldException(string rawData, int currentPosition, long currentRecordIndex, int currentFieldIndex)
			: base(rawData, currentPosition, currentRecordIndex, currentFieldIndex)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class.
		/// </summary>
		/// <param name="rawData">The raw data when the error occurred.</param>
		/// <param name="currentPosition">The current position in the raw data.</param>
		/// <param name="currentRecordIndex">The current record index.</param>
		/// <param name="currentFieldIndex">The current field index.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public MissingRecordFieldException(string rawData, int currentPosition, long currentRecordIndex, int currentFieldIndex, Exception innerException)
			: base(rawData, currentPosition, currentRecordIndex, currentFieldIndex, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MissingRecordFieldException class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="T:SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected MissingRecordFieldException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}