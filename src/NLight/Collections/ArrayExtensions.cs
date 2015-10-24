// Author(s): Sébastien Lorion

using System;

namespace NLight.Collections
{
	/// <summary>
	/// Contains extension methods for arrays.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		/// Inserts a value in the specified source array.
		/// </summary>
		/// <typeparam name="T">The type of array elements.</typeparam>
		/// <param name="source">The source array.</param>
		/// <param name="index">The index where the value will be inserted.</param>
		/// <param name="value">The value to insert.</param>
		/// <param name="isVirtualArray">Indicates if the array is virtual, i.e. its length can be bigger than the element count.</param>
		/// <returns>The resulting new array.</returns>
		public static T[] Insert<T>(this T[] source, int index, T value, bool isVirtualArray = false)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (index < 0 || index > source.Length) throw new ArgumentOutOfRangeException(nameof(index));

			int count = source.Length;

			if (isVirtualArray)
			{
				while (count > 1 && source[count - 1] == null)
					count--;

				if (count < 0)
					count = source.Length;
			}

			T[] destination;

			if (count < source.Length)
				destination = source;
			else
			{
				destination = new T[isVirtualArray ? source.Length * 2 : source.Length + 1];
				Array.Copy(source, destination, index);
			}

			Array.Copy(source, index, destination, index + 1, count - index);
			destination[index] = value;

			return destination;
		}

		/// <summary>
		/// Removes a value from the specified source array.
		/// </summary>
		/// <typeparam name="T">The type of array elements.</typeparam>
		/// <param name="source">The source array.</param>
		/// <param name="value">The value to remove.</param>
		/// <param name="isVirtualArray">Indicates if the array is virtual, i.e. its length can be bigger than the element count.</param>
		/// <returns>The resulting new array.</returns>
		public static T[] Remove<T>(this T[] source, T value, bool isVirtualArray = false)
		{
			TryRemove(source, value, isVirtualArray, out source);
			return source;
		}

		/// <summary>
		/// Removes a value from the specified source array.
		/// </summary>
		/// <typeparam name="T">The type of array elements.</typeparam>
		/// <param name="source">The source array.</param>
		/// <param name="value">The value to remove.</param>
		/// <param name="isVirtualArray">Indicates if the array is virtual, i.e. its length can be bigger than the element count.</param>
		/// <param name="output">The resulting new array if <paramref name="value"/> was found; otherwise <paramref name="source"/>.</param>
		/// <returns><c>true</c> if the value was found; otherwise <c>false</c>.</returns>
		public static bool TryRemove<T>(this T[] source, T value, bool isVirtualArray, out T[] output)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			int index = Array.IndexOf(source, value);

			if (index < 0)
			{
				output = source;
				return false;
			}
			else
			{
				output = RemoveAt(source, index, isVirtualArray);
				return true;
			}
		}

		/// <summary>
		/// Removes a value at the specified index in the source array.
		/// </summary>
		/// <typeparam name="T">The type of array elements.</typeparam>
		/// <param name="source">The source array.</param>
		/// <param name="index">The index where a value will be removed.</param>
		/// <param name="isVirtualArray">Indicates if the array is virtual, i.e. its length can be bigger than the element count.</param>
		/// <returns>The resulting new array.</returns>
		public static T[] RemoveAt<T>(this T[] source, int index, bool isVirtualArray = false)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (index < 0 || index >= source.Length) throw new ArgumentOutOfRangeException(nameof(index));

			T[] destination;

			if (isVirtualArray)
				destination = source;
			else
			{
				destination = new T[source.Length - 1];
				Array.Copy(source, destination, index);
			}

			if (index < source.Length - 1)
				Array.Copy(source, index + 1, destination, index, source.Length - index);

			if (isVirtualArray)
				destination[source.Length - 1] = default(T);

			return destination;
		}
	}
}