// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	public class DelimitedRecordColumn
		: RecordColumn
	{
		public DelimitedRecordColumn(string name) : base(name) { }
		public DelimitedRecordColumn(string name, Type dataType) : base(name, dataType) { }
		public DelimitedRecordColumn(string name, Type dataType, object defaultValue) : base(name, dataType, defaultValue) { }
	}
}