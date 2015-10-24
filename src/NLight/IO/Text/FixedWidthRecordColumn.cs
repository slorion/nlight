// Author(s): Sébastien Lorion

using System;

namespace NLight.IO.Text
{
	public class FixedWidthRecordColumn
		: RecordColumn
	{
		private int _minimumWidth;

		public FixedWidthRecordColumn(string name, int startingPosition, int width)
			: base(name)
		{
			Initialize(startingPosition, width);
		}

		public FixedWidthRecordColumn(string name, Type dataType, int startingPosition, int width)
			: base(name, dataType)
		{
			Initialize(startingPosition, width);
		}

		public FixedWidthRecordColumn(string name, Type dataType, object defaultValue, int startingPosition, int width)
			: base(name, dataType, defaultValue)
		{
			Initialize(startingPosition, width);
		}

		public int StartingPosition { get; private set; }

		public int Width { get; private set; }

		public char PaddingCharacter { get; set; }

		public RecordColumnAlignment ValueAlignment { get; set; }

		public bool TrimPadding { get; set; }

		public int MinimumWidth
		{
			get { return _minimumWidth; }
			set
			{
				if (value < 0 || value > this.Width) throw new ArgumentOutOfRangeException(nameof(value));

				_minimumWidth = value;
			}
		}

		private void Initialize(int startingPosition, int width)
		{
			if (startingPosition < 0) throw new ArgumentOutOfRangeException(nameof(startingPosition));
			if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));

			this.StartingPosition = startingPosition;
			this.Width = width;
			this.PaddingCharacter = ' ';
			this.TrimPadding = true;
		}
	}
}