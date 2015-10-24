// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;

namespace NLight.Tests.Unit.IO.Text.FixedWidthRecordReaderTests
{
	public sealed class FixedWidthRecordReaderTestData
	{
		public const string SampleData1 = @"r0c0r0c1r0c2r0c3r0c4
r1c0r1c1r1c2r1c3r1c4
r2c0r2c1r2c2r2c3r2c4
r3c0r3c1r3c2r3c3r3c4
r4c0r4c1r4c2r4c3r4c4";

		private static string[][] _data1 = new string[][] {
			new string[] {"r0c0", "r0c1", "r0c2", "r0c3", "r0c4"},
			new string[] {"r1c0", "r1c1", "r1c2", "r1c3", "r1c4"},
			new string[] {"r2c0", "r2c1", "r2c2", "r2c3", "r2c4"},
			new string[] {"r3c0", "r3c1", "r3c2", "r3c3", "r3c4"},
			new string[] {"r4c0", "r4c1", "r4c2", "r4c3", "r4c4"}};

		public const int SampleData1RecordCount = 5;
		public const int SampleData1ColumnCount = 5;

		public const string SampleTypedData1 = @"12001-01-0111111111111aabc{11111111-1111-1111-1111-111111111111}
02001-01-0111111111111aabc{11111111-1111-1111-1111-111111111111}";

		public static object[][] SampleTypedData1Values = new object[][] {
			new object[] {true, new DateTime(2001,1,1), Convert.ToSingle(1),Convert.ToDouble(1), Convert.ToDecimal(1), Convert.ToSByte(1), Convert.ToInt16(1), Convert.ToInt32(1), Convert.ToInt64(1), Convert.ToByte(1), Convert.ToUInt16(1), Convert.ToUInt32(1), Convert.ToUInt64(1), 'a', "abc", new Guid("{11111111-1111-1111-1111-111111111111}")},
			new object[] {false, new DateTime(2001,1,1), Convert.ToSingle(1),Convert.ToDouble(1), Convert.ToDecimal(1), Convert.ToSByte(1), Convert.ToInt16(1), Convert.ToInt32(1), Convert.ToInt64(1), Convert.ToByte(1), Convert.ToUInt16(1), Convert.ToUInt32(1), Convert.ToUInt64(1), 'a', "abc", new Guid("{11111111-1111-1111-1111-111111111111}")}};

		public static void SetupReaderForSampleData1(FixedWidthRecordReader fix)
		{
			for (int i = 0; i < SampleData1ColumnCount; i++)
				fix.Columns.Add(new FixedWidthRecordColumn("c" + i.ToString(), i * 4, 4));
		}

		public static void CheckSampleData1(FixedWidthRecordReader fix, bool readToEnd)
		{
			if (!readToEnd)
				CheckSampleData1(fix, fix.CurrentRecordIndex);
			else
			{
				long recordIndex = 0;

				while (fix.Read() == ReadResult.Success)
				{
					CheckSampleData1(fix, recordIndex);
					recordIndex++;
				}

				Assert.AreEqual(SampleData1RecordCount - 1, fix.CurrentRecordIndex);
			}
		}

		public static void CheckSampleData1(FixedWidthRecordReader fix, long recordIndex)
		{
			Assert.AreEqual(recordIndex, fix.CurrentRecordIndex);
			Assert.AreEqual(SampleData1ColumnCount, fix.Columns.Count);

			for (int i = 0; i < _data1[recordIndex].Length; i++)
				Assert.AreEqual(_data1[recordIndex][i], fix[i]);
		}
	}
}