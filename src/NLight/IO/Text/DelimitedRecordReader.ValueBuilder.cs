// Author(s): Sébastien Lorion

using System;
using System.Diagnostics;

namespace NLight.IO.Text
{
	partial class DelimitedRecordReader
	{
		private class ValueBuilder
		{
			private string _value;

			private char[] _buffer = new char[128];
			private int _position;

			private void CopyValueToBuffer(int additionalLength)
			{
				if (additionalLength < 0) throw new ArgumentOutOfRangeException(nameof(additionalLength));

				Debug.Assert(_value != null);

				int finalLength = _value.Length + additionalLength;

				if (finalLength >= _buffer.Length)
					Array.Resize(ref _buffer, Math.Max(finalLength, _buffer.Length * 2));

				for (int i = 0; i < _value.Length; i++)
					_buffer[i] = _value[i];

				_position = _value.Length;
				_value = null;
			}

			public void Append(char c)
			{
				if (_value != null)
					CopyValueToBuffer(1);

				if (_position == 0)
					_value = c.ToString();
				else
				{
					if (_position >= _buffer.Length)
						Array.Resize(ref _buffer, _buffer.Length * 2);

					_buffer[_position] = c;
					_position++;
				}
			}

			public void Append(char[] buffer, int start, int length)
			{
				if (buffer == null) throw new ArgumentNullException(nameof(buffer));
				if (start < 0 || start + length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(start));
				if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

				if (length == 0)
					return;

				if (_value != null)
					CopyValueToBuffer(length);

				if (_position == 0)
					_value = new string(buffer, start, length);
				else
				{
					if (_position + length >= _buffer.Length)
						Array.Resize(ref _buffer, Math.Max(_position + length, _buffer.Length * 2));

					Array.Copy(buffer, start, _buffer, _position, length);
					_position += length;
				}
			}

			public void Clear()
			{
				_value = null;
				_position = 0;
			}

			public void TrimEnd(Func<char, bool> isWhitespaceCallback)
			{
				if (isWhitespaceCallback == null) throw new ArgumentNullException(nameof(isWhitespaceCallback));

				if (_value != null)
				{
					int pos = _value.Length - 1;

					while (pos >= 0 && isWhitespaceCallback(_value[pos]))
						pos--;

					if (pos < _value.Length - 1)
						_value = pos > -1 ? _value.Substring(0, pos + 1) : string.Empty;
				}
				else
				{
					while (_position > 0 && isWhitespaceCallback(_buffer[_position - 1]))
						_position--;
				}
			}

			#region Overrides

			public override string ToString()
			{
				if (_value != null)
					return _value;
				else
					return new string(_buffer, 0, _position);
			}

			#endregion
		}
	}
}