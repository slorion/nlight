// Author(s): Sébastien Lorion

using NLight.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace NLight.IO.Text
{
	public class FixedWidthRecordReader
		: TextRecordReader<FixedWidthRecordColumn>
	{
		private List<Tuple<FixedWidthRecordColumn, int>> _sortedByStartingPosition;

		public FixedWidthRecordReader(TextReader reader)
			: this(reader, FixedWidthRecordReader.DefaultBufferSize)
		{
		}

		public FixedWidthRecordReader(TextReader reader, int bufferSize)
			: base(reader, bufferSize)
		{
			_sortedByStartingPosition = new List<Tuple<FixedWidthRecordColumn, int>>();

			this.Columns.CollectionChanged += Columns_CollectionChanged;
		}

		public bool AllRecordsOnSingleLine { get; set; }

		private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (sender == null) throw new ArgumentNullException(nameof(sender));
			if (e == null) throw new ArgumentNullException(nameof(e));

			_sortedByStartingPosition.Clear();

			for (int i = 0; i < this.Columns.Count; i++)
				_sortedByStartingPosition.Add(Tuple.Create(this.Columns[i], i));

			_sortedByStartingPosition.Sort((a, b) => a.Item1.StartingPosition.CompareTo(b.Item1.StartingPosition));
		}

		private ReadResult HandleMissingFieldError(Buffer<char> buffer, IList<string> values, int currentSortedColumnIndex)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (currentSortedColumnIndex < 0) throw new ArgumentOutOfRangeException(nameof(currentSortedColumnIndex));

			string value = null;

			switch (this.MissingFieldAction)
			{
				case MissingRecordFieldAction.ReturnEmptyValue:
					value = string.Empty;
					break;
				case MissingRecordFieldAction.ReturnNullValue:
					value = null;
					break;
				case MissingRecordFieldAction.HandleAsParseError:
				default:
					HandleParseError(new MissingRecordFieldException(new string(buffer.RawData, 0, buffer.Length), buffer.Position, this.CurrentRecordIndex, values.Count));
					return ReadResult.ParseError;
			}

			for (int index = currentSortedColumnIndex; index < _sortedByStartingPosition.Count; index++)
				values[_sortedByStartingPosition[index].Item2] = value;

			if (!this.AllRecordsOnSingleLine)
				SkipToNextLine();

			return ReadResult.Success;
		}

		#region Overrides

		protected override ReadResult ReadCore(Buffer<char> buffer, IList<string> values)
		{
			if (this.Columns.Count < 1) throw new InvalidOperationException(Resources.ExceptionMessages.IO_NoColumnDefined);

			for (int i = 0; i < this.Columns.Count; i++)
				values.Add(null);

			FixedWidthRecordColumn previousColumn = null;
			for (int sortedColumnIndex = 0; sortedColumnIndex < _sortedByStartingPosition.Count; sortedColumnIndex++)
			{
				string value = string.Empty;

				Tuple<FixedWidthRecordColumn, int> columnAndIndex = _sortedByStartingPosition[sortedColumnIndex];

				// we need to account for gaps between non contiguous columns, i.e. columns not starting immediately after the previous one
				int toSkip = previousColumn == null ? 0 : columnAndIndex.Item1.StartingPosition - (previousColumn.StartingPosition + previousColumn.Width);

				int remaining = columnAndIndex.Item1.Width + toSkip;
				while (remaining > 0)
				{
					int delta = Math.Min(remaining, buffer.Length - buffer.Position);

					int end = buffer.Position + delta;
					for (int i = buffer.Position; i < end; i++)
					{
						if (IsNewLine(buffer.RawData[i]))
							return HandleMissingFieldError(buffer, values, sortedColumnIndex);
					}

					if (delta - toSkip > 0)
						value += new string(buffer.RawData, buffer.Position + toSkip, delta - toSkip);

					buffer.Position += delta;
					remaining -= delta;
					toSkip = Math.Max(0, toSkip - delta);

					if (delta < remaining && !buffer.Fill())
						return HandleMissingFieldError(buffer, values, sortedColumnIndex);
				}

				if (columnAndIndex.Item1.TrimPadding)
				{
					if (columnAndIndex.Item1.ValueAlignment == RecordColumnAlignment.Left)
					{
						int start = value.Length - 1;

						while (value[start] == columnAndIndex.Item1.PaddingCharacter && start > columnAndIndex.Item1.MinimumWidth)
							start--;

						if (start < value.Length - 1)
							value = value.Substring(0, start + 1);
					}
					else if (columnAndIndex.Item1.ValueAlignment == RecordColumnAlignment.Right)
					{
						int start = 0;

						while (value[start] == columnAndIndex.Item1.PaddingCharacter && value.Length - start > columnAndIndex.Item1.MinimumWidth)
							start++;

						if (start > 0)
							value = value.Substring(start, value.Length - start);
					}
				}

				values[columnAndIndex.Item2] = value;
				previousColumn = columnAndIndex.Item1;
			}

			if (!this.AllRecordsOnSingleLine)
				SkipToNextLine();

			return ReadResult.Success;
		}

		#endregion
	}
}