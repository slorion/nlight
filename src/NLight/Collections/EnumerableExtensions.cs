// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Linq;

namespace NLight.Collections
{
	public static partial class EnumerableExtensions
	{
		/// <summary>
		/// Creates a read-only wrapper of a collection.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection.</typeparam>
		/// <param name="items">The collection to convert.</param>
		/// <returns>A read-only wrapper of <paramref name="items"/>.</returns>
		public static IReadOnlyList<T> ToIReadOnlyList<T>(this IEnumerable<T> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			return items as IReadOnlyList<T> ?? new IReadOnlyListWrapper<T>(items as IList<T> ?? items.ToList());
		}

		/// <summary>
		/// Creates a read-only wrapper of a collection.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection.</typeparam>
		/// <param name="items">The collection to convert.</param>
		/// <returns>A read-only wrapper of <paramref name="items"/>.</returns>
		public static IReadOnlyCollection<T> ToIReadOnlyCollection<T>(this IEnumerable<T> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			return items as IReadOnlyCollection<T> ?? new IReadOnlyCollectionWrapper<T>(items as ICollection<T> ?? items.ToList());
		}
	}
}