// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;

namespace NLight.Collections
{
	partial class EnumerableExtensions
	{
		private class IReadOnlyListWrapper<T>
			: IReadOnlyList<T>
		{
			private readonly IList<T> _inner;

			public IReadOnlyListWrapper(IList<T> list)
			{
				if (list == null) throw new ArgumentNullException(nameof(list));
				_inner = list;
			}

			#region IReadOnlyList<T> Members

			public T this[int index]
			{
				get { return _inner[index]; }
			}

			#endregion

			#region IReadOnlyCollection<T> Members

			public int Count
			{
				get { return _inner.Count; }
			}

			#endregion

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator()
			{
				return _inner.GetEnumerator();
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}
	}
}