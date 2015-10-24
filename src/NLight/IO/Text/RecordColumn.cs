// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	public class RecordColumn
	{
		public RecordColumn(string name)
			: this(name, typeof(string), string.Empty)
		{
		}

		public RecordColumn(string name, Type dataType)
			: this(name, dataType, null)
		{
		}

		public RecordColumn(string name, Type dataType, object defaultValue)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (dataType == null) throw new ArgumentNullException(nameof(dataType));

			if (defaultValue == null && dataType.IsValueType)
				defaultValue = Activator.CreateInstance(dataType);

			this.Name = name;
			this.DataType = dataType;
			this.DefaultValue = defaultValue;
		}

		public string Name { get; }
		public Type DataType { get; }
		public object DefaultValue { get; set; }
		public bool IsIgnored { get; set; }
	}
}