// Author(s): Sébastien Lorion

using System;

namespace NLight.Tests.Benchmarks
{
	[Flags]
	public enum BenchmarkTests
	{
		None = 0,
		FixedWidthReader = 1,
		DelimitedReader = 2,
		DelimitedReaderAdvancedEscaping = 4,
		All = 0xFFFF
	}
}