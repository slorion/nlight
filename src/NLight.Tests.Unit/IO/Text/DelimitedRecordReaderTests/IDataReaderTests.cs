// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public class IDataReaderTests
		: NLight.Tests.Unit.BCL.Data.IDataReaderTests
	{
		protected override IDataReader CreateDataReaderInstance()
		{
			var reader = new DelimitedRecordReader(new StringReader(DelimitedRecordReaderTestData.SampleTypedData1));
			Assert.IsTrue(reader.ReadColumnHeaders() == ReadResult.Success);

			return reader;
		}

		protected override DataTable GetExpectedSchema()
		{
			var table = new DataTable();

			using (var sr = new StringReader(DelimitedRecordReaderTestData.SampleTypedData1))
			{
				string header = sr.ReadLine();
				foreach (var rawColumnName in header.Split(','))
				{
					string[] parsedColumnName = rawColumnName.Split(':');
					table.Columns.Add(parsedColumnName[0], Type.GetType(parsedColumnName[0]));
				}
			}

			return table.CreateDataReader().GetSchemaTable();
		}

		protected override long GetExpectedRecordCount()
		{
			return DelimitedRecordReaderTestData.SampleTypedData1Values.Length;
		}

		protected override IList<object> GetExpectedFieldValues(long recordIndex)
		{
			if (recordIndex < 0 || (int) recordIndex >= DelimitedRecordReaderTestData.SampleTypedData1Values.Length)
				throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, null);

			return DelimitedRecordReaderTestData.SampleTypedData1Values[(int) recordIndex];
		}
	}
}