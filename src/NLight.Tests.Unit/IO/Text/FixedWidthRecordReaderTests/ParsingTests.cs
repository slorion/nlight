// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace NLight.Tests.Unit.IO.Text.FixedWidthRecordReaderTests
{
	public class ParsingTests
	{
		[Test]
		public void BasicTest()
		{
			using (var reader = new FixedWidthRecordReader(new StringReader("123123412345")))
			{
				reader.Columns.Add(new FixedWidthRecordColumn("a", 0, 3));
				reader.Columns.Add(new FixedWidthRecordColumn("b", 3, 4));
				reader.Columns.Add(new FixedWidthRecordColumn("c", 7, 5));

				Assert.IsTrue(reader.Read() == ReadResult.Success);
				Assert.AreEqual("123", reader[0]);
				Assert.AreEqual("1234", reader[1]);
				Assert.AreEqual("12345", reader[2]);
			}
		}

		[Test]
		public void TrimPaddingAlignLeftTest()
		{
			using (var reader = new FixedWidthRecordReader(new StringReader("123   ")))
			{
				reader.Columns.Add(new FixedWidthRecordColumn("a", 0, 6));
				reader.Columns[0].TrimPadding = true;
				reader.Columns[0].ValueAlignment = RecordColumnAlignment.Left;

				Assert.IsTrue(reader.Read() == ReadResult.Success);
				Assert.AreEqual("123", reader[0]);
			}
		}

		[Test]
		public void TrimPaddingAlignRightTest()
		{
			using (var reader = new FixedWidthRecordReader(new StringReader("   123")))
			{
				reader.Columns.Add(new FixedWidthRecordColumn("a", 0, 6));
				reader.Columns[0].TrimPadding = true;
				reader.Columns[0].ValueAlignment = RecordColumnAlignment.Right;

				Assert.IsTrue(reader.Read() == ReadResult.Success);
				Assert.AreEqual("123", reader[0]);
			}
		}

		[Test]
		public void NonContiguousColumnsTest()
		{
			using (var reader = new FixedWidthRecordReader(new StringReader("123123412345")))
			{
				reader.Columns.Add(new FixedWidthRecordColumn("a", 0, 3));
				reader.Columns.Add(new FixedWidthRecordColumn("c", 7, 5));

				Assert.IsTrue(reader.Read() == ReadResult.Success);
				Assert.AreEqual("123", reader[0]);
				Assert.AreEqual("12345", reader[1]);
			}
		}

		[Test]
		public void ParsingTest_Random()
		{
			//TODO: use RandomAttribute when usable

			var random = new Random();

			for (int i = 0; i < 100; i++)
			{
				int bufferSize = random.Next(1, 1024);
				int recordCount = random.Next(1, 100);
				int columnCount = random.Next(1, 100);
				int columnWidth = random.Next(4, 100);

				Assert.DoesNotThrow(() => ParsingTest(bufferSize, recordCount, columnCount, columnWidth), "BufferSize={0},RecordCount={1},ColumnCount={2},ColumnWidth={3}", bufferSize, recordCount, columnCount, columnWidth);
			}
		}

		[CLSCompliant(false)]
		[TestCase(4, 4, 4, 4)]
		[TestCase(4, 4, 4, 5)]
		public void ParsingTest(int bufferSize, int recordCount, int columnCount, int columnWidth)
		{
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "Must be 1 or more.");

			if (recordCount < 0 || recordCount > 99)
				throw new ArgumentOutOfRangeException(nameof(recordCount), recordCount, "Must be between 1 and 99 inclusively.");

			if (columnCount < 0 || columnCount > 99)
				throw new ArgumentOutOfRangeException(nameof(columnCount), columnCount, "Must be between 1 and 99 inclusively.");

			var expectedValues = new string[recordCount][];
			var data = new StringBuilder(recordCount * columnCount * columnWidth + recordCount);

			for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
			{
				expectedValues[recordIndex] = new string[columnCount];

				for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
				{
					string value = recordIndex.ToString().PadRight(2) + columnIndex.ToString().PadRight(2) + new string(' ', columnWidth - 4);

					expectedValues[recordIndex][columnIndex] = value;
					data.Append(value);
				}

				data.Append('\n');
			}

			using (var reader = new FixedWidthRecordReader(new StringReader(data.ToString()), bufferSize))
			{
				for (int i = columnCount - 1; i >= 0; i--)
				{
					var column = new FixedWidthRecordColumn("col" + (columnCount - i - 1).ToString(), typeof(string), string.Empty, i * columnWidth, columnWidth);
					column.TrimPadding = false;
					reader.Columns.Add(column);
				}

				int recordIndex = -1;
				Assert.AreEqual(recordIndex, reader.CurrentRecordIndex);

				while (reader.Read() == ReadResult.Success)
				{
					recordIndex++;
					Assert.AreEqual(recordIndex, reader.CurrentRecordIndex);

					for (int i = 0; i < reader.Columns.Count; i++)
					{
						Assert.AreEqual(expectedValues[recordIndex][i], reader[columnCount - i - 1]);
						Assert.AreEqual(expectedValues[recordIndex][i], reader["col" + (columnCount - i - 1).ToString()]);
					}
				}

				Assert.AreEqual(recordCount, recordIndex + 1);
				Assert.AreEqual(recordCount, reader.CurrentRecordIndex + 1);
			}
		}
	}
}