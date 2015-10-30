// Author(s): Sébastien Lorion

using NLight.Core;
using NLight.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace NLight.IO.Text
{
	public abstract partial class TextRecordReader<TColumn>
		: TextRecordReader, IDataReader, IDisposable
		where TColumn : RecordColumn
	{
		private Buffer<char> _buffer;
		private bool _eof;

		private readonly StreamReader _seekableReader;
		private readonly long _preambleLength;
		private long _recordStreamPositionIndexOffset;
		private readonly List<long> _recordStreamPositions = new List<long>();

		protected TextRecordReader(TextReader reader)
			: this(reader, DefaultBufferSize)
		{
		}

		protected TextRecordReader(TextReader reader, int bufferSize)
		{
#if DEBUG
			_allocStackTrace = new StackTrace();
#endif

			if (reader == null) throw new ArgumentNullException(nameof(reader));
			if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));

			this.SkipEmptyLines = true;
			this.CommentCharacter = DefaultCommentCharacter;

			this.CanDisposeBaseReader = true;
			this.BaseReader = reader;

			this.Columns = new RecordColumnCollection<TColumn>();

			this.CurrentRecord = new FastStringList(64);
			this.CurrentRecordIndex = -1;

			this.ValueConverter = new StringValueConverter();

			var seekableReader = reader as StreamReader;
			if (seekableReader != null)
			{
				_preambleLength = seekableReader.CurrentEncoding.GetPreamble().Length;

				if (seekableReader.BaseStream.CanSeek)
				{
					_seekableReader = seekableReader;

					// Handle bad implementations returning 0 or less
					if (seekableReader.BaseStream.Length > 0)
						bufferSize = (int) Math.Min(bufferSize, seekableReader.BaseStream.Length);
				}
			}

			_buffer = new Buffer<char>(bufferSize, BufferFillCallback);

			Debug.Assert(_seekableReader == null || _seekableReader.BaseStream.CanSeek);
		}

		public event EventHandler<ParseErrorEventArgs> ParseError;
		public event EventHandler<RecordParsedEventArgs> RecordParsed;

		protected virtual void OnParseError(ParseErrorEventArgs e)
		{
			if (e == null) throw new ArgumentNullException(nameof(e));

			this.ParseError?.Invoke(this, e);
		}

		public TextReader BaseReader { get; }
		public bool CanDisposeBaseReader { get; set; }

		public RecordColumnCollection<TColumn> Columns { get; }
		protected IList<string> CurrentRecord { get; }
		public long CurrentRecordIndex { get; private set; }

		public bool RecordPositionsCacheEnabled { get; private set; }
		public bool SkipEmptyLines { get; set; }
		public ParseErrorAction ParseErrorAction { get; set; }
		public MissingRecordFieldAction MissingFieldAction { get; set; }

		public StringValueConverter ValueConverter { get; }
		public CultureInfo Culture { get; set; }

		public char CommentCharacter { get; set; }

		public int BufferSize => _buffer?.Capacity ?? 0;

		public string this[int columnIndex]
		{
			get
			{
				if (columnIndex < 0 || columnIndex >= this.CurrentRecord.Count) throw new ArgumentOutOfRangeException(nameof(columnIndex));

				ValidateReader(Validations.HasCurrentRecord);

				return this.CurrentRecord[columnIndex];
			}
		}

		public string this[string columnName]
		{
			get
			{
				if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

				ValidateReader(Validations.HasCurrentRecord);
				int index = ValidateColumnName(columnName, nameof(columnName));

				return this.CurrentRecord[index];
			}
		}

		private int BufferFillCallback(char[] buffer, int offset)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || offset >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(offset));

			if (_eof)
				return 0;

			int count = this.BaseReader.Read(buffer, offset, buffer.Length - offset);

			_eof = (count <= 0);

			return count;
		}

		public void CopyCurrentRecordTo(object[] array)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));

			CopyCurrentRecordTo(array, 0);
		}

		public void CopyCurrentRecordTo(object[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex + this.Columns.Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			ValidateReader(Validations.HasCurrentRecord);

			int columnCount = this.Columns.Count;
			for (int i = 0; i < this.Columns.Count; i++)
				array[i] = this.GetValue(arrayIndex + i, null);
		}

		public void CopyCurrentRecordTo(Array array)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));

			CopyCurrentRecordTo(array, 0);
		}

		public void CopyCurrentRecordTo(Array array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex + this.Columns.Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			ValidateReader(Validations.HasCurrentRecord);

			int columnCount = this.Columns.Count;
			for (int i = 0; i < columnCount; i++)
				array.SetValue(this.GetValue(i, null), arrayIndex + i);
		}

		public void CopyCurrentRecordTo(string[] array)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));

			CopyCurrentRecordTo(array, 0);
		}

		public void CopyCurrentRecordTo(string[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex + this.Columns.Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			ValidateReader(Validations.HasCurrentRecord);

			this.CurrentRecord.CopyTo(array, arrayIndex);
		}

		public string GetDefaultColumnName(int columnIndex)
		{
			return "Column" + columnIndex.ToString(CultureInfo.InvariantCulture);
		}

		public IList<object> GetValues()
		{
			ValidateReader(Validations.HasCurrentRecord);

			var values = new object[this.Columns.Count];
			this.CopyCurrentRecordTo(values);
			return values;
		}

		public object GetValue(string columnName, object defaultValue)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return GetValue(columnIndex, defaultValue);
		}

		public object GetValue(int columnIndex, object defaultValue)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return GetValue(columnIndex, this.Columns[columnIndex].DataType, defaultValue);
		}

		public object GetValue(string columnName, Type type, object defaultValue)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return GetValue(columnIndex, type, defaultValue);
		}

		public object GetValue(int columnIndex, Type type, object defaultValue)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return this.ValueConverter.ConvertTo(this.CurrentRecord[columnIndex], TrimmingOptions.None, type, defaultValue, this.Culture);
		}

		public T GetValue<T>(string columnName, T defaultValue)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return GetValue<T>(columnIndex, defaultValue);
		}

		public T GetValue<T>(int columnIndex, T defaultValue)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return (T) this.ValueConverter.ConvertTo(this.CurrentRecord[columnIndex], TrimmingOptions.None, typeof(T), defaultValue, this.Culture);
		}

		public bool TryGetValue(string columnName, object defaultValue, out object value)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return TryGetValue(columnIndex, defaultValue, out value);
		}

		public bool TryGetValue(int columnIndex, object defaultValue, out object value)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return TryGetValue(columnIndex, this.Columns[columnIndex].DataType, defaultValue, out value);
		}

		public bool TryGetValue(string columnName, Type type, object defaultValue, out object value)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return TryGetValue(columnIndex, type, defaultValue, out value);
		}

		public bool TryGetValue(int columnIndex, Type type, object defaultValue, out object value)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return this.ValueConverter.TryConvertTo(this.CurrentRecord[columnIndex], TrimmingOptions.None, type, defaultValue, this.Culture, out value);
		}

		public bool TryGetValue<T>(string columnName, T defaultValue, out T value)
		{
			int columnIndex = ValidateColumnName(columnName, nameof(columnName));
			return TryGetValue<T>(columnIndex, defaultValue, out value);
		}

		public bool TryGetValue<T>(int columnIndex, T defaultValue, out T value)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			object objectValue;

			if (this.ValueConverter.TryConvertTo(this.CurrentRecord[columnIndex], TrimmingOptions.None, typeof(T), defaultValue, this.Culture, out objectValue))
			{
				value = (T) objectValue;
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		/// <summary>
		/// Handles a parsing error.
		/// </summary>
		/// <param name="error">The parsing error that occurred.</param>
		/// <exception cref="ArgumentNullException">
		///	<paramref name="error"/> is <c>null</c>.
		/// </exception>
		protected void HandleParseError(MalformedRecordException error)
		{
			if (error == null) throw new ArgumentNullException(nameof(error));

			switch (this.ParseErrorAction)
			{
				case ParseErrorAction.ThrowException:
					throw error;

				case ParseErrorAction.RaiseEvent:
					ParseErrorEventArgs e = new ParseErrorEventArgs(error, ParseErrorAction.ThrowException);
					OnParseError(e);

					switch (e.Action)
					{
						case ParseErrorAction.ThrowException:
							throw e.Error;

						case ParseErrorAction.RaiseEvent:
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.IO_ParseErrorActionInvalidInsideParseErrorEvent, e.Action), e.Error);

						case ParseErrorAction.SkipToNextLine:
							SkipToNextLine();
							break;

						default:
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.IO_ParseErrorActionNotSupported, e.Action), e.Error);
					}
					break;

				case ParseErrorAction.SkipToNextLine:
					SkipToNextLine();
					break;

				default:
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.IO_ParseErrorActionNotSupported, this.ParseErrorAction), error);
			}
		}

		protected virtual bool IsNewLine(char c)
		{
			return (c == '\r' || c == '\n');
		}

		/// <summary>
		/// Indicates whether the specified Unicode character is categorized as white space.
		/// </summary>
		/// <param name="c">A Unicode character.</param>
		/// <returns><c>true</c> if <paramref name="c"/> is white space; otherwise, <c>false</c>.</returns>
		protected virtual bool IsWhiteSpace(char c)
		{
			//if (c <= '\x00ff')
			return (c == ' ' || c == '\t');
			//else
			//    return (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.SpaceSeparator);
		}

		public ReadResult MoveTo(long recordIndex)
		{
			if (recordIndex < 0 || recordIndex > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(recordIndex));
			if (_seekableReader == null && recordIndex < this.CurrentRecordIndex) throw new InvalidOperationException(Resources.ExceptionMessages.IO_CannotMovePreviousRecordInForwardOnly);

			Debug.Assert(_seekableReader == null || _seekableReader.BaseStream.CanSeek);

			ValidateReader(Validations.IsNotDisposed);

			if (recordIndex == this.CurrentRecordIndex)
				return ReadResult.Success;
			else if (recordIndex < this.CurrentRecordIndex)
			{
				_seekableReader.DiscardBufferedData();
				_buffer.Clear();
				_eof = false;

				if (this.RecordPositionsCacheEnabled && recordIndex - _recordStreamPositionIndexOffset < _recordStreamPositions.Count)
				{
					_seekableReader.BaseStream.Position = Math.Max(_preambleLength, _recordStreamPositions[(int) (recordIndex - _recordStreamPositionIndexOffset)]);

					this.CurrentRecordIndex = recordIndex - 1;
					return Read();
				}
				else
				{
					this.StopCachingRecordPositions(true);

					_seekableReader.BaseStream.Position = _preambleLength;
					this.CurrentRecordIndex = -1;

					ReadResult readResult = ReadResult.Success;
					for (int i = 0; i <= recordIndex; i++)
					{
						readResult = this.Read();
						if (readResult == ReadResult.EndOfFile)
							return ReadResult.EndOfFile;
					}

					return readResult;
				}
			}
			else
			{
				long offset = recordIndex - this.CurrentRecordIndex;

				ReadResult readResult = ReadResult.Success;
				do
				{
					readResult = Read();
					if (readResult == ReadResult.EndOfFile)
						return ReadResult.EndOfFile;
				}
				while (--offset > 0);

				return readResult;
			}
		}

		protected bool ParseNewLine()
		{
			if (_buffer.EnsureHasData())
			{
				char c = _buffer.Current;

				if (c == '\r' && IsNewLine(c))
				{
					_buffer.Position++;

					// Skip following \n if there is one
					if (_buffer.EnsureHasData() && _buffer.Current == '\n')
						_buffer.Position++;

					return true;
				}
				else if (c == '\n')
				{
					_buffer.Position++;
					return true;
				}
			}

			return false;
		}

		public ReadResult Read(bool incrementRecordIndex = true)
		{
			ValidateReader(Validations.IsNotDisposed);

			if (!_buffer.EnsureHasData())
				return ReadResult.EndOfFile;

			if (!SkipEmptyAndCommentedLines())
				return ReadResult.EndOfFile;
			else
			{
				if (incrementRecordIndex)
				{
					this.CurrentRecordIndex++;

					if (this.RecordPositionsCacheEnabled && _seekableReader != null && this.CurrentRecordIndex - _recordStreamPositionIndexOffset <= _recordStreamPositions.Count)
						_recordStreamPositions.Add(_seekableReader.BaseStream.Position - sizeof(char) * (_buffer.Length - _buffer.Position));
				}

				this.CurrentRecord.Clear();
				ReadResult readResult = ReadCore(_buffer, this.CurrentRecord);

				while (this.CurrentRecord.Count < this.Columns.Count)
					this.CurrentRecord.Add(null);

				var recordParsed = this.RecordParsed;
				if (recordParsed != null)
					recordParsed(this, new RecordParsedEventArgs(readResult, this.CurrentRecord));

				return readResult;
			}
		}

		protected abstract ReadResult ReadCore(Buffer<char> buffer, IList<string> values);

		public bool SkipToNextLine()
		{
			ValidateReader(Validations.IsNotDisposed);

			char[] data = _buffer.RawData;

			do
			{
				int length = _buffer.Length;

				for (int i = _buffer.Position; i < length; i++)
				{
					if (IsNewLine(data[i]))
					{
						_buffer.Position = i;

						ParseNewLine();
						return _buffer.EnsureHasData();
					}
				}
			}
			while (_buffer.Fill());

			return false;
		}

		protected bool SkipEmptyAndCommentedLines()
		{
			while (_buffer.EnsureHasData())
			{
				if (_buffer.Current == this.CommentCharacter)
				{
					_buffer.Position++;
					SkipToNextLine();
				}
				else if (this.SkipEmptyLines && ParseNewLine())
					continue;
				else
					break;
			}

			return _buffer.EnsureHasData();
		}

		protected bool SkipWhiteSpaces(Buffer<char> buffer)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			char[] data = buffer.RawData;

			do
			{
				int length = _buffer.Length;

				for (int i = _buffer.Position; i < length; i++)
				{
					if (!IsWhiteSpace(data[i]))
					{
						_buffer.Position = i;
						return buffer.EnsureHasData();
					}
				}
			}
			while (buffer.Fill());

			return buffer.EnsureHasData();
		}

		public void StartCachingRecordPositions()
		{
			this.RecordPositionsCacheEnabled = true;
			_recordStreamPositions.Clear();
			_recordStreamPositionIndexOffset = Math.Max(0, this.CurrentRecordIndex);
		}

		public void StopCachingRecordPositions(bool clearCache)
		{
			this.RecordPositionsCacheEnabled = false;

			if (clearCache)
			{
				_recordStreamPositions.Clear();
				_recordStreamPositionIndexOffset = Math.Max(0, this.CurrentRecordIndex);
			}
		}

		protected void ValidateColumnIndex(int columnIndex, string argumentName)
		{
			if (columnIndex < 0 || columnIndex >= this.Columns.Count) throw new ArgumentOutOfRangeException(argumentName);
		}

		protected int ValidateColumnName(string columnName, string argumentName)
		{
			if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(argumentName);

			if (string.IsNullOrEmpty(columnName))
				throw new ArgumentNullException(argumentName);

			int index = this.Columns.IndexOf(columnName);

			if (index < 0)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.IO_ColumnNotFound, columnName), argumentName);

			return index;
		}

		protected void ValidateReader(Validations validations)
		{
			if ((validations & Validations.IsNotDisposed) == Validations.IsNotDisposed && this.IsDisposed)
				throw new ObjectDisposedException(null);

			if ((validations & Validations.HasCurrentRecord) == Validations.HasCurrentRecord && this.CurrentRecordIndex < 0)
				throw new InvalidOperationException(Resources.ExceptionMessages.IO_NoCurrentRecord);
		}

		#region IDataReader Members

		void IDataReader.Close() => this.Dispose();

		int IDataReader.Depth
		{
			get
			{
				ValidateReader(Validations.IsNotDisposed);

				return 0;
			}
		}

		DataTable IDataReader.GetSchemaTable()
		{
			ValidateReader(Validations.IsNotDisposed);

			DataTable schema = new DataTable("SchemaTable");
			schema.Locale = CultureInfo.InvariantCulture;
			schema.MinimumCapacity = this.Columns.Count;

			schema.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
			schema.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
			schema.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
			schema.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).DefaultValue = string.Empty;
			schema.Columns.Add(SchemaTableColumn.ColumnName, typeof(string));
			schema.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
			schema.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).DefaultValue = -1;
			schema.Columns.Add(SchemaTableColumn.DataType, typeof(Type));
			schema.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
			schema.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
			schema.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).DefaultValue = false;
			schema.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).DefaultValue = false;
			schema.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
			schema.Columns.Add(SchemaTableColumn.NonVersionedProviderType, typeof(int));
			schema.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short));
			schema.Columns.Add(SchemaTableColumn.NumericScale, typeof(short));
			schema.Columns.Add(SchemaTableColumn.ProviderType, typeof(int));

			schema.Columns.Add(SchemaTableOptionalColumn.AutoIncrementSeed, typeof(long)).DefaultValue = 0L;
			schema.Columns.Add(SchemaTableOptionalColumn.AutoIncrementStep, typeof(long)).DefaultValue = 1L;
			schema.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
			schema.Columns.Add(SchemaTableOptionalColumn.BaseColumnNamespace, typeof(string));
			schema.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
			schema.Columns.Add(SchemaTableOptionalColumn.BaseTableNamespace, typeof(string)).DefaultValue = string.Empty;
			schema.Columns.Add(SchemaTableOptionalColumn.ColumnMapping, typeof(MappingType));
			schema.Columns.Add(SchemaTableOptionalColumn.DefaultValue, typeof(object));
			schema.Columns.Add(SchemaTableOptionalColumn.Expression, typeof(string));
			schema.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).DefaultValue = false;
			schema.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
			schema.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).DefaultValue = false;
			schema.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).DefaultValue = false;

			// null marks columns that will change for each row, except DataType and ProviderType which are String by default
			object[] schemaRow =
				new object[] {
					true,					// 00- AllowDBNull
					null,					// 01- BaseColumnName
					null,					// 02- BaseSchemaName
					null,					// 03- BaseTableName
					null,					// 04- ColumnName
					null,					// 05- ColumnOrdinal
					null,					// 06- ColumnSize
					typeof(string),			// 07- DataType
					false,					// 08- IsAliased
					false,					// 09- IsExpression
					false,					// 10- IsKey
					false,					// 11- IsLong
					false,					// 12- IsUnique
					null,					// 13- NonVersionedProviderType
					DBNull.Value,			// 14- NumericPrecision
					DBNull.Value,			// 15- NumericScale
					//TODO: null or typeof(string) ?
					null,					// 16- ProviderType

					//TODO: seed = 0 and step = 1 ?
					null,					// 17- AutoIncrementSeed
					null,					// 18- AutoIncrementStep
					null,					// 19- BaseCatalogName
					string.Empty,			// 20- BaseColumnNamespace
					null,					// 21- BaseServerName
					null,					// 22- BaseTableNamespace
					MappingType.Element,	// 23- ColumnMapping
					null,					// 24- DefaultValue
					null,					// 25- Expression
					false,					// 26- IsAutoIncrement
					false,					// 27- IsHidden
					false,					// 28- IsReadOnly
					false					// 29- IsRowVersion
				};

			for (int i = 0; i < this.Columns.Count; i++)
			{
				schemaRow[1] = this.Columns[i].Name;
				schemaRow[4] = this.Columns[i].Name;
				schemaRow[5] = i;
				schemaRow[7] = this.Columns[i].DataType;

				schema.Rows.Add(schemaRow);
			}

			return schema;
		}

		bool IDataReader.IsClosed => this.IsDisposed;

		bool IDataReader.NextResult()
		{
			ValidateReader(Validations.IsNotDisposed);

			return false;
		}

		bool IDataReader.Read()
		{
			ReadResult result;

			do
			{
				result = Read(true);
			} while (result == ReadResult.ParseError && result != ReadResult.EndOfFile);

			return (result != ReadResult.EndOfFile);
		}

		int IDataReader.RecordsAffected => -1;

		#endregion

		#region IDataRecord Members

		int IDataRecord.FieldCount
		{
			get
			{
				ValidateReader(Validations.IsNotDisposed);

				return this.Columns.Count;
			}
		}

		bool IDataRecord.GetBoolean(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Boolean))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToBoolean(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Boolean), this.Culture);
		}

		byte IDataRecord.GetByte(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Byte))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToByte(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Byte), this.Culture);
		}

		long IDataRecord.GetBytes(int columnIndex, long valueOffset, byte[] buffer, int bufferOffset, int length)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			string value = this.CurrentRecord[columnIndex];

			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, null);
			if (valueOffset < 0 || valueOffset + length > value.Length * sizeof(char)) throw new ArgumentOutOfRangeException(nameof(valueOffset), valueOffset, null);
			if (bufferOffset < 0 || bufferOffset + length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(bufferOffset), bufferOffset, null);

			return Encoding.Unicode.GetBytes(value, (int) valueOffset, length / sizeof(char), buffer, bufferOffset);
		}

		char IDataRecord.GetChar(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(char))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToChar(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Char), this.Culture);
		}

		long IDataRecord.GetChars(int columnIndex, long valueOffset, char[] buffer, int bufferOffset, int length)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			string value = this.CurrentRecord[columnIndex];

			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, null);
			if (valueOffset < 0 || valueOffset + length > value.Length) throw new ArgumentOutOfRangeException(nameof(valueOffset), valueOffset, null);
			if (bufferOffset < 0 || bufferOffset + length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(bufferOffset), bufferOffset, null);

			int end = (int) valueOffset + length;

			for (int i = (int) valueOffset; i < end; i++)
				buffer[bufferOffset + i] = value[i];

			return length;
		}

		IDataReader IDataRecord.GetData(int columnIndex)
		{
			ValidateReader(Validations.IsNotDisposed);

			if (columnIndex == 0)
				return this;
			else
				return null;
		}

		string IDataRecord.GetDataTypeName(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return this.Columns[columnIndex].DataType.FullName;
		}

		DateTime IDataRecord.GetDateTime(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(DateTime))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToDateTime(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(DateTime), this.Culture);
		}

		decimal IDataRecord.GetDecimal(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Decimal))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToDecimal(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Decimal), this.Culture);
		}

		double IDataRecord.GetDouble(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Double))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToDouble(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Double), this.Culture);
		}

		Type IDataRecord.GetFieldType(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return this.Columns[columnIndex].DataType;
		}

		float IDataRecord.GetFloat(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Single))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToSingle(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Single), this.Culture);
		}

		Guid IDataRecord.GetGuid(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Guid))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToGuid(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Guid), this.Culture);
		}

		short IDataRecord.GetInt16(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Int16))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToInt16(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Int16), this.Culture);
		}

		int IDataRecord.GetInt32(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Int32))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToInt32(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Int32), this.Culture);
		}

		long IDataRecord.GetInt64(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (this.Columns[columnIndex].DataType != typeof(Int64))
				throw new InvalidCastException();

			return this.ValueConverter.ConvertToInt64(this.CurrentRecord[columnIndex], TrimmingOptions.None, default(Int64), this.Culture);
		}

		string IDataRecord.GetName(int columnIndex)
		{
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return this.Columns[columnIndex].Name;
		}

		int IDataRecord.GetOrdinal(string columnName)
		{
			return this.Columns.IndexOf(columnName);
		}

		string IDataRecord.GetString(int columnIndex)
		{
			if (this.Columns[columnIndex].DataType != typeof(string))
				throw new InvalidCastException();

			return this[columnIndex];
		}

		object IDataRecord.GetValue(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			if (((IDataRecord) this).IsDBNull(columnIndex))
				return DBNull.Value;
			else
				return this.ValueConverter.ConvertTo(this.CurrentRecord[columnIndex], TrimmingOptions.None, this.Columns[columnIndex].DataType, this.Columns[columnIndex].DefaultValue, this.Culture);
		}

		int IDataRecord.GetValues(object[] values)
		{
			ValidateReader(Validations.HasCurrentRecord);

			if (values == null) throw new ArgumentNullException(nameof(values));

			for (int i = 0; i < this.Columns.Count; i++)
				values[i] = ((IDataRecord) this).GetValue(i);

			return this.Columns.Count;
		}

		bool IDataRecord.IsDBNull(int columnIndex)
		{
			ValidateReader(Validations.HasCurrentRecord);
			ValidateColumnIndex(columnIndex, nameof(columnIndex));

			return string.IsNullOrEmpty(this.CurrentRecord[columnIndex]);
		}

		object IDataRecord.this[string columnName]
		{
			get
			{
				ValidateReader(Validations.HasCurrentRecord);
				int index = ValidateColumnName(columnName, nameof(columnName));

				return ((IDataRecord) this).GetValue(index);
			}
		}

		object IDataRecord.this[int columnIndex] => ((IDataRecord) this).GetValue(columnIndex);

		#endregion

		#region IDisposable Members

		private readonly StackTrace _allocStackTrace;

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.IsDisposed)
			{
				try
				{
					if (disposing)
					{
						if (this.BaseReader != null && this.CanDisposeBaseReader)
							this.BaseReader.Dispose();
					}
				}
				catch (Exception ex) when (!disposing)
				{
					Log.Source.TraceEvent(TraceEventType.Error, 0, Resources.LogMessages.Shared_ExceptionDuringFinalization, ex);
				}
				finally
				{
					_buffer = null;
					this.IsDisposed = true;
				}
			}
		}

		~TextRecordReader()
		{
			Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
			Dispose(false);
		}

		#endregion
	}
}