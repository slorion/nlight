// Author(s): Sébastien Lorion

using System;
using System.Data;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockDataCommand
		: DbCommand
	{
		public MockDataCommand() : base() { }

		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		protected override DbConnection DbConnection { get; set; }
		protected override DbParameterCollection DbParameterCollection => new MockDataParameterCollection();
		protected override DbTransaction DbTransaction { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }

		public override void Cancel() { }
		protected override DbParameter CreateDbParameter() => new MockParameter();
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new MockDataReader(this, behavior);
		public override int ExecuteNonQuery() => -1;
		public override object ExecuteScalar() => DBNull.Value;
		public override void Prepare() { }
	}
}