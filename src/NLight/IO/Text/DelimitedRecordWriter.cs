// Author(s): Sébastien Lorion

using NLight.Text;
using System.IO;

namespace NLight.IO.Text
{
	public class DelimitedRecordWriter
		: TextRecordWriter<DelimitedRecordColumn>
	{
		public const char DefaultDelimiterCharacter = ',';
		public const char DefaultQuoteCharacter = '"';
		public const char DefaultQuoteEscapeCharacter = '"';

		private char _quote;
		private char _quoteEscape;

		private string _escapedQuoteString;
		private string _quoteString;

		private bool _isFirstField;

		public DelimitedRecordWriter(TextWriter writer)
			: base(writer)
		{
			this.DelimiterCharacter = DefaultDelimiterCharacter;
			_quote = DefaultQuoteCharacter;
			_quoteEscape = DefaultQuoteEscapeCharacter;

			UpdateEscapedQuoteStrings();
		}

		public char DelimiterCharacter { get; set; }

		public char QuoteCharacter
		{
			get { return _quote; }
			set
			{
				_quote = value;
				UpdateEscapedQuoteStrings();
			}
		}

		public char QuoteEscapeCharacter
		{
			get { return _quoteEscape; }
			set
			{
				_quoteEscape = value;
				UpdateEscapedQuoteStrings();
			}
		}

		private void UpdateEscapedQuoteStrings()
		{
			_quoteString = _quote.ToString();
			_escapedQuoteString = _quoteEscape.ToString() + _quote.ToString();
		}

		#region Overrides

		protected override void WriteFieldCore(TextWriter writer, string value)
		{
			if (!_isFirstField)
				writer.Write(this.DelimiterCharacter);

			if ((this.FieldTrimmingOptions & TrimmingOptions.Both) == TrimmingOptions.Both)
				value = value.Trim();
			else if ((this.FieldTrimmingOptions & TrimmingOptions.End) == TrimmingOptions.End)
				value = value.TrimEnd();
			else if ((this.FieldTrimmingOptions & TrimmingOptions.Start) == TrimmingOptions.Start)
				value = value.TrimStart();

			value = value.Replace(_quoteString, _escapedQuoteString);

			writer.Write(_quote);
			writer.Write(value);
			writer.Write(_quote);

			_isFirstField = false;
		}

		protected override void WriteRecordStartCore(TextWriter writer)
		{
			base.WriteRecordStartCore(writer);

			_isFirstField = true;
		}

		#endregion
	}
}