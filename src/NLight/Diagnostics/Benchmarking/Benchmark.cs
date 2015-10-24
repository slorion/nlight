// Author(s): Sébastien Lorion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Diagnostics.Benchmarking
{
	/// <summary>
	/// Contains methods to execute a benchmark.
	/// </summary>
	public static class Benchmark
	{
		/// <summary>
		/// Executes a benchmark.
		/// </summary>
		/// <typeparam name="TBenchmarkArgs">The type of class containing the benchmark arguments.</typeparam>
		/// <param name="benchmarkName">The name of the benchmark.</param>
		/// <param name="benchmarkOptions">The benchmark options.</param>
		/// <param name="benchmarkArgs">The benchmark arguments.</param>
		/// <param name="outputCallback">
		///		The benchmark output callback. Its arguments are:
		///		<list type="">
		///			<item>sender (an instance or the type of the class that executed the benchmark)</item>
		///			<item>benchmark options</item>
		///			<item>benchmark arguments</item>
		///			<item>current iteration index</item>
		///			<item>benchmark result</item>
		///		</list>
		/// </param>
		/// <param name="action">The operation to benchmark.</param>
		public static void Execute<TBenchmarkArgs>(string benchmarkName, BenchmarkOptions benchmarkOptions, TBenchmarkArgs benchmarkArgs, Action<object, BenchmarkOptions, TBenchmarkArgs, long, BenchmarkResult> outputCallback, Action<TBenchmarkArgs> action)
		{
			if (benchmarkOptions == null) throw new ArgumentNullException(nameof(benchmarkOptions));
			if (action == null) throw new ArgumentNullException(nameof(action));

			benchmarkName = benchmarkName ?? string.Empty;

			for (long iterationIndex = 0; iterationIndex < benchmarkOptions.BenchmarkIterationCount; iterationIndex++)
			{
				GC.Collect(2, GCCollectionMode.Forced);
				GC.WaitForPendingFinalizers();
				GC.Collect(2, GCCollectionMode.Forced);

				long memory = GC.GetTotalMemory(false);

				int gc0 = GC.CollectionCount(0);
				int gc1 = GC.CollectionCount(1);
				int gc2 = GC.CollectionCount(2);

				var timer = new Stopwatch();

				if (benchmarkOptions.ConcurrencyLevel <= 1)
				{
					timer.Start();

					for (long i = 0; i < benchmarkOptions.ActionIterationCount; i++)
						Repeat(action, benchmarkArgs, benchmarkOptions.ActionRepeatCount, benchmarkOptions.CancellationToken);

					timer.Stop();
				}
				else
				{
					// total time will include overhead of multi-threading
					timer.Start();

					Parallel.For(0, benchmarkOptions.ActionIterationCount, new ParallelOptions { CancellationToken = benchmarkOptions.CancellationToken, MaxDegreeOfParallelism = benchmarkOptions.ConcurrencyLevel }, (i) =>
						Repeat(action, benchmarkArgs, benchmarkOptions.ActionRepeatCount, benchmarkOptions.CancellationToken));

					timer.Stop();
				}

				var result = new BenchmarkResult();
				result.UsedMemory = GC.GetTotalMemory(false) - memory;
				result.GC0 = GC.CollectionCount(0) - gc0;
				result.GC1 = GC.CollectionCount(1) - gc1;
				result.GC2 = GC.CollectionCount(2) - gc2;

				result.Name = benchmarkName;
				result.Timer = timer;

				if (outputCallback != null)
				{
					object target = action.Target ?? action.Method.DeclaringType;
					outputCallback(target, benchmarkOptions, benchmarkArgs, iterationIndex, result);
				}
			}
		}

		private static void Repeat<T>(Action<T> action, T args, long count, CancellationToken cancellationToken)
		{
			for (long i = 0; i < count; i++)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				action(args);
			}
		}
	}
}