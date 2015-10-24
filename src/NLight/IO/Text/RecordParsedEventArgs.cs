// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;

namespace NLight.IO.Text
{
	public class RecordParsedEventArgs
		: EventArgs
	{
		public RecordParsedEventArgs(ReadResult readResult, IList<string> record)
			: base()
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			this.ReadResult = readResult;
			this.Record = record;
		}

		public ReadResult ReadResult { get; }
		public IList<string> Record { get; }
	}
}
