// Author(s): Sébastien Lorion

using NLight.Collections;
using System;

namespace NLight.IO.Text
{
	public class RecordColumnCollection<T>
		: OrderedDictionary<string, T>
		where T : RecordColumn
	{
		public RecordColumnCollection()
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
		}

		public void Add(T column)
		{
			if (column == null) throw new ArgumentNullException(nameof(column));

			this.Add(column.Name, column);
		}

		//protected override void SetItem(int index, T item)
		//{
		//	// check if the new column name conflicts with an existing column besides the one being removed
		//	T removedColumn = this[index];
		//	T newColumn;
		//	if (this.Dictionary.TryGetValue(this.GetKeyForItem(item), out newColumn) && newColumn != removedColumn)
		//		throw new DuplicateRecordColumnException(this.GetKeyForItem(item));

		//	base.SetItem(index, item);
		//	OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, item));
		//}
	}
}