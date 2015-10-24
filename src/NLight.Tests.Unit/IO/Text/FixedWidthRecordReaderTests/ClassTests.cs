// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace NLight.Tests.Unit.IO.Text.FixedWidthRecordReaderTests
{
	public partial class ClassTests
	{
		#region MoveTo tests

		[Test]
		public void MoveToTest1()
		{
			using (var fix = new FixedWidthRecordReader(new StringReader(FixedWidthRecordReaderTestData.SampleData1)))
			{
				FixedWidthRecordReaderTestData.SetupReaderForSampleData1(fix);

				for (int i = 0; i < FixedWidthRecordReaderTestData.SampleData1RecordCount; i++)
				{
					fix.MoveTo(i);
					FixedWidthRecordReaderTestData.CheckSampleData1(fix, i);
				}
			}
		}

		//TODO: revise
		//[Test]
		//public void MoveToTest2()
		//{
		//	using (var fix = new FixedWidthRecordReader(new StringReader(FixedWidthRecordReaderTestData.SampleData1)))
		//	{
		//		FixedWidthRecordReaderTestData.SetupReaderForSampleData1(fix);

		//		fix.MoveTo(1);

		//		Assert.Throws<InvalidOperationException>(() => fix.MoveTo(0));
		//	}
		//}

		[Test]
		public void MoveToTest3()
		{
			using (var fix = new FixedWidthRecordReader(new StringReader(FixedWidthRecordReaderTestData.SampleData1)))
			{
				FixedWidthRecordReaderTestData.SetupReaderForSampleData1(fix);
				Assert.AreEqual(ReadResult.EndOfFile, fix.MoveTo(FixedWidthRecordReaderTestData.SampleData1RecordCount));
			}
		}

		[Test]
		public void MoveToTest_SeekableReader()
		{
			using (var ms = new MemoryStream())
			{
				var writer = new StreamWriter(ms, Encoding.Unicode);
				writer.Write(FixedWidthRecordReaderTestData.SampleData1);
				writer.Flush();

				ms.Position = 0;

				using (var fix = new FixedWidthRecordReader(new StreamReader(ms, Encoding.Unicode)))
				{
					fix.StartCachingRecordPositions();
					FixedWidthRecordReaderTestData.SetupReaderForSampleData1(fix);

					while (fix.Read() == ReadResult.Success)
					{
					}

					for (int i = FixedWidthRecordReaderTestData.SampleData1RecordCount - 1; i >= 0; i--)
					{
						Assert.AreEqual(ReadResult.Success, fix.MoveTo(i));
						FixedWidthRecordReaderTestData.CheckSampleData1(fix, i);
					}
				}
			}
		}

		//TODO: test MoveTo with start/stop record position caching

		#endregion

		#region SkipEmptyLines

		[Test]
		public void SkipEmptyLinesTest1()
		{
			using (var fix = new FixedWidthRecordReader(new StringReader("00\n\n10")))
			{
				fix.Columns.Add(new FixedWidthRecordColumn("c1", 0, 2));
				fix.SkipEmptyLines = false;
				fix.MissingFieldAction = MissingRecordFieldAction.ReturnEmptyValue;

				Assert.AreEqual(ReadResult.Success, fix.Read());
				Assert.AreEqual(1, fix.Columns.Count);
				Assert.AreEqual("00", fix[0]);

				Assert.AreEqual(ReadResult.Success, fix.Read());
				Assert.AreEqual(1, fix.Columns.Count);
				Assert.AreEqual(string.Empty, fix[0]);

				Assert.AreEqual(ReadResult.Success, fix.Read());
				Assert.AreEqual(1, fix.Columns.Count);
				Assert.AreEqual("10", fix[0]);

				Assert.AreEqual(ReadResult.EndOfFile, fix.Read());
			}
		}

		[Test]
		public void SkipEmptyLinesTest2()
		{
			using (var fix = new FixedWidthRecordReader(new StringReader("00\n\n10")))
			{
				fix.Columns.Add(new FixedWidthRecordColumn("c1", 0, 2));
				fix.SkipEmptyLines = true;

				Assert.AreEqual(ReadResult.Success, fix.Read());
				Assert.AreEqual(1, fix.Columns.Count);
				Assert.AreEqual("00", fix[0]);

				Assert.AreEqual(ReadResult.Success, fix.Read());
				Assert.AreEqual(1, fix.Columns.Count);
				Assert.AreEqual("10", fix[0]);

				Assert.AreEqual(ReadResult.EndOfFile, fix.Read());
			}
		}

		#endregion
	}
}