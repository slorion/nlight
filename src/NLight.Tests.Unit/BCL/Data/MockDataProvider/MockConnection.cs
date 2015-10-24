// Author(s): Sébastien Lorion

using System.Data;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockConnection
		: DbConnection
	{
		private string _database = string.Empty;
		private ConnectionState _state = ConnectionState.Closed;

		public MockConnection() : base() { }

		public override ConnectionState State => _state;
		public override string ConnectionString { get; set; }
		public override string Database => _database;
		public override string DataSource => string.Empty;
		public override string ServerVersion => string.Empty;

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => new MockTransaction(this, isolationLevel);
		protected override DbCommand CreateDbCommand() => new MockDataCommand { Connection = this };

		public override void ChangeDatabase(string databaseName) => _database = databaseName;
		public override void Close() => _state = ConnectionState.Closed;
		public override void Open() => _state = ConnectionState.Open;
	}
}