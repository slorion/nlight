// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;
using System;

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	public static class DelimitedRecordReaderTestData
	{
		public const string SampleData1 = @"
# This is a comment
""First Name"", ""Last Name"", Address, City, State, ""Zip Code""	
John,Doe,120 jefferson st.,Riverside, NJ, 08075
Jack,McGinnis,220 hobo Av.,Phila	, PA,09119
""John """"Da Man"""""",Repici,120 Jefferson St.,Riverside, NJ,08075

# This is a comment
Stephen,Tyler,""7452 Terrace """"At the Plaza"""" road"",SomeTown,SD, 91234
,Blankman,,SomeTown, SD, 00298
""Joan """"the bone"""", Anne"",Jet,""9th, at Terrace plc"",Desert City,CO,00123";

		private static string[][] _data1 = new string[][] {
			new string[] {"First Name", "Last Name", "Address", "City", "State", "Zip Code"},
			new string[] {"John", "Doe", "120 jefferson st.", "Riverside", "NJ", "08075"},
			new string[] {"Jack", "McGinnis", "220 hobo Av.", "Phila", "PA", "09119"},
			new string[] {@"John ""Da Man""", "Repici", "120 Jefferson St.", "Riverside", "NJ", "08075"},
			new string[] {"Stephen", "Tyler", @"7452 Terrace ""At the Plaza"" road", "SomeTown", "SD", "91234"},
			new string[] {"", "Blankman", "", "SomeTown", "SD", "00298"},
			new string[] {@"Joan ""the bone"", Anne", "Jet", "9th, at Terrace plc", "Desert City", "CO", "00123"}};

		public const int SampleData1RecordCount = 6;
		public const int SampleData1ColumnCount = 6;

		public const string SampleData1Header0 = "First Name";
		public const string SampleData1Header1 = "Last Name";
		public const string SampleData1Header2 = "Address";
		public const string SampleData1Header3 = "City";
		public const string SampleData1Header4 = "State";
		public const string SampleData1Header5 = "Zip Code";

		public const string SampleTypedData1 = @"System.Boolean:bool,System.DateTime:date,System.Single:float,System.Double:double,System.Decimal:decimal,System.SByte:sbyte,System.Int16:int16,System.Int32:int32,System.Int64:int64,System.Byte:byte,System.UInt16:uint16,System.UInt32:uint32,System.UInt64:uint64,System.Char:char,System.String:string,System.Guid:guid
1,2001-01-01,1,1,1,1,1,1,1,1,1,1,1,a,abc,{11111111-1111-1111-1111-111111111111}
""true"",""2001-01-01"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""a"",""abc"",""{11111111-1111-1111-1111-111111111111}""";

		public static object[][] SampleTypedData1Values = new object[][] {
			new object[] {true, new DateTime(2001,1,1), Convert.ToSingle(1),Convert.ToDouble(1), Convert.ToDecimal(1), Convert.ToSByte(1), Convert.ToInt16(1), Convert.ToInt32(1), Convert.ToInt64(1), Convert.ToByte(1), Convert.ToUInt16(1), Convert.ToUInt32(1), Convert.ToUInt64(1), 'a', "abc", new Guid("{11111111-1111-1111-1111-111111111111}")},
			new object[] {true, new DateTime(2001,1,1), Convert.ToSingle(1),Convert.ToDouble(1), Convert.ToDecimal(1), Convert.ToSByte(1), Convert.ToInt16(1), Convert.ToInt32(1), Convert.ToInt64(1), Convert.ToByte(1), Convert.ToUInt16(1), Convert.ToUInt32(1), Convert.ToUInt64(1), 'a', "abc", new Guid("{11111111-1111-1111-1111-111111111111}")}};

		public static void CheckSampleData1(DelimitedRecordReader csv, bool hasHeaders, bool readToEnd)
		{
			if (!readToEnd)
				CheckSampleData1(csv, hasHeaders, csv.CurrentRecordIndex);
			else
			{
				long recordIndex = 0;

				while (csv.Read() == ReadResult.Success)
				{
					CheckSampleData1(csv, hasHeaders, recordIndex);
					recordIndex++;
				}

				if (hasHeaders)
					Assert.AreEqual(SampleData1RecordCount - 1, csv.CurrentRecordIndex);
				else
					Assert.AreEqual(SampleData1RecordCount, csv.CurrentRecordIndex);
			}
		}

		public static void CheckSampleData1(DelimitedRecordReader csv, bool hasHeaders, long recordIndex)
		{
			Assert.AreEqual(recordIndex, csv.CurrentRecordIndex);
			Assert.AreEqual(SampleData1ColumnCount, csv.Columns.Count);

			long actualRecordIndex = (hasHeaders ? recordIndex + 1 : recordIndex);

			for (int i = 0; i < _data1[actualRecordIndex].Length; i++)
				Assert.AreEqual(_data1[actualRecordIndex][i], csv[i]);
		}

		public static void CheckSampleData1(string[] fields, bool hasHeaders, long recordIndex, int startIndex)
		{
			long actualRecordIndex = (hasHeaders ? recordIndex + 1 : recordIndex);

			Assert.That(fields.Length - startIndex, Is.GreaterThanOrEqualTo(_data1[actualRecordIndex].Length));

			for (int i = 0; i < _data1[actualRecordIndex].Length; i++)
				Assert.AreEqual(_data1[actualRecordIndex][i], fields[startIndex + i]);
		}
	}
}