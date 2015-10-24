// Author(s): Sébastien Lorion

using System;
using System.Data;

namespace NLight.Data
{
	/// <summary>
	/// Contains methods to convert <see cref="DbType"/> to and from various types.
	/// </summary>
	public static class DbTypeExtensions
	{
		/// <summary>
		/// Converts the specified <see cref="DbType"/> to the corresponding <see cref="Type"/>.
		/// </summary>
		/// <param name="dbType">The <see cref="DbType"/> to convert.</param>
		/// <returns>The corresponding <see cref="Type"/>.</returns>
		public static Type ToType(this DbType dbType)
		{
			switch (dbType)
			{
				case DbType.AnsiString: return typeof(string);
				case DbType.AnsiStringFixedLength: return typeof(string);
				case DbType.Binary: return typeof(byte[]);
				case DbType.Boolean: return typeof(bool);
				case DbType.Byte: return typeof(Byte);
				case DbType.Currency: return typeof(Double);
				case DbType.Date: return typeof(DateTime);
				case DbType.DateTime: return typeof(DateTime);
				case DbType.Decimal: return typeof(decimal);
				case DbType.Double: return typeof(Double);
				case DbType.Guid: return typeof(Guid);
				case DbType.Int16: return typeof(Int16);
				case DbType.Int32: return typeof(Int32);
				case DbType.Int64: return typeof(Int64);
				case DbType.Object: return typeof(object);
				case DbType.SByte: return typeof(SByte);
				case DbType.Single: return typeof(Single);
				case DbType.String: return typeof(string);
				case DbType.StringFixedLength: return typeof(string);
				case DbType.Time: return typeof(DateTime);
				case DbType.UInt16: return typeof(UInt16);
				case DbType.UInt32: return typeof(UInt32);
				case DbType.UInt64: return typeof(UInt64);
				case DbType.VarNumeric: return typeof(decimal);
				case DbType.Xml: return typeof(string); //TODO: verify if DbType.Xml should translate to typeof(string)
				default: throw new ArgumentException(string.Empty, "dbType");
			}
		}

		/// <summary>
		/// Converts from the specified <see cref="Type"/> to the corresponding <see cref="DbType"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to convert.</param>
		/// <returns>The corresponding <see cref="DbType"/>.</returns>
		public static DbType ToDbType(this Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			switch (Convert.GetTypeCode(type))
			{
				case TypeCode.Boolean: return DbType.Boolean;
				case TypeCode.Byte: return DbType.Byte;
				case TypeCode.Char: return DbType.String;
				case TypeCode.DBNull: return DbType.Object;
				case TypeCode.DateTime: return DbType.DateTime;
				case TypeCode.Decimal: return DbType.Decimal;
				case TypeCode.Double: return DbType.Double;
				case TypeCode.Empty: return DbType.Object;
				case TypeCode.Int16: return DbType.Int16;
				case TypeCode.Int32: return DbType.Int32;
				case TypeCode.Int64: return DbType.Int64;
				case TypeCode.Object: return DbType.Object;
				case TypeCode.SByte: return DbType.SByte;
				case TypeCode.Single: return DbType.Single;
				case TypeCode.String: return DbType.String;
				case TypeCode.UInt16: return DbType.UInt16;
				case TypeCode.UInt32: return DbType.UInt32;
				case TypeCode.UInt64: return DbType.UInt64;
				default: return DbType.Object;
			}
		}
	}
}