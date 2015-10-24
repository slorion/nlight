// Author(s): Sébastien Lorion

using System;
using System.Data;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockTransaction
		: DbTransaction
	{
		private readonly MockConnection _connection;
		private readonly IsolationLevel _isolationLevel;

		public MockTransaction(MockConnection connection, IsolationLevel isolationLevel)
			: base()
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
			_isolationLevel = isolationLevel;
		}

		protected override DbConnection DbConnection => _connection;
		public override IsolationLevel IsolationLevel => _isolationLevel;

		public override void Commit() { }
		public override void Rollback() { }
	}
}