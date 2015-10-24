// Author(s): Sébastien Lorion

namespace NLight.Tests.Unit.IO.Text.DelimitedRecordReaderTests
{
	partial class ClassTests
	{
		private struct Triple<TFirst, TSecond, TThird>
		{
			public Triple(TFirst first, TSecond second, TThird third)
			{
				First = first;
				Second = second;
				Third = third;
			}

			public TFirst First;
			public TSecond Second;
			public TThird Third;
		}
	}
}