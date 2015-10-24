// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public class UnicodeParsingTests
	{
		[Test]
		public void UnicodeParsingTest1()
		{
			// control characters and comma are skipped

			char[] raw = new char[65536 - 13];

			for (int i = 0; i < raw.Length; i++)
				raw[i] = (char) (i + 14);

			raw[44 - 14] = ' '; // skip comma

			string data = new string(raw);

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StringReader(data)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(data, csv[0]);
				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void UnicodeParsingTest2()
		{
			byte[] buffer;

			string test = "München";

			using (MemoryStream stream = new MemoryStream())
			{
				using (TextWriter writer = new StreamWriter(stream, Encoding.Unicode))
				{
					writer.WriteLine(test);
				}

				buffer = stream.ToArray();
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StreamReader(new MemoryStream(buffer), Encoding.Unicode, false)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(test, csv[0]);
				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}

		[Test]
		public void UnicodeParsingTest3()
		{
			const string test = "München";

			byte[] buffer;

			using (MemoryStream stream = new MemoryStream())
			{
				using (TextWriter writer = new StreamWriter(stream, Encoding.Unicode))
				{
					writer.Write(test);
				}

				buffer = stream.ToArray();
			}

			using (DelimitedRecordReader csv = new DelimitedRecordReader(new StreamReader(new MemoryStream(buffer), Encoding.Unicode, false)))
			{
				Assert.AreEqual(ReadResult.Success, csv.Read());
				Assert.AreEqual(test, csv[0]);
				Assert.AreEqual(ReadResult.EndOfFile, csv.Read());
			}
		}
	}
}