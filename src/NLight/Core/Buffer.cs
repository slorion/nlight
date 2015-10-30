// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Linq;

namespace NLight.Core
{
	/// <summary>
	/// Represents a data buffer.
	/// </summary>
	/// <typeparam name="T">The type of the data contained in the buffer.</typeparam>
	public class Buffer<T>
	{
		private readonly T[] _rawData;
		private readonly Func<T[], int, int> _fillCallback;

		/// <summary>
		/// Initializes a new instance of the <see cref="Buffer{T}"/> class.
		/// </summary>
		/// <param name="capacity">The buffer capacity.</param>
		/// <param name="fillCallback">
		///		The function that will be used to fill the buffer.
		///		The function takes the buffer array and the offset where filling will begin and returns the number of values that were obtained.
		/// </param>
		/// <param name="bookmarkComparer">The <see cref="IEqualityComparer{T}"/> that will be used to search bookmarks.</param>
		public Buffer(int capacity, Func<T[], int, int> fillCallback = null, IEqualityComparer<string> bookmarkComparer = null)
		{
			if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));

			_rawData = new T[capacity];
			_fillCallback = fillCallback;
			this.Bookmarks = new Dictionary<string, int>(bookmarkComparer);
		}

		/// <summary>
		/// Gets the value at the specified index.
		/// </summary>
		public T this[int index] => _rawData[index];

		/// <summary>
		/// Gets the value at the current position.
		/// </summary>
		public T Current => _rawData[this.Position];

		/// <summary>
		/// Gets the raw buffer data.
		/// </summary>
		public T[] RawData => _rawData;

		/// <summary>
		/// Gets the buffer capacity.
		/// </summary>
		public int Capacity => _rawData.Length;

		/// <summary>
		/// Gets the current length of the buffer data.
		/// </summary>
		public int Length { get; private set; }

		/// <summary>
		/// Gets or sets the current position inside the buffer.
		/// </summary>
		public int Position { get; set; }

		/// <summary>
		/// Gets a table of position bookmarks inside the buffer.
		/// </summary>
		public IDictionary<string, int> Bookmarks { get; }

		/// <summary>
		/// Ensures the buffer has data.
		/// </summary>
		/// <returns><c>true</c> if the buffer has data; otherwise, <c>false</c>.</returns>
		public bool EnsureHasData() => this.Position < this.Length || Fill(0);

		/// <summary>
		/// Clears the buffer by setting its length to 0, and also clears the raw data array.
		/// </summary>
		public void Clear(bool purgeData = false)
		{
			this.Position = 0;
			this.Length = 0;

			if (purgeData)
				Array.Clear(_rawData, 0, _rawData.Length);
		}

		/// <summary>
		/// Fills the buffer with new data, but keeps <paramref name="keepCount"/> values.
		/// </summary>
		/// <param name="keepCount">The number of values to keep.</param>
		/// <returns><c>true</c> if the buffer has data; otherwise, <c>false</c>.</returns>
		public bool Fill(int keepCount = 0)
		{
			if (keepCount < 0) throw new ArgumentOutOfRangeException(nameof(keepCount));

			if (_fillCallback == null)
				return false;
			else
			{
				if (keepCount == 0)
					this.Position = 0;
				else
				{
					if (keepCount >= _rawData.Length)
						throw new ArgumentException(Resources.ExceptionMessages.Core_CannotKeepAllBufferElements, "keepCount");

					Array.Copy(_rawData, this.Length - keepCount, _rawData, 0, keepCount);
					this.Position = keepCount - (this.Length - this.Position);

					foreach (var key in this.Bookmarks.Keys.ToArray())
						this.Bookmarks[key] = keepCount - (this.Length - this.Bookmarks[key]);
				}

				int readLength = _fillCallback(_rawData, keepCount);
				this.Length = readLength + keepCount;

				return readLength > 0;
			}
		}
	}
}