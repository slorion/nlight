// Author(s): Sébastien Lorion

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace NLight.Tests.Unit.BCL.Data
{
	//TODO: need to test invalid arguments and do more thorough tests
	public abstract class IDataReaderTests
	{
		protected abstract IDataReader CreateDataReaderInstance();
		protected abstract DataTable GetExpectedSchema();
		protected abstract long GetExpectedRecordCount();
		protected abstract IList<object> GetExpectedFieldValues(long recordIndex);

		#region IDataReader tests

		[Test]
		public void DepthTest()
		{
			Assert.Inconclusive("Not implemented.");
		}

		[Test]
		public void IsClosedTest()
		{
			using (IDataReader reader = CreateDataReaderInstance())
			{
				Assert.IsFalse(reader.IsClosed);
				reader.Close();
				Assert.IsTrue(reader.IsClosed);
			}
		}

		[Test]
		public void RecordsAffectedTest()
		{
			Assert.Inconclusive("Not implemented.");
		}

		[Test]
		public void CloseTest()
		{
			using (IDataReader reader = CreateDataReaderInstance())
			{
				Assert.IsFalse(reader.IsClosed);
				reader.Close();
				Assert.IsTrue(reader.IsClosed);

				// can call Close() multiple times
				reader.Close();
				Assert.IsTrue(reader.IsClosed);

				Assert.Throws<ObjectDisposedException>(() => { var result = reader.Depth; });
				Assert.Throws<ObjectDisposedException>(() => { var result = reader.FieldCount; });
				Assert.Throws<ObjectDisposedException>(() => { var result = reader.GetSchemaTable(); });
				Assert.Throws<ObjectDisposedException>(() => { var result = reader.NextResult(); });
				Assert.Throws<ObjectDisposedException>(() => { var result = reader.Read(); });
			}
		}

		[Test]
		public void GetSchemaTableTest()
		{
			DataTable expected = GetExpectedSchema();

			DataTable actual;
			using (IDataReader reader = CreateDataReaderInstance())
			{
				actual = reader.GetSchemaTable();
			}

			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.AllowDBNull));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.BaseColumnName));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.BaseSchemaName));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.BaseTableName));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.ColumnName));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.ColumnOrdinal));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.ColumnSize));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.DataType));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.IsAliased));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.IsExpression));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.IsKey));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.IsLong));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.IsUnique));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.NonVersionedProviderType));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.NumericPrecision));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.NumericScale));
			Assert.IsTrue(actual.Columns.Contains(SchemaTableColumn.ProviderType));

			Assert.LessOrEqual(expected.Columns.Count, actual.Columns.Count);

			foreach (DataColumn expectedColumn in expected.Columns)
			{
				DataColumn actualColumn = actual.Columns[expectedColumn.ColumnName];

				Assert.AreEqual(expectedColumn.AllowDBNull, actualColumn.AllowDBNull, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.AutoIncrement, actualColumn.AutoIncrement, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.AutoIncrementSeed, actualColumn.AutoIncrementSeed, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.AutoIncrementStep, actualColumn.AutoIncrementStep, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.Caption, actualColumn.Caption, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.ColumnMapping, actualColumn.ColumnMapping, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.ColumnName, actualColumn.ColumnName, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.DataType, actualColumn.DataType, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.DateTimeMode, actualColumn.DateTimeMode, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.DefaultValue, actualColumn.DefaultValue, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.DesignMode, actualColumn.DesignMode, expectedColumn.ColumnName);

				Assert.AreEqual(expectedColumn.Expression, actualColumn.Expression, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.MaxLength, actualColumn.MaxLength, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.Namespace, actualColumn.Namespace, expectedColumn.ColumnName);
				// column.Ordinal: column order does not matter
				Assert.AreEqual(expectedColumn.Prefix, actualColumn.Prefix, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.ReadOnly, actualColumn.ReadOnly, expectedColumn.ColumnName);
				Assert.AreEqual(expectedColumn.Unique, actualColumn.Unique, expectedColumn.ColumnName);
			}

			for (int i = 0; i < expected.Rows.Count; i++)
			{
				foreach (DataColumn column in expected.Columns)
					Assert.AreEqual(expected.Rows[i][column], actual.Rows[i][actual.Columns[column.ColumnName]]);
			}
		}

		[Test]
		public void NextResultTest()
		{
			Assert.Inconclusive("Not implemented.");
		}

		[Test]
		public void ReadTest()
		{
			long actualCount = 0;

			using (IDataReader reader = CreateDataReaderInstance())
			{
				while (reader.Read())
				{
					actualCount++;
				}
			}

			Assert.AreEqual(GetExpectedRecordCount(), actualCount);
		}

		[Test]
		public void DisposeTest()
		{
			IDataReader reader;
			using (reader = CreateDataReaderInstance())
			{
				Assert.IsFalse(reader.IsClosed);
			}

			Assert.IsTrue(reader.IsClosed);

			// can call Dispose() multiple times
			reader.Dispose();
			Assert.IsTrue(reader.IsClosed);

			Assert.Throws<ObjectDisposedException>(() => { var result = reader.Depth; });
			Assert.Throws<ObjectDisposedException>(() => { var result = reader.FieldCount; });
			Assert.Throws<ObjectDisposedException>(() => { var result = reader.GetSchemaTable(); });
			Assert.Throws<ObjectDisposedException>(() => { var result = reader.NextResult(); });
			Assert.Throws<ObjectDisposedException>(() => { var result = reader.Read(); });
		}

		#endregion

		#region IDataRecord tests

		public void GetDataTest()
		{
			Assert.Inconclusive("Not implemented.");
		}

		[Test]
		public void FieldCountTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				Assert.AreEqual(expectedSchema.Rows.Count, reader.FieldCount);

				while (reader.Read())
				{
					Assert.AreEqual(expectedSchema.Rows.Count, reader.FieldCount);
				}
			}
		}

		[Test]
		public void IndexerByIndexTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if (expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType], reader[columnIndex].GetType());

						Assert.AreEqual(expectedValues[columnIndex], reader[columnIndex]);
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void IndexerByNameTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						var columnName = (string) expectedSchema.Rows[columnIndex][SchemaTableColumn.ColumnName];

						if (expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType], reader[columnName].GetType());

						Assert.AreEqual(expectedValues[columnIndex], reader[columnName]);
					}

					recordIndex++;
				}
			}
		}

		#region Get<native type> tests

		// these tests all follow the same pattern

		[Test]
		public void GetBooleanTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Boolean) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetBoolean(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetBoolean(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetByteTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Byte) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetByte(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetByte(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetCharTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Char) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetChar(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetChar(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetDateTimeTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(DateTime) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetDateTime(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetDateTime(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetDecimalTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Decimal) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetDecimal(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetDecimal(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetDoubleTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Double) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetDouble(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetDouble(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetFloatTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Single) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetFloat(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetFloat(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetGuidTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Guid) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetGuid(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetGuid(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetInt16Test()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Int16) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetInt16(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetInt16(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetInt32Test()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Int32) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetInt32(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetInt32(columnIndex));
					}

					recordIndex++;
				}
			}

		}

		[Test]
		public void GetInt64Test()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(Int64) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetInt64(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetInt64(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetStringTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if ((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType] == typeof(string) && expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedValues[columnIndex], reader.GetString(columnIndex));
						else
							Assert.Throws<InvalidCastException>(() => reader.GetString(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		#endregion

		[Test]
		public void GetBytesTest()
		{
			DataTable expectedSchema = GetExpectedSchema();
			DataRow stringColumnSchema = expectedSchema.Select(string.Format("{0}='{1}'", SchemaTableColumn.ColumnName, typeof(string).FullName))[0];
			int stringColumnIndex = (int) stringColumnSchema[SchemaTableColumn.ColumnOrdinal];

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;

				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					if (expectedValues[stringColumnIndex] != DBNull.Value)
					{
						var value = (string) expectedValues[stringColumnIndex];

						Byte[] expected = Encoding.Unicode.GetBytes(value);
						Byte[] actual = new Byte[expected.Length];

						long count = reader.GetBytes(stringColumnIndex, 0, actual, 0, actual.Length);

						Assert.AreEqual(expected.Length, (int) count);
						Assert.AreEqual(expected.Length, actual.Length);
						CollectionAssert.AreEqual(expected, actual);
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetCharsTest()
		{
			DataTable expectedSchema = GetExpectedSchema();
			DataRow stringColumnSchema = expectedSchema.Select(string.Format("{0}='{1}'", SchemaTableColumn.ColumnName, typeof(string).FullName))[0];
			int stringColumnIndex = (int) stringColumnSchema[SchemaTableColumn.ColumnOrdinal];

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;

				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					if (expectedValues[stringColumnIndex] != DBNull.Value)
					{
						var value = (string) expectedValues[stringColumnIndex];

						Char[] expected = value.ToCharArray();
						Char[] actual = new Char[expected.Length];

						long count = reader.GetChars(stringColumnIndex, 0, actual, 0, actual.Length);

						Assert.AreEqual(expected.Length, (int) count);
						Assert.AreEqual(expected.Length, actual.Length);
						CollectionAssert.AreEqual(expected, actual);
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetDataTypeNameTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					for (int columnIndex = 0; columnIndex < expectedSchema.Rows.Count; columnIndex++)
					{
						Assert.AreEqual(((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType]).FullName, reader.GetDataTypeName(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetFieldTypeTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					for (int columnIndex = 0; columnIndex < expectedSchema.Rows.Count; columnIndex++)
					{
						Assert.AreEqual((Type) expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType], reader.GetFieldType(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetNameTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					for (int columnIndex = 0; columnIndex < expectedSchema.Rows.Count; columnIndex++)
					{
						Assert.AreEqual((string) expectedSchema.Rows[columnIndex][SchemaTableColumn.ColumnName], reader.GetName(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetOrdinalTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					for (int columnIndex = 0; columnIndex < expectedSchema.Rows.Count; columnIndex++)
					{
						string columnName = (string) expectedSchema.Rows[columnIndex][SchemaTableColumn.ColumnName];
						Assert.AreEqual(columnIndex, reader.GetOrdinal(columnName));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetValueTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if (expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType], reader.GetValue(columnIndex).GetType());

						Assert.AreEqual(expectedValues[columnIndex], reader.GetValue(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void GetValuesTest()
		{
			DataTable expectedSchema = GetExpectedSchema();

			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);
					object[] actualValues = new object[expectedValues.Count];

					Assert.AreEqual(expectedValues.Count, reader.GetValues(actualValues));

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						if (expectedValues[columnIndex] != DBNull.Value)
							Assert.AreEqual(expectedSchema.Rows[columnIndex][SchemaTableColumn.DataType], actualValues[columnIndex].GetType());

						Assert.AreEqual(expectedValues[columnIndex], actualValues[columnIndex]);
					}

					recordIndex++;
				}
			}
		}

		[Test]
		public void IsDBNullTest()
		{
			using (IDataReader reader = CreateDataReaderInstance())
			{
				long recordIndex = 0;
				while (reader.Read())
				{
					IList<object> expectedValues = GetExpectedFieldValues(recordIndex);

					for (int columnIndex = 0; columnIndex < expectedValues.Count; columnIndex++)
					{
						bool expected = (expectedValues[columnIndex] == DBNull.Value);
						Assert.AreEqual(expected, reader.IsDBNull(columnIndex));
					}

					recordIndex++;
				}
			}
		}

		#endregion
	}
}