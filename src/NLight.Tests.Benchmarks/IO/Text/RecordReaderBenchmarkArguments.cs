// Author(s): Sébastien Lorion

using NLight.IO.Text;
using System.Text;

namespace NLight.Tests.Benchmarks.IO.Text
{
	public abstract class RecordReaderBenchmarkArguments
	{
		public RecordReaderBenchmarkArguments()
			: base()
		{
			this.BufferSize = TextRecordReader.DefaultBufferSize;
			this.FieldIndex = -1;
			this.Encoding = Encoding.UTF8;
		}

		public string Path { get; set; }
		public int BufferSize { get; set; }
		public int FieldIndex { get; set; }
		public bool SkipEmptyLines { get; set; }
		public Encoding Encoding { get; set; }
	}
}