using System;
using System.IO;
using System.Text;
using NLight.IO.Text;
using NUnit.Framework;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordWriterTests
{
	public class ClassTests
	{
		[Test]
		public void WhenWritingHeaderWithDataType_ColumnOrderingIsValid()
		{
			var sb = new StringBuilder();
			using (var writer = new DelimitedRecordWriter(new StringWriter(sb)))
			{
				writer.Columns.Add(new DelimitedRecordColumn("A"));
				writer.Columns.Add(new DelimitedRecordColumn("B"));
				writer.Columns.Add(new DelimitedRecordColumn("C"));
				writer.Columns.Add(new DelimitedRecordColumn("D"));
				writer.WriteColumnHeaders(true);
			}

			Assert.AreEqual("\"A:String\",\"B:String\",\"C:String\",\"D:String\"" + Environment.NewLine, sb.ToString());
		}

		[Test]
		public void WhenWritingHeaderWithNoDataType_ColumnOrderingIsValid()
		{
			var sb = new StringBuilder();
			using (var writer = new DelimitedRecordWriter(new StringWriter(sb)))
			{
				writer.Columns.Add(new DelimitedRecordColumn("A"));
				writer.Columns.Add(new DelimitedRecordColumn("B"));
				writer.Columns.Add(new DelimitedRecordColumn("C"));
				writer.Columns.Add(new DelimitedRecordColumn("D"));
				writer.WriteColumnHeaders(false);
			}

			Assert.AreEqual("\"A\",\"B\",\"C\",\"D\"" + Environment.NewLine, sb.ToString());
		}
	}
}