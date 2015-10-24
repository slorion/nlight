// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;
using System.IO;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public class ArgumentValidationTests
	{
		#region Constructors

		[Test]
		public void ArgumentTestCtor1()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				using (DelimitedRecordReader csv = new DelimitedRecordReader(null))
				{
				}
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1), 0))
				{
				}
			});

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1), -1))
				{
				}
			});
		}

		[Test]
		public void ArgumentTestCtor2()
		{
			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(""), 1))
			{
				Assert.AreEqual(1, csv.BufferSize);
			}
		}

		#endregion

		#region Indexers

		[Test]
		public void ArgumentTestIndexer()
		{
			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() =>
				{
					string s = csv[-1];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() =>
				{
					string s = csv[DelimitedRecordReaderTestData.SampleData1RecordCount];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());

				Assert.Throws<ArgumentOutOfRangeException>(() =>
				{
					string s = csv[-1];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());

				Assert.Throws<ArgumentOutOfRangeException>(() =>
				{
					string s = csv[DelimitedRecordReaderTestData.SampleData1RecordCount];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<InvalidOperationException>(() =>
				{
					string s = csv["asdf"];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<InvalidOperationException>(() =>
				{
					string s = csv[DelimitedRecordReaderTestData.SampleData1Header0];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());

				Assert.Throws<ArgumentNullException>(() =>
				{
					string s = csv[null];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());

				Assert.Throws<ArgumentNullException>(() =>
				{
					string s = csv[string.Empty];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(ReadResult.Success, csv.Read());

				Assert.Throws<ArgumentNullException>(() =>
				{
					string s = csv[null];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(ReadResult.Success, csv.Read());

				Assert.Throws<ArgumentNullException>(() =>
				{
					string s = csv[string.Empty];
				});
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.ReadColumnHeaders());
				Assert.AreEqual(ReadResult.Success, csv.Read());

				Assert.Throws<ArgumentException>(() =>
				{
					string s = csv["asdf"];
				});
			}
		}

		#endregion

		#region CopyCurrentRecordTo

		[Test]
		public void ArgumentTestCopyCurrentRecordTo()
		{
			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.Throws<ArgumentNullException>(() => csv.CopyCurrentRecordTo(null));
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.Throws<ArgumentOutOfRangeException>(() => csv.CopyCurrentRecordTo(new string[1], -1));
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.Throws<ArgumentOutOfRangeException>(() => csv.CopyCurrentRecordTo(new string[1], 1));
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.Throws<ArgumentOutOfRangeException>(() => csv.CopyCurrentRecordTo(new string[DelimitedRecordReaderTestData.SampleData1RecordCount - 1], 0));
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.Throws<ArgumentOutOfRangeException>(() => csv.CopyCurrentRecordTo(new string[DelimitedRecordReaderTestData.SampleData1RecordCount], 1));
			}
		}

		#endregion

		#region MoveTo

		[Test]
		public void ArgumentTestMoveTo()
		{
			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1)))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => csv.MoveTo(-1));
			}
		}

		#endregion
	}
}