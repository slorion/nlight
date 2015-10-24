// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public class ParsingTests
	{
		[Flags]
		public enum ReaderOptions
		{
			None = 0,
			AdvancedEscaping = 0x1,
			NoDoubleQuoteEscaping = 0x2,
			NoSkipEmptyLines = 0x4,
			NoTrim = 0x8
		}

		[TestCase(1)]
		[TestCase(9)]
		[TestCase(14)]
		[TestCase(39)]
		[TestCase(67)]
		[TestCase(166)]
		[TestCase(194)]
		[TestCase(204)]
		[TestCase(1024)]
		public void ParsingTest_SpecificBufferSize([Random(1, 1024, 1000)] int bufferSize)
		{
			using (DelimitedRecordReader reader = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleData1), bufferSize))
			{
				Assert.AreEqual(ReadResult.Success, reader.ReadColumnHeaders());
				DelimitedRecordReaderTestData.CheckSampleData1(reader, true, true);
			}
		}

		[TestCase(10)]
		[TestCase(34)]
		public void AvancedParsingTest_SpecificBufferSize([Random(10, 1024, 1000)] int bufferSize)
		{
			const string Data = @"\u0031\u0032\u033\u34\x0035\x0036\x037\x38\u0039,\d00049\d0050\d051\d52\o000065\o00066\o0067\o070\o71
									""\n\t\\\r\f\v\a\b\e""";

			string[] values = { "123456789", "123456789", "\n\t\\\r\f\v\a\b\u001b" };

			using (DelimitedRecordReader reader = new DelimitedRecordReader(new StringReader(Data), bufferSize))
			{
				reader.AdvancedEscapingEnabled = true;

				Assert.AreEqual(ReadResult.Success, reader.Read());
				Assert.AreEqual(values[0], reader[0]);
				Assert.AreEqual(values[1], reader[1]);

				Assert.AreEqual(ReadResult.Success, reader.Read());
				Assert.AreEqual(values[2], reader[0]);

				Assert.AreEqual(ReadResult.EndOfFile, reader.Read());
			}
		}

		//Standard parsing
		[CLSCompliant(false)]
		[TestCase("1\r\n\r\n1", null, null, null, null, null, null, new string[] { "1", "1" })]
		[TestCase("\"Bob said, \"\"Hey!\"\"\",2, 3 ", null, null, null, null, null, null, new string[] { @"Bob said, ""Hey!""|2|3" })]
		[TestCase("\"Bob said, \"\"Hey!\"\"\",2, 3 ", null, ReaderOptions.NoTrim, null, null, null, null, new string[] { @"Bob said, ""Hey!""|2| 3 " })]
		[TestCase("1\r2\n", null, null, null, null, null, null, new string[] { "1", "2" })]
		[TestCase("\"\n\r\n\n\r\r\",,\t,\n", null, null, null, null, null, null, new string[] { "\n\r\n\n\r\r|||" })]
		[TestCase("1,2", null, null, null, null, null, null, new string[] { "1|2" })]
		[TestCase("\r\n1\r\n", null, null, null, null, null, null, new string[] { "1" })]
		[TestCase("\"bob said, \"\"Hey!\"\"\",2, 3 ", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "bob said, \"Hey!\"|2|3" })]
		[TestCase(",", null, null, null, null, null, null, new string[] { "|" })]
		[TestCase("1\r2", null, null, null, null, null, null, new string[] { "1", "2" })]
		[TestCase("1\n2", null, null, null, null, null, null, new string[] { "1", "2" })]
		[TestCase("1\r\n2", null, null, null, null, null, null, new string[] { "1", "2" })]
		[TestCase("1\n\r2", null, null, null, null, null, null, new string[] { "1", "2" })]
		[TestCase("1\r", null, null, null, null, null, null, new string[] { "1" })]
		[TestCase("1\n", null, null, null, null, null, null, new string[] { "1" })]
		[TestCase("1\r\n", null, null, null, null, null, null, new string[] { "1" })]
		[TestCase("1\n\r", null, null, null, null, null, null, new string[] { "1" })]
		[TestCase("1\r2\n", null, null, '\r', null, null, null, new string[] { "1|2" })]
		[TestCase("\"July 4th, 2005\"", null, null, null, null, null, null, new string[] { "July 4th, 2005" })]
		[TestCase(" 1", null, ReaderOptions.NoTrim, null, null, null, null, new string[] { " 1" })]
		[TestCase("", null, null, null, null, null, null, new string[] { })]
		[TestCase("user_id,name\r\n1,Bruce", null, null, null, null, null, "user_id|name", new string[] { "1|Bruce" })]
		[TestCase("\"data \r\n here\"", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "data \r\n here" })]
		[TestCase("\r\r\n1\r", null, ReaderOptions.NoTrim, '\r', null, null, null, new string[] { "||", "1|" })]
		[TestCase("\"double\"\"\"\"double quotes\"", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "double\"\"double quotes" })]
		[TestCase("'bob said, ''Hey!''',2, 3 ", null, null, null, '\'', null, null, new string[] { "bob said, 'Hey!'|2|3" })]
		[TestCase("\"data \"\" here\"", null, ReaderOptions.AdvancedEscaping, null, '\0', null, null, new string[] { "\"data \"\" here\"" })]
		[TestCase("1\r\n\r\n1", null, null, null, null, null, null, new string[] { "1", "1" })]
		[TestCase("1\r\n# bunch of crazy stuff here\r\n1", null, null, null, null, null, null, new string[] { "1", "1" })]
		[TestCase("\"1\",Bruce\r\n\"2\n\",Toni\r\n\"3\",Brian\r\n", null, null, null, null, null, null, new string[] { "1|Bruce", "2\n|Toni", "3|Brian" })]
		[TestCase("\"double\\\\\\\\double backslash\"", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "double\\\\double backslash" })]
		[TestCase("foo,\"bar,baz\"", null, null, null, null, null, null, new string[] { "foo|bar,baz" })]
		[TestCase("\t", null, null, '\t', null, null, null, new string[] { "|" })]
		[TestCase("abc,def,ghi\n", null, null, null, null, null, null, new string[] { "abc|def|ghi" })]
		[TestCase("00,01,   \n10,11,   ", 1, null, null, null, null, null, new string[] { "00|01|", "10|11|" })]
		[TestCase("\"00\",\n\"10\",", null, null, null, null, null, null, new string[] { "00|", "10|" })]
		[TestCase("First record          ,Second record", 16, null, null, null, null, null, new string[] { "First record|Second record" })]
		//Advanced escaping
		[TestCase(@"\u0031\u0032\u033\u34\x0035\x0036\x037\x38\u0039,\d00049\d0050\d051\d52\o000065\o00066\o0067\o070\o71", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "123456789|123456789" })]
		[TestCase(@"""\n\t\\\r\f\v\a\b\e""", null, ReaderOptions.AdvancedEscaping, null, null, null, null, new string[] { "\n\t\\\r\f\v\a\b\u001b" })]
		public void ParsingTest(string data, int? bufferSize, ReaderOptions? options, char? delimiter, char? quote, char? comment, string expectedColumnHeaders, IEnumerable<string> expectedRecords)
		{
			using (var reader = new DelimitedRecordReader(new StringReader(data), bufferSize.HasValue ? bufferSize.Value : DelimitedRecordReader.DefaultBufferSize))
			{
				if (options != null)
				{
					reader.AdvancedEscapingEnabled = (options & ReaderOptions.AdvancedEscaping) != 0;
					reader.DoubleQuoteEscapingEnabled = (options & ReaderOptions.NoDoubleQuoteEscaping) == 0;
					reader.SkipEmptyLines = (options & ReaderOptions.NoSkipEmptyLines) == 0;
					reader.TrimWhiteSpaces = (options & ReaderOptions.NoTrim) == 0;
				}

				if (comment != null)
					reader.CommentCharacter = comment.Value;

				if (delimiter != null)
					reader.DelimiterCharacter = delimiter.Value;

				if (quote != null)
					reader.QuoteCharacter = quote.Value;

				string[] headers = null;

				if (!string.IsNullOrEmpty(expectedColumnHeaders))
				{
					reader.DynamicColumnCount = false;
					Assert.AreEqual(ReadResult.Success, reader.ReadColumnHeaders());

					headers = expectedColumnHeaders.Split('|');

					Assert.AreEqual(headers.Length, reader.Columns.Count);

					for (int i = 0; i < headers.Length; i++)
						Assert.AreEqual(headers[i], reader.Columns[i].Name);
				}

				foreach (var record in expectedRecords)
				{
					Assert.AreEqual(ReadResult.Success, reader.Read());

					string[] values = record.Split('|');

					if (headers != null)
						Assert.AreEqual(headers.Length, values.Length);

					Assert.AreEqual(values.Length, reader.Columns.Count);

					for (int columnIndex = 0; columnIndex < values.Length; columnIndex++)
						Assert.AreEqual(values[columnIndex], reader[columnIndex]);

					if (headers != null)
					{
						for (int columnIndex = 0; columnIndex < values.Length; columnIndex++)
							Assert.AreEqual(values[columnIndex], reader[headers[columnIndex]]);
					}
				}

				Assert.AreEqual(ReadResult.EndOfFile, reader.Read());
			}
		}
	}
}