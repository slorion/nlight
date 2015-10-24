// Author(s): Sébastien Lorion

using NLight.Core;
using NLight.Text.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NLight.IO.Text
{
	public partial class DelimitedRecordReader
		: TextRecordReader<DelimitedRecordColumn>
	{
		public const char DefaultDelimiterCharacter = ',';
		public const char DefaultQuoteCharacter = '"';
		public const char DefaultColumnHeaderTypeSeparator = ':';

		private readonly ValueBuilder _valueBuilder;
		private bool _readingColumnHeaders;

		public DelimitedRecordReader(TextReader reader)
			: this(reader, DefaultBufferSize)
		{
		}

		public DelimitedRecordReader(TextReader reader, int bufferSize)
			: base(reader, bufferSize)
		{
			this.DefaultColumnNamePrefix = "Column";
			this.DynamicColumnCount = true;
			this.TrimWhiteSpaces = true;

			this.DelimiterCharacter = DefaultDelimiterCharacter;
			this.QuoteCharacter = DefaultQuoteCharacter;
			this.DoubleQuoteEscapingEnabled = true;
			this.ColumnHeaderTypeSeparator = DefaultColumnHeaderTypeSeparator;

			_valueBuilder = new ValueBuilder();
		}

		public bool DynamicColumnCount { get; set; }
		public string DefaultColumnNamePrefix { get; set; }
		public bool TrimWhiteSpaces { get; set; }
		public bool AdvancedEscapingEnabled { get; set; }
		public char DelimiterCharacter { get; set; }
		public char QuoteCharacter { get; set; }
		public bool DoubleQuoteEscapingEnabled { get; set; }
		public char ColumnHeaderTypeSeparator { get; set; }

		private string ParseField(Buffer<char> buffer, bool keepValue, out bool endsWithDelimiter, out bool parsingErrorOccurred)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			_valueBuilder.Clear();

			endsWithDelimiter = false;
			parsingErrorOccurred = false;

			if (this.TrimWhiteSpaces)
			{
				if (!SkipWhiteSpaces(buffer))
					return string.Empty;
			}

			if (buffer.Current == this.QuoteCharacter)
			{
				#region Quoted field

				// skip initial quote
				buffer.Position++;

				bool quoted = true;

				for (;;)
				{
					int start = buffer.Position;

					while (buffer.Position < buffer.Length)
					{
						char c = buffer.Current;

						if (c == this.QuoteCharacter)
						{
							// if next char is a quote, then the quote is escaped
							if (this.DoubleQuoteEscapingEnabled
								&& (
									(buffer.Position + 1 < buffer.Length && buffer.RawData[buffer.Position + 1] == this.QuoteCharacter)
									|| (buffer.Position + 1 == buffer.Length && this.BaseReader.Peek() == (int) this.QuoteCharacter)))
							{
								if (keepValue)
									_valueBuilder.Append(buffer.RawData, start, buffer.Position - start + 1);

								if (buffer.Position + 1 == buffer.Length)
								{
									buffer.Fill();
									buffer.Position++;
								}
								else
								{
									buffer.Position += 2;
									buffer.EnsureHasData();
								}

								start = buffer.Position;
							}
							else
							{
								quoted = false;
								break;
							}
						}
						else if (keepValue && this.AdvancedEscapingEnabled && c == EscapedStringParser.EscapeCharacter)
						{
							char escapedValue;
							int currentValueLength = buffer.Position - start;

							if (EscapedStringParser.TryParseEscapedChar(buffer, out escapedValue))
							{
								_valueBuilder.Append(buffer.RawData, start, currentValueLength);
								_valueBuilder.Append(escapedValue);

								start = buffer.Position;
							}
							else
							{
								buffer.Position++;
							}
						}
						else
						{
							buffer.Position++;
						}
					}

					if (keepValue && buffer.Position > start)
						_valueBuilder.Append(buffer.RawData, start, buffer.Position - start);

					if (!quoted)
						break;
					else
					{
						// still expecting a quote, try to get more data
						if (!buffer.Fill())
						{
							parsingErrorOccurred = true;
							return _valueBuilder.ToString();
						}
					}
				}

				// skip final quote
				buffer.Position++;

				if (this.TrimWhiteSpaces)
					SkipWhiteSpaces(buffer);

				// skip delimiter if present
				if (buffer.EnsureHasData())
				{
					if (buffer.Current == this.DelimiterCharacter)
					{
						buffer.Position++;
						endsWithDelimiter = true;
					}
					else if (!base.IsNewLine(buffer.Current))
					{
						parsingErrorOccurred = true;
						return _valueBuilder.ToString();
					}
				}

				#endregion
			}
			else
			{
				#region Unquoted field

				int start = buffer.Position;

				for (;;)
				{
					char c = buffer.Current;

					if (c == this.DelimiterCharacter || base.IsNewLine(c))
					{
						if (keepValue)
						{
							if (this.TrimWhiteSpaces)
								TrimEnd(_valueBuilder, buffer.RawData, start, buffer.Position - 1);
							else
								_valueBuilder.Append(buffer.RawData, start, buffer.Position - start);
						}

						// only skip delimiter, new line will be parsed by main loop
						if (c == this.DelimiterCharacter)
						{
							buffer.Position++;
							endsWithDelimiter = true;
						}

						break;
					}
					else if (keepValue && this.AdvancedEscapingEnabled && c == EscapedStringParser.EscapeCharacter)
					{
						char escapedValue;
						int currentValueLength = buffer.Position - start;

						if (EscapedStringParser.TryParseEscapedChar(buffer, out escapedValue))
						{
							_valueBuilder.Append(buffer.RawData, start, currentValueLength);
							_valueBuilder.Append(escapedValue);

							start = buffer.Position;
						}
						else
						{
							buffer.Position++;
						}
					}
					else
					{
						buffer.Position++;
					}

					if (buffer.Position >= buffer.Length)
					{
						// if start = 0, the field length is bigger than the buffer length
						// so concatenate the buffer content and flush it
						if (start == 0)
						{
							if (keepValue)
								_valueBuilder.Append(buffer.RawData, 0, buffer.Position);

							start = buffer.Length;
						}

						if (!buffer.Fill(buffer.Length - start))
						{
							// concatenate remaining buffer content
							if (keepValue && buffer.Length > 0)
							{
								if (this.TrimWhiteSpaces)
									TrimEnd(_valueBuilder, buffer.RawData, 0, buffer.Position - 1);
								else
									_valueBuilder.Append(buffer.RawData, 0, buffer.Position);
							}

							buffer.Position = buffer.Length;

							break;
						}

						start = 0;
					}
				}

				#endregion
			}

			return _valueBuilder.ToString();
		}

		public ReadResult ReadColumnHeaders()
		{
			try
			{
				_readingColumnHeaders = true;

				var readResult = Read(false);
				if (readResult != ReadResult.Success)
					return readResult;

				this.Columns.Clear();

				int columnCount = this.CurrentRecord.Count;

				for (int i = 0; i < columnCount; i++)
				{
					string[] column = this.CurrentRecord[i].Split(this.ColumnHeaderTypeSeparator);

					string columnName = column.Length > 0 && !string.IsNullOrWhiteSpace(column[0])
						? column[0]
						: this.DefaultColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture);

					if (column.Length < 2)
						this.Columns.Add(new DelimitedRecordColumn(columnName));
					else
					{
						Type dataType;
						string typeName = column[1];

						switch (typeName.ToLower(CultureInfo.InvariantCulture))
						{
							case "string":
								dataType = typeof(string);
								break;
							case "int":
							case "int32":
								dataType = typeof(int);
								break;
							case "bool":
							case "boolean":
								dataType = typeof(bool);
								break;
							case "double":
								dataType = typeof(double);
								break;
							case "decimal":
								dataType = typeof(decimal);
								break;
							case "date":
							case "datetime":
								dataType = typeof(DateTime);
								break;
							case "long":
							case "int64":
								dataType = typeof(long);
								break;
							case "guid":
								dataType = typeof(Guid);
								break;
							case "time":
							case "timespan":
								dataType = typeof(TimeSpan);
								break;
							case "float":
							case "single":
								dataType = typeof(float);
								break;
							case "uint":
							case "uint32":
								dataType = typeof(uint);
								break;
							case "ulong":
							case "uint64":
								dataType = typeof(ulong);
								break;
							case "short":
							case "int16":
								dataType = typeof(short);
								break;
							case "ushort":
							case "uint16":
								dataType = typeof(ushort);
								break;
							case "sbyte":
								dataType = typeof(sbyte);
								break;
							case "byte":
								dataType = typeof(byte);
								break;
							case "char":
								dataType = typeof(char);
								break;

							default:
								dataType = Type.GetType(typeName, true, true);
								break;
						}

						this.Columns.Add(new DelimitedRecordColumn(columnName, dataType));
					}
				}

				return ReadResult.Success;
			}
			finally
			{
				_readingColumnHeaders = false;
			}
		}

		private void TrimEnd(ValueBuilder value, char[] buffer, int start, int end)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
			if (end >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(end));

			for (int i = end; i >= start; i--)
			{
				if (!IsWhiteSpace(buffer[i]))
				{
					value.Append(buffer, start, i - start + 1);
					return;
				}
			}

			_valueBuilder.TrimEnd(IsWhiteSpace);
		}

		#region Overrides

		protected override bool IsNewLine(char c)
		{
			if (c == this.DelimiterCharacter)
				return false;
			else
				return base.IsNewLine(c);
		}

		protected override bool IsWhiteSpace(char c)
		{
			if (c == this.DelimiterCharacter)
				return false;
			else
				return base.IsWhiteSpace(c);
		}

		protected override ReadResult ReadCore(Buffer<char> buffer, IList<string> values)
		{
			// this keeps track of the index of the last field that ended with a delimiter
			// it will let us know if we need to add an empty field because the record ended like "field1," or "\"field1\","
			int lastIndexFieldEndingWithDelimiter = -1;
			bool keepValue = false;

			while (buffer.EnsureHasData() && !IsNewLine(buffer.Current))
			{
				keepValue = (this.DynamicColumnCount || (values.Count < this.Columns.Count && !this.Columns[values.Count].IsIgnored) || _readingColumnHeaders);

				bool parsingErrorOccurred;
				bool endsWithDelimiter;

				string value = ParseField(buffer, keepValue, out endsWithDelimiter, out parsingErrorOccurred);

				if (!parsingErrorOccurred)
					values.Add(value);

				if (parsingErrorOccurred)
				{
					HandleParseError(new MalformedRecordException(new string(buffer.RawData, 0, buffer.Length), buffer.Position, this.CurrentRecordIndex, values.Count));
					return ReadResult.ParseError;
				}

				if (endsWithDelimiter)
					lastIndexFieldEndingWithDelimiter = values.Count - 1;
			}

			// Post-processing

			// add an empty field only if the last field ends with a delimiter or if the current record is empty
			if (lastIndexFieldEndingWithDelimiter == values.Count - 1 || values.Count == 0)
			{
				// and if we keep the value
				if (keepValue)
					values.Add(string.Empty);
			}

			if (this.DynamicColumnCount)
			{
				int count = values.Count;

				if (this.Columns.Count < count)
				{
					for (int i = this.Columns.Count; i < count; i++)
						this.Columns.Add(new DelimitedRecordColumn(GetDefaultColumnName(i)));
				}
				else
				{
					for (int i = this.Columns.Count - 1; i >= count; i--)
						this.Columns.Remove(this.Columns[i].Name);
				}
			}
			else
			{
				if (values.Count < this.Columns.Count)
				{
					switch (this.MissingFieldAction)
					{
						case MissingRecordFieldAction.ReturnEmptyValue:
							for (int i = this.Columns.Count - values.Count; i > 0; i--)
								values.Add(string.Empty);
							break;

						case MissingRecordFieldAction.ReturnNullValue:
							for (int i = this.Columns.Count - values.Count; i > 0; i--)
								values.Add(null);
							break;

						case MissingRecordFieldAction.HandleAsParseError:
						default:
							HandleParseError(new MissingRecordFieldException(new string(buffer.RawData, 0, buffer.Length), buffer.Position, this.CurrentRecordIndex, values.Count));
							return ReadResult.ParseError;
					}
				}
			}

			if (buffer.Position < buffer.Length)
				ParseNewLine();

			return ReadResult.Success;
		}

		#endregion
	}
}