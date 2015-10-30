// Author(s): Sébastien Lorion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace NLight.Collections
{
	[Serializable]
	public class OrderedDictionary<TKey, TValue>
		: IDictionary<TKey, TValue>, IReadOnlyList<TValue>, INotifyCollectionChanged
	{
		// the use of a concurrent dictionary is not for thread-safety purpose, but to facilitate the synchronisation of the key->index mapping with the value collection
		private readonly ConcurrentDictionary<TKey, int> _dictionary;

		// a simple value list is used to maximise access by index performance and minimise memory footprint
		private readonly List<TValue> _list;

		public OrderedDictionary() : this(null, 0, EqualityComparer<TKey>.Default) { }
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(null, collection, EqualityComparer<TKey>.Default) { }
		public OrderedDictionary(IEqualityComparer<TKey> comparer) : this(null, 0, comparer) { }
		public OrderedDictionary(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default) { }
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(null, collection, comparer) { }
		public OrderedDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : this((int?) concurrencyLevel, capacity, comparer) { }
		public OrderedDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this((int?) concurrencyLevel, collection, comparer) { }

		private OrderedDictionary(int? concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
		{
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

			_dictionary = concurrencyLevel == null ? new ConcurrentDictionary<TKey, int>(comparer) : new ConcurrentDictionary<TKey, int>(concurrencyLevel.Value, capacity, comparer);
			_list = new List<TValue>(capacity);
		}

		private OrderedDictionary(int? concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

			int? count = (collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? (collection as IReadOnlyCollection<KeyValuePair<TKey, TValue>>)?.Count;

			_dictionary = concurrencyLevel == null ? new ConcurrentDictionary<TKey, int>(comparer) : new ConcurrentDictionary<TKey, int>(concurrencyLevel.Value, count ?? 31, comparer);
			_list = new List<TValue>(count ?? 0);

			foreach (var kv in collection)
			{
				if (!_dictionary.TryAdd(kv.Key, _list.Count))
					throw new ArgumentException(Resources.ExceptionMessages.Collections_SourceCannotContainDuplicateKey, nameof(collection));
				else
					_list.Add(kv.Value);
			}
		}

		public int IndexOf(TKey key)
		{
			int index;
			if (!_dictionary.TryGetValue(key, out index))
				return -1;
			else
				return index;
		}

		#region INotifyCollectionChanged members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region IReadOnlyList<TValue> members

		public TValue this[int index] => _list[index];
		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => _list.GetEnumerator();

		#endregion

		#region IDictionary<TKey, TValue> members

		public TValue this[TKey key]
		{
			get { return _list[_dictionary[key]]; }
			set
			{
				var action = NotifyCollectionChangedAction.Add;

				_dictionary.AddOrUpdate(
					key,
					k =>
					{
						action = NotifyCollectionChangedAction.Add;
						_list.Add(value);
						return _list.Count - 1;
					},
					(k, index) =>
					{
						action = NotifyCollectionChangedAction.Replace;
						_list[index] = value;
						return index;
					});

				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, new KeyValuePair<TKey, TValue>(key, value)));
			}
		}

		public int Count => _list.Count;
		public bool IsReadOnly => false;
		public ICollection<TKey> Keys => _dictionary.Keys;
		public ICollection<TValue> Values => _list;

		public void Add(KeyValuePair<TKey, TValue> item) => this.Add(item.Key, item.Value);

		public void Add(TKey key, TValue value)
		{
			int index = _list.Count;

			if (_dictionary.TryAdd(key, index))
			{
				_list.Add(value);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
			}
		}

		public void Clear()
		{
			_list.Clear();
			_dictionary.Clear();

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			int index;
			return _dictionary.TryGetValue(item.Key, out index) && object.Equals(_list[index], item.Value);
		}

		public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, null);

			foreach (var keyValue in this)
			{
				array[arrayIndex] = keyValue;
				arrayIndex++;
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			// if we assume that most of the time, the requested key/value pair has an exact match,
			// then it is better to go ahead and remove the key from the dictionary preemptively

			int index;
			if (!_dictionary.TryRemove(item.Key, out index))
				return false;
			else if (object.Equals(_list[index], item.Value))
			{
				_list.RemoveAt(index);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
				return true;
			}
			else
			{
				_dictionary[item.Key] = index;
				return false;
			}
		}

		public bool Remove(TKey key)
		{
			int index;
			if (!_dictionary.TryRemove(key, out index))
				return false;
			else
			{
				TValue value = _list[index];
				_list.RemoveAt(index);

				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));

				return true;
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int index;
			if (!_dictionary.TryGetValue(key, out index))
			{
				value = default(TValue);
				return false;
			}
			else
			{
				value = _list[index];
				return true;
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.Select(keyIndex => new KeyValuePair<TKey, TValue>(keyIndex.Key, _list[keyIndex.Value])).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		#endregion
	}
}