// Author(s): Sébastien Lorion

using DataStreams.FixedWidth;
using NLight.IO.Text;
using System.IO;

namespace NLight.Tests.Benchmarks.IO.Text
{
	public static class FixedRecordReaderBenchmarks
	{
		private const int ColumnCount = 6;
		private const int ColumnWidth = 5;

		public static void ReadAll(FixedRecordReaderBenchmarkArguments args)
		{
			using (var reader = new FixedWidthRecordReader(new StreamReader(args.Path, args.Encoding, true, args.BufferSize), args.BufferSize))
			{
				reader.SkipEmptyLines = args.SkipEmptyLines;

				for (int i = 0; i < ColumnCount; i++)
					reader.Columns.Add(new FixedWidthRecordColumn(reader.GetDefaultColumnName(i), typeof(string), string.Empty, i * ColumnWidth, ColumnWidth));

				string s;

				if (args.FieldIndex < 0)
				{
					while (reader.Read() != ReadResult.EndOfFile)
					{
						for (int i = 0; i < reader.Columns.Count - 1; i++)
							s = reader[i];
					}
				}
				else
				{
					while (reader.Read() != ReadResult.EndOfFile)
					{
						for (int i = 0; i < args.FieldIndex + 1; i++)
							s = reader[i];
					}
				}
			}
		}

		public static void ReadAll_DataStreams(FixedRecordReaderBenchmarkArguments args)
		{
			using (var reader = new FixedWidthReader(new StreamReader(args.Path, args.Encoding, true, args.BufferSize)))
			{
				reader.Settings.CaptureRawRecord = false;

				for (int i = 0; i < ColumnCount; i++)
					reader.Columns.Add(ColumnWidth);

				string s;

				if (args.FieldIndex < 0)
				{
					while (reader.ReadRecord())
					{
						for (int i = 0; i < reader.Columns.Count - 1; i++)
							s = reader[i];
					}
				}
				else
				{
					while (reader.ReadRecord())
					{
						for (int i = 0; i < args.FieldIndex + 1; i++)
							s = reader[i];
					}
				}
			}
		}
	}
}