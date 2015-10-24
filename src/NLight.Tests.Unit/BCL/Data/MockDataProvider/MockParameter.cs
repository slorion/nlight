// Author(s): Sébastien Lorion

using System.Data;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockParameter
		: DbParameter
	{
		public MockParameter() : base() { }

		public override DbType DbType { get; set; }
		public override ParameterDirection Direction { get; set; }
		public override bool IsNullable { get; set; }
		public override string ParameterName { get; set; }
		public override int Size { get; set; }
		public override string SourceColumn { get; set; }
		public override bool SourceColumnNullMapping { get; set; }
		public override DataRowVersion SourceVersion { get; set; }
		public override object Value { get; set; }

		public override void ResetDbType() => this.DbType = DbType.String;
	}
}