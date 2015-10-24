// Author(s): Sébastien Lorion

using System.Runtime.CompilerServices;

namespace NLight.Text.Parsing
{
	/// <summary>
	/// Contains methods to parse numbers.
	/// </summary>
	public static class NumberParser
	{
		/// <summary>
		/// Converts a char to its corresponding hexadecimal digit.
		/// </summary>
		/// <param name="value">The char to convert.</param>
		/// <returns>The corresponding hexadecimal digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ConvertHexDigit(char value)
		{
			if ((value <= '9') && (value >= '0'))
				return (value - '0');

			if ((value >= 'a') && (value <= 'f'))
				return ((value - 'a') + 10);

			if ((value >= 'A') && (value <= 'F'))
				return ((value - 'A') + 10);

			return -1;
		}

		/// <summary>
		/// Converts a char to its corresponding decimal digit.
		/// </summary>
		/// <param name="value">The char to convert.</param>
		/// <returns>The corresponding decimal digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ConvertDecimalDigit(char value)
		{
			if ((value <= '9') && (value >= '0'))
				return (value - '0');

			return -1;
		}

		/// <summary>
		/// Converts a char to its corresponding octal digit.
		/// </summary>
		/// <param name="value">The char to convert.</param>
		/// <returns>The corresponding octal digit.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ConvertOctalDigit(char value)
		{
			if ((value <= '7') && (value >= '0'))
				return (value - '0');

			return -1;
		}
	}
}