// Author(s): Sébastien Lorion

using System;

namespace NLight.Core
{
	/// <summary>
	/// Contains useful functions for manipulating bits.
	/// </summary>
	public static class BinaryHelper
	{
		/// <summary>
		/// Rotates the bits to the left.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="count">The number of bits to rotate.</param>
		/// <returns>The value rotated by <paramref name="count"/> to the left.</returns>
		public static int RotateLeft(int value, int count) => ((value << (count & 0x1f)) | (count >> ((0x20 - count) & 0x1f)));

		/// <summary>
		/// Rotates the bits to the right.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="count">The number of bits to rotate.</param>
		/// <returns>The value rotated by <paramref name="count"/> to the right.</returns>
		public static int RotateRight(int value, int count) => ((value >> (count & 0x1f)) | (value << ((0x20 - count) & 0x1f)));

		/// <summary>
		/// Reverses the number of significant bits specified.
		/// </summary>
		/// <param name="value">The value to reverse.</param>
		/// <param name="significantBitCount">The number of significant bits to reverse.</param>
		/// <returns>The <paramref name="value"/> with its <paramref name="significantBitCount"/> reversed.</returns>
		/// <example>
		/// Example, if the input is 376, with significantBitCount=11, the output is 244 (decimal, base 10).
		/// 
		/// 376 = 00000101111000
		/// 244 = 00000011110100
		/// 
		/// Example, if the input is 900, with significantBitCount=11, the output is 270.
		/// 
		/// 900 = 00001110000100
		/// 270 = 00000100001110
		/// 
		/// Example, if the input is 900, with significantBitCount=12, the output is 540.
		/// 
		/// 900 = 00001110000100
		/// 540 = 00001000011100
		/// 
		/// Example, if the input is 154, with significantBitCount=4, the output is 5.
		/// 
		/// 154 = 00000010011010
		/// 005 = 00000000000101
		/// </example>
		public static long Reverse(long value, int significantBitCount)
		{
			ulong n = (ulong) value << (64 - significantBitCount);
			n = n >> 32 | n << 32;
			n = n >> 0xf & 0x0000ffff | n << 0xf & 0xffff0000;
			n = n >> 0x8 & 0x00ff00ff | n << 0x8 & 0xff00ff00;
			n = n >> 0x4 & 0x0f0f0f0f | n << 0x4 & 0xf0f0f0f0;
			n = n >> 0x2 & 0x33333333 | n << 0x2 & 0xcccccccc;
			n = n >> 0x1 & 0x55555555 | n << 0x1 & 0xaaaaaaaa;

			return (long) n | value & (-1 << significantBitCount);
		}
	}
}