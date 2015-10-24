// Author(s): Sébastien Lorion

using NLight.Core;
using System;
using System.Collections.Generic;

namespace NLight.Text.Parsing
{
	/// <summary>
	/// Contains methods to parse an escaped string.
	/// </summary>
	public static class EscapedStringParser
	{
		private const int MaximumEscapedValueLength = 8; // \oxxxxxx

		/// <summary>
		/// Defines the escape character.
		/// </summary>
		public const char EscapeCharacter = '\\';

		/// <summary>
		/// Tries to parse an escaped character.
		/// </summary>
		/// <param name="buffer">The data to parse.</param>
		/// <param name="value">The parsed value if successful.</param>
		/// <returns><c>true</c> if an escaped character was successfully parsed; otherwise, <c>false</c>.</returns>
		public static bool TryParseEscapedChar(Buffer<char> buffer, out char value)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			int pos = buffer.Position;

			if (buffer.Length - pos < MaximumEscapedValueLength)
			{
				buffer.Fill(buffer.Length - pos);
				pos = buffer.Position;
			}

			if (TryParseEscapedChar(buffer.RawData, ref pos, buffer.Length - 1, out value))
			{
				buffer.Position = pos;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Tries to parse an escaped character.
		/// </summary>
		/// <param name="data">The data to parse.</param>
		/// <param name="startIndex">The starting position where the parsing will occur.</param>
		/// <param name="endIndex">The end position where the parsing must stop, whether successful or not.</param>
		/// <param name="value">The parsed value if successful.</param>
		/// <returns><c>true</c> if an escaped character was successfully parsed; otherwise, <c>false</c>.</returns>
		public static bool TryParseEscapedChar(string data, ref int startIndex, int endIndex, out char value)
		{
			return TryParseEscapedChar(data, ref startIndex, endIndex, out value);
		}

		/// <summary>
		/// Tries to parse an escaped character.
		/// </summary>
		/// <param name="data">The data to parse.</param>
		/// <param name="startIndex">The starting position where the parsing will occur.</param>
		/// <param name="endIndex">The end position where the parsing must stop, whether successful or not.</param>
		/// <param name="value">The parsed value if successful.</param>
		/// <returns><c>true</c> if an escaped character was successfully parsed; otherwise, <c>false</c>.</returns>
		public static bool TryParseEscapedChar(char[] data, ref int startIndex, int endIndex, out char value)
		{
			return TryParseEscapedChar((IReadOnlyList<char>) data, ref startIndex, endIndex, out value);
		}

		/// <summary>
		/// Tries to parse an escaped character.
		/// </summary>
		/// <param name="data">The data to parse.</param>
		/// <param name="startIndex">The starting position where the parsing will occur.</param>
		/// <param name="endIndex">The end position where the parsing must stop, whether successful or not.</param>
		/// <param name="value">The parsed value if successful.</param>
		/// <returns><c>true</c> if an escaped character was successfully parsed; otherwise, <c>false</c>.</returns>
		private static bool TryParseEscapedChar(IReadOnlyList<char> data, ref int startIndex, int endIndex, out char value)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));
			if (startIndex < 0 || startIndex >= data.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (endIndex < 0 || endIndex >= data.Count) throw new ArgumentOutOfRangeException(nameof(endIndex));

			if (endIndex - startIndex < 2 || data[startIndex] != EscapeCharacter)
			{
				value = '\0';
				return false;
			}

			int pos = startIndex + 1;

			switch (data[pos])
			{
				// new line
				case 'n':
					startIndex = pos + 1;
					value = '\n';
					return true;

				// tab
				case 't':
					startIndex = pos + 1;
					value = '\t';
					return true;

				// literal escape char
				case '\\':
					startIndex = pos + 1;
					value = '\\';
					return true;

				// Unicode char (hexadecimal)
				case 'u':
				case 'x':
					{
						startIndex = pos + 1;

						int charValue = 0;
						int end = Math.Min(startIndex + 4, endIndex);

						while (startIndex <= end)
						{
							int digit = NumberParser.ConvertHexDigit(data[startIndex]);

							if (digit < 0)
								break;

							charValue <<= 4;
							charValue += digit;
							startIndex++;
						}

						value = (char) charValue;
						return true;
					}

				// carriage return
				case 'r':
					startIndex = pos + 1;
					value = '\r';
					return true;

				// form feed
				case 'f':
					startIndex = pos + 1;
					value = '\f';
					return true;

				// vertical quote
				case 'v':
					startIndex = pos + 1;
					value = '\v';
					return true;

				// alert
				case 'a':
					startIndex = pos + 1;
					value = '\a';
					return true;

				// backspace
				case 'b':
					startIndex = pos + 1;
					value = '\b';
					return true;

				// escape
				case 'e':
					startIndex = pos + 1;
					value = '\u001b';
					return true;

				// Unicode char (decimal)
				case 'd':
					{
						startIndex = pos + 1;

						int charValue = 0;
						int end = Math.Min(startIndex + 5, endIndex);

						while (startIndex <= end)
						{
							int digit = NumberParser.ConvertHexDigit(data[startIndex]);

							if (digit < 0)
								break;

							charValue *= 10;
							charValue += digit;
							startIndex++;
						}

						value = (char) charValue;
						return true;
					}

				// Unicode char (octal)
				case 'o':
					{
						startIndex = pos + 1;

						int charValue = 0;
						int end = Math.Min(startIndex + 6, endIndex);

						while (startIndex <= end)
						{
							int digit = NumberParser.ConvertHexDigit(data[startIndex]);

							if (digit < 0)
								break;

							charValue <<= 3;
							charValue += digit;
							startIndex++;
						}

						value = (char) charValue;
						return true;
					}

				default:
					value = '\0';
					return false;
			}
		}
	}
}