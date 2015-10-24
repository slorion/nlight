// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public partial class ClassTests
	{
		[Test]
		public void FieldCountTest1()
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				DelimitedRecordReaderTestData.CheckSampleData1(csv, true, true);
			}
		}

		[Test]
		public void ReadColumnHeadersTest_NotCalledWithDynamicColumns()
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				csv.DynamicColumnCount = true;
				DelimitedRecordReaderTestData.CheckSampleData1(csv, false, true);
			}
		}

		[Test]
		public void ReadColumnHeadersTest_NotCalledWithoutDynamicColumns()
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				csv.DynamicColumnCount = false;

				while (csv.Read() == ReadResult.Success)
					Assert.AreEqual(0, csv.Columns.Count);
			}
		}

		[Test]
		public void ReadColumnHeadersTest_Called([Values(false, true)] bool useDynamicColumnCount)
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				csv.DynamicColumnCount = useDynamicColumnCount;

				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());

				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1RecordCount, csv.Columns.Count);

				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header0, csv.Columns[0].Name);
				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header1, csv.Columns[1].Name);
				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header2, csv.Columns[2].Name);
				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header3, csv.Columns[3].Name);
				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header4, csv.Columns[4].Name);
				Assert.AreEqual(DelimitedRecordReaderTestData.SampleData1Header5, csv.Columns[5].Name);

				Assert.AreEqual(0, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header0));
				Assert.AreEqual(1, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header1));
				Assert.AreEqual(2, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header2));
				Assert.AreEqual(3, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header3));
				Assert.AreEqual(4, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header4));
				Assert.AreEqual(5, csv.Columns.IndexOf(DelimitedRecordReaderTestData.SampleData1Header5));

				DelimitedRecordReaderTestData.CheckSampleData1(csv, true, true);
			}
		}

		[Test]
		[TestCase("")]
		[TestCase("#")]
		[TestCase("#asdf\n\n#asdf,asdf")]
		[TestCase("\n")]
		[TestCase("\n\n\n\n")]
		public void ReadColumnHeadersTest_EmptyCsv(string data)
		{
			using (var csv = new DelimitedRecordReader(new StringReader(data)))
			{
				csv.CommentCharacter = '#';
				csv.SkipEmptyLines = true;

				Assert.AreEqual(ReadResult.EndOfFile, csv.ReadColumnHeaders());
				Assert.AreEqual(0, csv.Columns.Count);
			}
		}

		[TestCase((string) null)]
		[TestCase("")]
		[TestCase("AnotherName")]
		public void ReadColumnHeadersTest_WithEmptyColumnNames(string defaultColumnName)
		{
			if (defaultColumnName == null)
				defaultColumnName = "Column";

			using (var csv = new DelimitedRecordReader(new StringReader(",  ,,aaa,\"   \",,,")))
			{
				csv.DefaultColumnNamePrefix = defaultColumnName;

				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(8, csv.Columns.Count);

				Assert.AreEqual("aaa", csv.Columns[3].Name);
				foreach (var index in new int[] { 0, 1, 2, 4, 5, 6, 7 })
					Assert.AreEqual(defaultColumnName + index.ToString(), csv.Columns[index].Name);
			}
		}

		[Test]
		public void ReadColumnHeadersTest_TypedColumns()
		{
			Triple<string, Type, object>[] supported = new Triple<string, Type, object>[] {
				new Triple<string, Type, object>("string", typeof(string), default(string)),
				new Triple<string, Type, object>("char", typeof(char), default(char)),
				new Triple<string, Type, object>("sbyte", typeof(sbyte), default(sbyte)),
				new Triple<string, Type, object>("byte", typeof(byte), default(byte)),
				new Triple<string, Type, object>("short", typeof(short), default(short)),
				new Triple<string, Type, object>("ushort", typeof(ushort), default(ushort)),
				new Triple<string, Type, object>("int", typeof(int), default(int)),
				new Triple<string, Type, object>("uint", typeof(uint), default(uint)),
				new Triple<string, Type, object>("long", typeof(long), default(long)),
				new Triple<string, Type, object>("ulong", typeof(ulong), default(ulong)),
				new Triple<string, Type, object>("float", typeof(float), default(float)),
				new Triple<string, Type, object>("double", typeof(double), default(double)),
				new Triple<string, Type, object>("decimal", typeof(decimal), default(decimal)),
				new Triple<string, Type, object>("DateTime", typeof(DateTime), default(DateTime)),
				new Triple<string, Type, object>("TimeSpan", typeof(TimeSpan), default(TimeSpan)),
				new Triple<string, Type, object>("String", typeof(String), default(String)),
				new Triple<string, Type, object>("Char", typeof(Char), default(Char)),
				new Triple<string, Type, object>("SByte", typeof(SByte), default(SByte)),
				new Triple<string, Type, object>("Byte", typeof(Byte), default(Byte)),
				new Triple<string, Type, object>("Int16", typeof(Int16), default(Int16)),
				new Triple<string, Type, object>("UInt16", typeof(UInt16), default(UInt16)),
				new Triple<string, Type, object>("Int32", typeof(Int32), default(Int32)),
				new Triple<string, Type, object>("UInt32", typeof(UInt32), default(UInt32)),
				new Triple<string, Type, object>("Int64", typeof(Int64), default(Int64)),
				new Triple<string, Type, object>("UInt64", typeof(UInt64), default(UInt64)),
				new Triple<string, Type, object>("Single", typeof(Single), default(Single)),
				new Triple<string, Type, object>("Double", typeof(Double), default(Double)),
				new Triple<string, Type, object>("Decimal", typeof(Decimal), default(Decimal))
			};

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < supported.Length; i++)
			{
				Triple<string, Type, object> entry = supported[i];

				sb.Append(entry.First);
				sb.Append(i);
				sb.Append(':');
				sb.Append(entry.First);

				if (i < supported.Length - 1)
					sb.Append(',');
			}

			using (var csv = new DelimitedRecordReader(new StringReader(sb.ToString())))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(supported.Length, csv.Columns.Count);

				for (int i = 0; i < supported.Length; i++)
				{
					Assert.AreEqual(supported[i].Second, csv.Columns[i].DataType);
					Assert.AreEqual(supported[i].Third, csv.Columns[i].DefaultValue);
				}

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void CopyCurrentRecordToTest([Range(0, 5)] int startIndex)
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				var record = new string[DelimitedRecordReaderTestData.SampleData1ColumnCount + startIndex];

				while (csv.Read() == ReadResult.Success)
				{
					csv.CopyCurrentRecordTo(record, startIndex);
					DelimitedRecordReaderTestData.CheckSampleData1(record, false, csv.CurrentRecordIndex, startIndex);
				}
			}
		}

		[Test]
		public void CopyCurrentRecordToTest_NoCurrentRecord()
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<InvalidOperationException>(() => csv.CopyCurrentRecordTo(new string[DelimitedRecordReaderTestData.SampleData1RecordCount]));
			}
		}

		[Test]
		public void MoveToTest_Forward(
			[Values(false, true)] bool readHeaders,
			[Range(0, DelimitedRecordReaderTestData.SampleData1RecordCount - 1)] int toRecordIndex)
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				if (readHeaders)
					Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());

				csv.MoveTo(toRecordIndex);
				DelimitedRecordReaderTestData.CheckSampleData1(csv, readHeaders, toRecordIndex);
			}
		}

		//TODO: revise
		//[Test]
		//public void MoveToTest_BackwardCannotSeek()
		//{
		//	using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
		//	{
		//		Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());

		//		csv.MoveTo(1);

		//		Assert.Throws<InvalidOperationException>(() => csv.MoveTo(0));
		//	}
		//}

		[Test]
		public void MoveToTest_AfterLastRecord()
		{
			using (var csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(ReadResult.EndOfFile, csv.MoveTo(DelimitedRecordReaderTestData.SampleData1RecordCount));
			}
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void MoveToTest_SeekableStream(bool withCaching)
		{
			using (var ms = new MemoryStream())
			{
				var writer = new StreamWriter(ms, Encoding.Unicode);
				writer.Write(DelimitedRecordReaderTestData.SampleData1);
				writer.Flush();

				ms.Position = 0;

				using (var csv = new DelimitedRecordReader(new StreamReader(ms, Encoding.Unicode)))
				{
					if (withCaching)
						csv.StartCachingRecordPositions();

					while (csv.Read() == ReadResult.Success)
					{
					}

					for (int i = DelimitedRecordReaderTestData.SampleData1RecordCount - 1; i >= 0; i--)
					{
						Assert.AreEqual(ReadResult.Success, csv.MoveTo(i));
						DelimitedRecordReaderTestData.CheckSampleData1(csv, false, i);
					}
				}
			}
		}

		[Test]
		public void SkipEmptyLinesTest1()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("00\n\n10")))
			{
				csv.SkipEmptyLines = false;
				csv.MissingFieldAction = MissingRecordFieldAction.ReturnEmptyValue;

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("00", csv[0]);

				// if dynamic column count stays active, there will be 0 column for the empty line
				csv.DynamicColumnCount = false;

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual(string.Empty, csv[0]);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("10", csv[0]);

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void SkipEmptyLinesTest2()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("00\n\n10")))
			{
				csv.SkipEmptyLines = true;

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("00", csv[0]);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("10", csv[0]);

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void SkipEmptyLinesTest3()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("00\n\n10")))
			{
				csv.SkipEmptyLines = false;
				csv.MissingFieldAction = MissingRecordFieldAction.ReturnEmptyValue;

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("00", csv[0]);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(0, csv.Columns.Count);

				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(1, csv.Columns.Count);
				Assert.AreEqual("10", csv[0]);

				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void DataReaderGetNameWithNoCurrentRecordTest()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("0,1,2")))
			{
				csv.ReadColumnHeaders();

				for (int i = 0; i < 3; i++)
					Assert.AreEqual(i.ToString(), ((System.Data.IDataReader) csv).GetName(i));
			}
		}

		[Test]
		public void DataReaderGetOrdinalWithNoCurrentRecordTest()
		{
			using (var csv = new DelimitedRecordReader(new StringReader("0,1,2")))
			{
				csv.ReadColumnHeaders();

				for (int i = 0; i < 3; i++)
					Assert.AreEqual(i, ((System.Data.IDataReader) csv).GetOrdinal(i.ToString()));
			}
		}
	}
}