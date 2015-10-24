// Author(s): Sébastien Lorion

using System.Diagnostics;

namespace NLight.Diagnostics.Benchmarking
{
	/// <summary>
	/// Represents a benchmark result.
	/// </summary>
	public class BenchmarkResult
	{
		/// <summary>
		/// Gets the benchmark name.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the used memory.
		/// </summary>
		public long UsedMemory { get; internal set; }

		/// <summary>
		/// Gets the number of first generation garbage collections that occurred.
		/// </summary>
		public int GC0 { get; internal set; }

		/// <summary>
		/// Gets the number of second generation garbage collections that occurred.
		/// </summary>
		public int GC1 { get; internal set; }

		/// <summary>
		/// Gets the number of third generation garbage collections that occurred.
		/// </summary>
		public int GC2 { get; internal set; }

		/// <summary>
		/// Gets the timer used for the benchmark.
		/// </summary>
		public Stopwatch Timer { get; internal set; }
	}
}