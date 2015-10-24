// Author(s): Sébastien Lorion

using System;
using System.Diagnostics;

using NLight.Diagnostics.Benchmarking;
using NLight.Text;
using NLight.Tests.Benchmarks.IO.Text;
using NLight.Text.Encodings;

namespace NLight.Tests.Benchmarks
{
	class Program
	{
		//TODO: for now, the main program code is very tied to IO benchmarks. Needs to refactor to make it more modular.

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			var benchmarkOptions = new BenchmarkOptions { ActionIterationCount = 1, BenchmarkIterationCount = 3, ConcurrencyLevel = 1 };

			var tests = BenchmarkTests.All;
			bool profiling = false;

			var converter = new StringValueConverter();

			if (args.Length > 0)
			{
				tests = (BenchmarkTests) converter.ConvertTo(args[0], TrimmingOptions.Both, typeof(BenchmarkTests), BenchmarkTests.All);

				if (args.Length > 1)
					benchmarkOptions.BenchmarkIterationCount = converter.ConvertToInt64(args[1], TrimmingOptions.Both, benchmarkOptions.BenchmarkIterationCount, null);

				if (args.Length > 2)
					profiling = converter.ConvertToBoolean(args[2], TrimmingOptions.Both, false, null);
			}

			#region FixedWidthReader

			if (tests.HasFlag(BenchmarkTests.FixedWidthReader))
			{
				var files = profiling
					? new string[] { @"IO\Text\files\fixed.txt" }
					: new string[] { @"IO\Text\files\fixed.txt", @"IO\Text\files\test1.csv", @"IO\Text\files\test2.csv" };

				foreach (var file in files)
				{
					Console.WriteLine("--- FixedWidthReader - {0} ---", file);

					var benchmarkArgs = new FixedRecordReaderBenchmarkArguments();
					benchmarkArgs.Path = file;

					Benchmark.Execute("NLight", benchmarkOptions, benchmarkArgs, OutputResults, FixedRecordReaderBenchmarks.ReadAll);
					Benchmark.Execute("DataStreams", benchmarkOptions, benchmarkArgs, OutputResults, FixedRecordReaderBenchmarks.ReadAll_DataStreams);
				}
			}

			#endregion

			#region DelimitedReader

			if (tests.HasFlag(BenchmarkTests.DelimitedReader))
			{
				var files = profiling
					? new string[] { @"IO\Text\files\test1.csv" }
					: new string[] { @"IO\Text\files\test1.csv", @"IO\Text\files\test2.csv", @"IO\Text\files\test3.csv", @"IO\Text\files\test4.csv", @"IO\Text\files\test5.csv" };

				foreach (var file in files)
				{
					Console.WriteLine("--- DelimitedReader - {0} ---", file);

					var benchmarkArgs = new DelimitedRecordReaderBenchmarkArguments();
					benchmarkArgs.Path = file;
					benchmarkArgs.TrimWhiteSpaces = true;

					Benchmark.Execute("LumenWorks", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll_LumenWorks);
					Benchmark.Execute("NLight", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll);
					Benchmark.Execute("DataStreams", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll_DataStreams);
					//Benchmark.Execute("OleDb", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll_OleDb);
					//Benchmark.Execute("Regex", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll_Regex);
				}
			}

			#endregion

			#region DelimitedReaderAdvancedEscaping

			if (tests.HasFlag(BenchmarkTests.DelimitedReaderAdvancedEscaping))
			{
				var files = profiling
					? new string[] { @"IO\Text\files\test4.csv" }
					: new string[] { @"IO\Text\files\test3.csv", @"IO\Text\files\test4.csv" };

				foreach (var file in files)
				{
					Console.WriteLine("--- DelimitedReader with advanced escaping - {0} ---", file);

					var benchmarkArgs = new DelimitedRecordReaderBenchmarkArguments();
					benchmarkArgs.Path = file;
					benchmarkArgs.TrimWhiteSpaces = true;
					benchmarkArgs.AdvancedEscapingEnabled = true;

					Benchmark.Execute("NLight", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll);
					Benchmark.Execute("DataStreams", benchmarkOptions, benchmarkArgs, OutputResults, DelimitedRecordReaderBenchmarks.ReadAll_DataStreams);
				}
			}

			#endregion

			Console.WriteLine("\nDone");
			Console.ReadLine();
		}

		#region Support members

		static void OutputResults(object sender, BenchmarkOptions benchmarkOptions, RecordReaderBenchmarkArguments benchmarkArgs, long iterationIndex, BenchmarkResult result)
		{
			var fi = new System.IO.FileInfo(benchmarkArgs.Path);
			decimal rate = ((decimal) fi.Length / 1024 / 1024) / ((decimal) result.Timer.ElapsedMilliseconds / 1000);

			Console.WriteLine("{0}: {1,25}: {2,10} ticks, {3,10} bytes, {4,4} gc0, {5,4} gc1, {6,4} gc2, {7,6:F} MB/s", iterationIndex, result.Name, result.Timer.ElapsedTicks, result.UsedMemory, result.GC0, result.GC1, result.GC2, rate);
			Trace.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7:F}", iterationIndex, result.Name, result.Timer.ElapsedTicks, result.UsedMemory, result.GC0, result.GC1, result.GC2, rate));
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject != null)
				Console.WriteLine("Unhandled exception :\n\n'{0}'.", e.ExceptionObject.ToString());
			else
				Console.WriteLine("Unhandled exception occurred.");

			Console.ReadLine();
		}

		#endregion
	}
}