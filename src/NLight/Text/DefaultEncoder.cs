// Author(s): Sébastien Lorion

using System;
using System.Text;

namespace NLight.Text
{
	internal class DefaultDecoder
		: Decoder
	{
		private readonly Encoding _encoding;

		public DefaultDecoder(Encoding encoding)
		{
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));

			_encoding = encoding;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return _encoding.GetCharCount(bytes, index, count);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
		}
	}
}