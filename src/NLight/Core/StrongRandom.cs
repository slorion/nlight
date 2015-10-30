// Author(s): Sébastien Lorion

using System;
using System.Security.Cryptography;

namespace NLight.Core
{
	// could be made faster by buffering calls to GetBytes, but thread-safety issues would need to be dealt with
	public static class StrongRandom
	{
		private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

		public static int NextInt32(int max) => NextInt32(0, max);

		public static int NextInt32(int min, int max)
		{
			if (max < min) throw new ArgumentOutOfRangeException(nameof(max), max, null);

			int value = BitConverter.ToInt32(NextBytes(sizeof(int)), 0) & 0x7FFF;
			int range = max - min;

			return (value % range) + min;
		}

		public static long NextInt64(long max) => NextInt64(0, max);

		public static long NextInt64(long min, long max)
		{
			if (max < min) throw new ArgumentOutOfRangeException(nameof(max), max, null);

			long value = BitConverter.ToInt64(NextBytes(sizeof(long)), 0) & 0x7FFFFFFF;
			long range = max - min;

			return (value % range) + min;
		}

		public static byte[] NextBytes(int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, null);

			var bytes = new byte[count];
			_rng.GetBytes(bytes);

			return bytes;
		}

		public static void NextBytes(byte[] buffer, int offset, int count)
		{
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			if (count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count), count, null);

			_rng.GetBytes(buffer, offset, count);
		}
	}
}