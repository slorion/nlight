// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public class MalformedTests
	{
		#region Utilities

		private void CheckMissingFieldUnquoted(long recordCount, int columnCount, long badRecordIndex, int badColumnIndex, int bufferSize)
		{
			CheckMissingFieldUnquoted(recordCount, columnCount, badRecordIndex, badColumnIndex, bufferSize, MissingRecordFieldAction.HandleAsParseError);
			CheckMissingFieldUnquoted(recordCount, columnCount, badRecordIndex, badColumnIndex, bufferSize, MissingRecordFieldAction.ReturnEmptyValue);
			CheckMissingFieldUnquoted(recordCount, columnCount, badRecordIndex, badColumnIndex, bufferSize, MissingRecordFieldAction.ReturnNullValue);
		}

		private void CheckMissingFieldUnquoted(long recordCount, int columnCount, long badRecordIndex, int badColumnIndex, int bufferSize, MissingRecordFieldAction action)
		{
			// construct the csv data with template "00,01,02\n10,11,12\n...." and calculate expected error position

			long capacity = recordCount * (columnCount * 2 + columnCount - 1) + recordCount;
			Assert.IsTrue(capacity <= int.MaxValue);

			var sb = new StringBuilder((int) capacity);

			for (long i = 0; i < recordCount; i++)
			{
				int readColumnCount;

				if (i == badRecordIndex)
					readColumnCount = badColumnIndex;
				else
					readColumnCount = columnCount;

				for (int j = 0; j < readColumnCount; j++)
				{
					sb.Append(i);
					sb.Append(j);
					sb.Append(DelimitedRecordReader.DefaultDelimiterCharacter);
				}

				sb.Length--;
				sb.Append('\n');
			}

			// test csv

			const string AssertMessage = "RecordIndex={0}; ColumnIndex={1}; Position={2}; Action={3}";

			using (var csv = new DelimitedRecordReader(new StringReader(sb.ToString()), bufferSize))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < columnCount; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				csv.MissingFieldAction = action;
				Assert.AreEqual(columnCount, csv.Columns.Count);

				try
				{
					while (csv.Read() == ReadResult.Success)
					{
						Assert.AreEqual(columnCount, csv.Columns.Count);

						for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
						{
							string s = csv[columnIndex];

							if (csv.CurrentRecordIndex != badRecordIndex || columnIndex < badColumnIndex)
								Assert.AreEqual(csv.CurrentRecordIndex.ToString() + columnIndex.ToString(), s);//, AssertMessage, csv.CurrentRecordIndex, columnIndex, -1, action);
							else
							{
								switch (action)
								{
									case MissingRecordFieldAction.ReturnEmptyValue:
										Assert.AreEqual(string.Empty, s);//, AssertMessage, csv.CurrentRecordIndex, columnIndex, -1, action);
										break;

									case MissingRecordFieldAction.ReturnNullValue:
										Assert.IsNull(s);//, AssertMessage, csv.CurrentRecordIndex, columnIndex, -1, action);
										break;

									case MissingRecordFieldAction.HandleAsParseError:
										throw new AssertionException(string.Format("Failed to throw HandleAsParseError. - " + AssertMessage, csv.CurrentRecordIndex, columnIndex, -1, action));

									default:
										throw new AssertionException(string.Format("'{0}' is not handled by this test.", action));
								}
							}
						}
					}
				}
				catch (MissingRecordFieldException ex)
				{
					Assert.AreEqual(badRecordIndex, ex.CurrentRecordIndex, AssertMessage, ex.CurrentRecordIndex, ex.CurrentColumnIndex, ex.BufferPosition, action);
					Assert.IsTrue(ex.CurrentColumnIndex >= badColumnIndex, AssertMessage, ex.CurrentRecordIndex, ex.CurrentColumnIndex, ex.BufferPosition, action);
				}
			}
		}

		#endregion

		[Test]
		public void MissingFieldUnquotedTest1()
		{
			CheckMissingFieldUnquoted(4, 4, 2, 2, DelimitedRecordReader.DefaultBufferSize);
			CheckMissingFieldUnquoted(4, 4, 2, 2, DelimitedRecordReader.DefaultBufferSize);
		}

		[Test]
		public void MissingFieldUnquotedTest2()
		{
			// With bufferSize = 16, faulty new line char is at the start of next buffer read
			CheckMissingFieldUnquoted(4, 4, 2, 3, 16);
		}

		[Test]
		public void MissingFieldUnquotedTest3()
		{
			// test missing field when end of buffer has been reached
			CheckMissingFieldUnquoted(3, 4, 2, 3, 16);
		}

		[Test]
		public void MissingFieldUnquotedTest4()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("a,b\n   ")))
			{
				csv.SkipEmptyLines = true;
				csv.MissingFieldAction = MissingRecordFieldAction.ReturnNullValue;

				csv.DynamicColumnCount = true;
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(2, csv.Columns.Count);
				Assert.AreEqual("a", csv[0]);
				Assert.AreEqual("b", csv[1]);

				csv.DynamicColumnCount = false;
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(string.Empty, csv[0]);
				Assert.AreEqual(null, csv[1]);
			}
		}

		[Test]
		public void MissingFieldQuotedTest1()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\"\n3,3,3,3";

			using (var csv = new DelimitedRecordReader(new StringReader(Data)))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 4; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				var ex = Assert.Throws<MissingRecordFieldException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(2, ex.CurrentRecordIndex);
				Assert.AreEqual(2, ex.CurrentColumnIndex);
			}
		}

		[Test]
		public void MissingFieldQuotedTest2()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\",\n3,3,3,3";

			using (var csv = new DelimitedRecordReader(new StringReader(Data), 11))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 4; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				var ex = Assert.Throws<MissingRecordFieldException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(2, ex.CurrentRecordIndex);
				Assert.AreEqual(3, ex.CurrentColumnIndex);
			}
		}

		[Test]
		public void MissingFieldQuotedTest3()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\"\n\"3\",3,3,3";

			using (var csv = new DelimitedRecordReader(new StringReader(Data)))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 4; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				var ex = Assert.Throws<MissingRecordFieldException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(2, ex.CurrentRecordIndex);
				Assert.AreEqual(2, ex.CurrentColumnIndex);
			}
		}

		[Test]
		public void MissingFieldQuotedTest4()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\",\n\"3\",3,3,3";

			using (var csv = new DelimitedRecordReader(new StringReader(Data), 11))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 4; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				var ex = Assert.Throws<MissingRecordFieldException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(2, ex.CurrentRecordIndex);
				Assert.AreEqual(3, ex.CurrentColumnIndex);
			}
		}

		[Test]
		public void MissingDelimiterAfterQuotedFieldTest1()
		{
			const string Data = "\"111\",\"222\"\"333\"";

			using (var csv = new DelimitedRecordReader(new StringReader(Data)))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 3; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				csv.DoubleQuoteEscapingEnabled = false;
				csv.TrimWhiteSpaces = true;

				var ex = Assert.Throws<MalformedRecordException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(0, ex.CurrentRecordIndex);
				Assert.AreEqual(1, ex.CurrentColumnIndex);
			}
		}

		[Test]
		public void MissingDelimiterAfterQuotedFieldTest2()
		{
			const string Data = "\"111\",\"222\",\"333\"\n\"111\",\"222\"\"333\"";

			using (var csv = new DelimitedRecordReader(new StringReader(Data)))
			{
				csv.DynamicColumnCount = false;
				for (int i = 0; i < 3; i++)
					csv.Columns.Add(new DelimitedRecordColumn(csv.GetDefaultColumnName(i)));

				csv.DoubleQuoteEscapingEnabled = false;

				var ex = Assert.Throws<MalformedRecordException>(
					() =>
					{
						while (csv.Read() == ReadResult.Success)
						{
						}
					});

				Assert.AreEqual(1, ex.CurrentRecordIndex);
				Assert.AreEqual(1, ex.CurrentColumnIndex);
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public void MoreFieldsTest(bool dynamicColumnCount)
		{
			const string Data = "ORIGIN,DESTINATION\nPHL,FLL,kjhkj kjhkjh,eg,fhgf\nNYC,LAX";

			using (var csv = new DelimitedRecordReader(new StringReader(Data)))
			{
				csv.DynamicColumnCount = dynamicColumnCount;

				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(2, csv.Columns.Count);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual("PHL", csv[0]);
				Assert.AreEqual("FLL", csv[1]);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual("NYC", csv[0]);
				Assert.AreEqual("LAX", csv[1]);

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void MalformedTest1()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("11,12,13\n21,\"\"22\",23\n31,32,33")))
			{
				csv.ParseErrorAction = ParseErrorAction.SkipToNextLine;

				csv.DynamicColumnCount = true;
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual("11", csv[0]);
				Assert.AreEqual("12", csv[1]);
				Assert.AreEqual("13", csv[2]);

				csv.DynamicColumnCount = false;
				Assert.AreEqual(ReadResult.ParseError, csv.Read());
				Assert.AreEqual("21", csv[0]);
				Assert.AreEqual(null, csv[1]);
				Assert.AreEqual(null, csv[2]);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual("31", csv[0]);
				Assert.AreEqual("32", csv[1]);
				Assert.AreEqual("33", csv[2]);

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}
	}
}