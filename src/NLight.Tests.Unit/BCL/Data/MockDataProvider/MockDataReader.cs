// Author(s): Sébastien Lorion

using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockDataReader
		: DbDataReader
	{
		private bool _isClosed;

		public MockDataReader(MockDataCommand command, CommandBehavior behavior)
			: base()
		{
			if (command == null) throw new ArgumentNullException(nameof(command));

			this.Command = command;
			this.CommandBehavior = behavior;
		}

		public MockDataCommand Command { get; }
		public CommandBehavior CommandBehavior { get; }

		public override int Depth => 0;
		public override int FieldCount => 0;
		public override bool HasRows => false;
		public override bool IsClosed => _isClosed;
		public override int RecordsAffected => 0;

		public override object this[string name] { get { throw new ArgumentOutOfRangeException(nameof(name)); } }
		public override object this[int ordinal] { get { throw new ArgumentOutOfRangeException(nameof(ordinal)); } }

		public override void Close()
		{
			if ((this.CommandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
			{
				if (this.Command.Transaction != null)
					this.Command.Transaction.Dispose();

				if (this.Command.Connection != null)
					this.Command.Connection.Close();
			}

			_isClosed = true;
		}

		public override System.Collections.IEnumerator GetEnumerator() => Enumerable.Empty<IDataRecord>().GetEnumerator();

		public override bool GetBoolean(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override byte GetByte(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override char GetChar(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override DateTime GetDateTime(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override decimal GetDecimal(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override double GetDouble(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override float GetFloat(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override Guid GetGuid(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override short GetInt16(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override int GetInt32(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override long GetInt64(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override string GetString(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override object GetValue(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override int GetValues(object[] values) => this.FieldCount;

		public override string GetDataTypeName(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override Type GetFieldType(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }

		public override string GetName(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
		public override int GetOrdinal(string name) => -1;
		public override bool IsDBNull(int ordinal) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }

		public override DataTable GetSchemaTable() => new DataTable().CreateDataReader().GetSchemaTable();

		public override bool NextResult() => false;
		public override bool Read() => false;
	}
}