// Author(s): Sébastien Lorion

using System;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockDataAdapter
		: DbDataAdapter
	{
		public EventHandler<RowUpdatingEventArgs> RowUpdating;
		public EventHandler<RowUpdatedEventArgs> RowUpdated;
	}
}