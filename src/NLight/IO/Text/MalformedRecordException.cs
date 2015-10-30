// Author(s): Sébastien Lorion

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace NLight.IO.Text
{
	/// <summary>
	/// Represents the exception that is thrown when a record is malformed.
	/// </summary>
	[Serializable]
	public class MalformedRecordException
		: Exception
	{
		/// <summary>
		/// Contains the message that describes the error.
		/// </summary>
		private string _message;

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class.
		/// </summary>
		public MalformedRecordException()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public MalformedRecordException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public MalformedRecordException(string message, Exception innerException)
			: base(string.Empty, innerException)
		{
			_message = (message == null ? string.Empty : message);

			this.DataBuffer = string.Empty;
			this.BufferPosition = -1;
			this.CurrentRecordIndex = -1;
			this.CurrentColumnIndex = -1;
		}

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class.
		/// </summary>
		/// <param name="dataBuffer">The data buffer content when the error occurred.</param>
		/// <param name="bufferPosition">The current position in the data buffer content.</param>
		/// <param name="currentRecordIndex">The current record index.</param>
		/// <param name="currentColumnIndex">The current column index.</param>
		public MalformedRecordException(string dataBuffer, int bufferPosition, long currentRecordIndex, int currentColumnIndex)
			: this(dataBuffer, bufferPosition, currentRecordIndex, currentColumnIndex, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class.
		/// </summary>
		/// <param name="dataBuffer">The data buffer content when the error occurred.</param>
		/// <param name="bufferPosition">The current position in the data buffer content.</param>
		/// <param name="currentRecordIndex">The current record index.</param>
		/// <param name="currentColumnIndex">The current column index.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public MalformedRecordException(string dataBuffer, int bufferPosition, long currentRecordIndex, int currentColumnIndex, Exception innerException)
			: base(String.Empty, innerException)
		{
			this.DataBuffer = (dataBuffer == null ? string.Empty : dataBuffer);
			this.BufferPosition = bufferPosition;
			this.CurrentRecordIndex = currentRecordIndex;
			this.CurrentColumnIndex = currentColumnIndex;

			_message = string.Format(CultureInfo.InvariantCulture, Resources.ExceptionMessages.IO_MalformedRecordException, this.CurrentRecordIndex, this.CurrentColumnIndex, this.BufferPosition, this.DataBuffer);
		}

		/// <summary>
		/// Initializes a new instance of the MalformedRecordException class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="T:SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected MalformedRecordException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_message = info.GetString("MyMessage");
			this.DataBuffer = info.GetString("DataBuffer");
			this.BufferPosition = info.GetInt32("BufferPosition");
			this.CurrentRecordIndex = info.GetInt64("CurrentRecordIndex");
			this.CurrentColumnIndex = info.GetInt32("CurrentColumnIndex");
		}

		/// <summary>
		/// Gets the data buffer content when the error occurred.
		/// </summary>
		public string DataBuffer { get; }

		/// <summary>
		/// Gets the current position in the data buffer.
		/// </summary>
		public int BufferPosition { get; }

		/// <summary>
		/// Gets the current record index.
		/// </summary>
		public long CurrentRecordIndex { get; }

		/// <summary>
		/// Gets the current column index.
		/// </summary>
		public int CurrentColumnIndex { get; }

		#region Overrides

		/// <summary>
		/// Gets a message that describes the current exception.
		/// </summary>
		public override string Message => _message;

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:SerializationInfo"/> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:StreamingContext"/> that contains contextual information about the source or destination.</param>
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("MyMessage", _message);
			info.AddValue("DataBuffer", this.DataBuffer);
			info.AddValue("BufferPosition", this.BufferPosition);
			info.AddValue("CurrentRecordIndex", this.CurrentRecordIndex);
			info.AddValue("CurrentColumnIndex", this.CurrentColumnIndex);
		}

		#endregion
	}
}