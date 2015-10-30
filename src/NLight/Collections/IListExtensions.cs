// Author(s): Sébastien Lorion

using NLight.Core;
using System;
using System.Collections.Generic;

namespace NLight.Collections
{
	public static class IListExtensions
	{
		public static void Shuffle<T>(this IList<T> items, int iterationCount = 1000)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (iterationCount < 0) throw new ArgumentOutOfRangeException(nameof(iterationCount), iterationCount, null);

			for (int i = 0; i < iterationCount; i++)
			{
				int from = StrongRandom.NextInt32(0, items.Count);
				int to = StrongRandom.NextInt32(0, items.Count);

				var temp = items[from];
				items[from] = items[to];
				items[to] = temp;
			}
		}
	}
}