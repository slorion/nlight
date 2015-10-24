// Author(s): Sébastien Lorion

using NLight.Text;
using System.IO;

namespace NLight.IO.Text
{
	public class FixedWidthRecordWriter
		: TextRecordWriter<FixedWidthRecordColumn>
	{
		public FixedWidthRecordWriter(TextWriter writer)
			: base(writer)
		{
		}

		public bool AllRecordsOnSingleLine { get; set; }

		#region Overrides

		protected override void WriteFieldCore(TextWriter writer, string value)
		{
			FixedWidthRecordColumn column = this.Columns[this.CurrentColumnIndex];

			if ((this.FieldTrimmingOptions & TrimmingOptions.Both) == TrimmingOptions.Both)
				value = value.Trim();
			else if ((this.FieldTrimmingOptions & TrimmingOptions.End) == TrimmingOptions.End)
				value = value.TrimEnd();
			else if ((this.FieldTrimmingOptions & TrimmingOptions.Start) == TrimmingOptions.Start)
				value = value.TrimStart();

			if (value.Length > column.Width)
				value = value.Substring(0, column.Width);
			else if (column.ValueAlignment == RecordColumnAlignment.Right)
				writer.Write(new string(column.PaddingCharacter, column.Width - value.Length));

			writer.Write(value);

			if (value.Length < column.Width && column.ValueAlignment == RecordColumnAlignment.Left)
				writer.Write(new string(column.PaddingCharacter, column.Width - value.Length));
		}

		protected override void WriteRecordEndCore(TextWriter writer)
		{
			if (!this.AllRecordsOnSingleLine)
				writer.WriteLine();
		}

		#endregion
	}
}