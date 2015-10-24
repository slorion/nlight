// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;

namespace NLight.Collections
{
	partial class EnumerableExtensions
	{
		private class IReadOnlyCollectionWrapper<T>
			: IReadOnlyCollection<T>
		{
			private readonly ICollection<T> _inner;

			public IReadOnlyCollectionWrapper(ICollection<T> collection)
			{
				if (collection == null) throw new ArgumentNullException(nameof(collection));
				_inner = collection;
			}

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