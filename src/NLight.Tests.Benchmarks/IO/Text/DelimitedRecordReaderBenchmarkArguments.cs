// Author(s): Sébastien Lorion

namespace NLight.Tests.Benchmarks.IO.Text
{
	public class DelimitedRecordReaderBenchmarkArguments
		: RecordReaderBenchmarkArguments
	{
		public DelimitedRecordReaderBenchmarkArguments() : base() { }

		public bool AdvancedEscapingEnabled { get; set; }
		public bool DoubleQuoteEscapingEnabled { get; set; }
		public bool TrimWhiteSpaces { get; set; }
	}
}