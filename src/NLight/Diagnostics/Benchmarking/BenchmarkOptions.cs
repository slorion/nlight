// Author(s): Sébastien Lorion

using System.Threading;

namespace NLight.Diagnostics.Benchmarking
{
	/// <summary>
	/// Represents benchmark options.
	/// </summary>
	public class BenchmarkOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BenchmarkOptions"/> class.
		/// </summary>
		public BenchmarkOptions()
		{
			this.BenchmarkIterationCount = 1;
			this.ConcurrencyLevel = 1;
			this.ActionIterationCount = 1;
			this.ActionRepeatCount = 1;
		}

		public CancellationToken CancellationToken { get; set; }

		/// <summary>
		/// Gets or sets the action iteration count.
		/// </summary>
		public long ActionIterationCount { get; set; }

		/// <summary>
		/// Gets or sets the number of times the action will be repeated inside an iteration.
		/// </summary>
		public long ActionRepeatCount { get; set; }

		/// <summary>
		/// Gets or sets the benchmark iteration count.
		/// </summary>
		public long BenchmarkIterationCount { get; set; }

		/// <summary>
		/// Gets or sets the concurrency level, i.e. the maximum number of actions to execute in parallel.
		/// </summary>
		public int ConcurrencyLevel { get; set; }
	}
}