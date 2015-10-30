// Author(s): Sébastien Lorion

using NLight.Collections;
using System;
using System.Collections.Generic;

namespace NLight.IO.Text
{
	partial class TextRecordReader<TColumn>
	{
		/// <summary>
		/// Specialized list that does not check arguments and does not release references when clearing.
		/// It is safe to use as a text record holder, but not as a general purpose collection.
		/// </summary>
		private class FastStringList
			: IList<string>
		{
			private string[] _items;
			private int _size;

			public FastStringList(int capacity)
			{
				if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

				_items = new string[capacity];
			}

			private void EnsureCapacity()
			{
				if (_size == _items.Length)
					Array.Resize(ref _items, _size << 1);
			}

			#region IList<string> Members

			public int IndexOf(string item) => Array.IndexOf(_items, item);

			public void Insert(int index, string item)
			{
				EnsureCapacity();
				_items = _items.Insert(index, item, true);
			}

			public void RemoveAt(int index)
			{
				_items = _items.RemoveAt(index, true);
			}

			public string this[int index]
			{
				get
				{
					if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), index, null);

					return _items[index];
				}
				set
				{
					if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index), index, null);

					_items[index] = value;
				}
			}

			#endregion

			#region ICollection<string> Members

			public void Add(string item)
			{
				EnsureCapacity();

				_items[_size] = item;
				_size++;
			}

			public void Clear()
			{
				_size = 0;
			}

			public bool Contains(string item) => IndexOf(item) > -1;

			public void CopyTo(string[] array, int arrayIndex) => Array.Copy(_items, 0, array, arrayIndex, _size);

			public int Count => _size;

			public bool IsReadOnly => false;

			public bool Remove(string item)=> _items.TryRemove(item, true, out _items);

			#endregion

			#region IEnumerable<string> Members

			public IEnumerator<string> GetEnumerator()
			{
				for (int i = 0; i < _size; i++)
					yield return _items[i];
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

			#endregion
		}
	}
}