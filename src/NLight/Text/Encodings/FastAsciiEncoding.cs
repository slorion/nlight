// Author(s): Sébastien Lorion

using System;
using System.Text;

namespace NLight.Text.Encodings
{
	/// <summary>
	/// Represents a faster ASCII character encoding that does not handle code pages and fallback.
	/// </summary>
	public class FastAsciiEncoding
		: ASCIIEncoding
	{
		public FastAsciiEncoding()
			: base()
		{
		}

		public override Decoder GetDecoder()
		{
			return new DefaultDecoder(this);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			int length = byteCount - byteIndex;

			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					fixed (char* pChars = chars)
					{
						AsciiToUnicode(pBytes + byteIndex, (byte*) (pChars + charIndex), byteCount);
					}
				}
			}

			return length;
		}

		private static unsafe void AsciiToUnicode(byte* src, byte* dest, int length)
		{
			if (src == null) throw new ArgumentNullException(nameof(src));
			if (dest == null) throw new ArgumentNullException(nameof(dest));
			if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, null);

			//TODO: handle code page 1252, range 0x80-0x9F, 
			// 1000 0000 <= c => 1001 1111, ie (c >> 5 == 0x4)
			// http://www.unicode.org/charts
			// http://www.unicode.org/Public/MAPPINGS/VENDORS/MICSFT/WINDOWS/CP1252.TXT
			// http://www.i18nguy.com/unicode/codepages.html

			if (length >= 0x8)
			{
				do
				{
					*((int*) dest) = *(src + 1) << 16 | *(src);
					*((int*) (dest + 4)) = *(src + 3) << 16 | *(src + 2);
					*((int*) (dest + 8)) = *(src + 5) << 16 | *(src + 4);
					*((int*) (dest + 12)) = *(src + 7) << 16 | *(src + 6);

					dest += 0x10;
					src += 0x8;
				}
				while ((length -= 0x8) >= 0x8);
			}

			if (length > 0)
			{
				if ((length & 0x4) != 0)
				{
					*((int*) dest) = *(src + 1) << 16 | *(src);
					*((int*) (dest + 4)) = *(src + 3) << 16 | *(src + 2);

					dest += 8;
					src += 4;
				}
				if ((length & 0x2) != 0)
				{
					*((int*) dest) = *(src + 1) << 16 | *(src);

					dest += 4;
					src += 2;
				}
				if ((length & 0x1) != 0)
				{
					*((short*) dest) = *((byte*) src);

					dest += 2;
					src++;
				}
			}
		}
	}
}