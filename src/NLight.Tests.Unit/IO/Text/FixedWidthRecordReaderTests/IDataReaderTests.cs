// Author(s): Sébastien Lorion

using NLight.IO.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace NLight.Tests.Unit.IO.Text.FixedWidthRecordReaderTests
{
	public class IDataReaderTests
		: NLight.Tests.Unit.BCL.Data.IDataReaderTests
	{
		protected override IDataReader CreateDataReaderInstance()
		{
			var reader = new FixedWidthRecordReader(new StringReader(FixedWidthRecordReaderTestData.SampleTypedData1));

			reader.Columns.Add(new FixedWidthRecordColumn("System.Boolean", typeof(System.Boolean), 0, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.DateTime", typeof(System.DateTime), 1, 10));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Single", typeof(System.Single), 11, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Double", typeof(System.Double), 12, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Decimal", typeof(System.Decimal), 13, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.SByte", typeof(System.SByte), 14, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Int16", typeof(System.Int16), 15, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Int32", typeof(System.Int32), 16, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Int64", typeof(System.Int64), 17, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Byte", typeof(System.Byte), 18, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.UInt16", typeof(System.UInt16), 19, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.UInt32", typeof(System.UInt32), 20, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.UInt64", typeof(System.UInt64), 21, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Char", typeof(System.Char), 22, 1));
			reader.Columns.Add(new FixedWidthRecordColumn("System.String", typeof(System.String), 23, 3));
			reader.Columns.Add(new FixedWidthRecordColumn("System.Guid", typeof(System.Guid), 26, 38));

			return reader;
		}

		protected override DataTable GetExpectedSchema()
		{
			var table = new DataTable();

			table.Columns.Add("System.Boolean", typeof(System.Boolean));
			table.Columns.Add("System.DateTime", typeof(System.DateTime));
			table.Columns.Add("System.Single", typeof(System.Single));
			table.Columns.Add("System.Double", typeof(System.Double));
			table.Columns.Add("System.Decimal", typeof(System.Decimal));
			table.Columns.Add("System.SByte", typeof(System.SByte));
			table.Columns.Add("System.Int16", typeof(System.Int16));
			table.Columns.Add("System.Int32", typeof(System.Int32));
			table.Columns.Add("System.Int64", typeof(System.Int64));
			table.Columns.Add("System.Byte", typeof(System.Byte));
			table.Columns.Add("System.UInt16", typeof(System.UInt16));
			table.Columns.Add("System.UInt32", typeof(System.UInt32));
			table.Columns.Add("System.UInt64", typeof(System.UInt64));
			table.Columns.Add("System.Char", typeof(System.Char));
			table.Columns.Add("System.String", typeof(System.String));
			table.Columns.Add("System.Guid", typeof(System.Guid));

			return table.CreateDataReader().GetSchemaTable();
		}

		protected override long GetExpectedRecordCount()
		{
			return FixedWidthRecordReaderTestData.SampleTypedData1Values.Length;
		}

		protected override IList<object> GetExpectedFieldValues(long recordIndex)
		{
			if (recordIndex < 0 || (int) recordIndex >= FixedWidthRecordReaderTestData.SampleTypedData1Values.Length)
				throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, null);

			return FixedWidthRecordReaderTestData.SampleTypedData1Values[(int) recordIndex];
		}
	}
}
