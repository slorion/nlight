// Author(s): Sébastien Lorion

using NLight.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace NLight.Text
{
	public class StringValueConverter
	{
		private string[] _expectedDateTimeFormats;

		public StringValueConverter()
		{
			_expectedDateTimeFormats = new string[] { };

			this.DateTimeStyles = DateTimeStyles.None;
			this.DateTimeConversionOption = ConversionOption.Loose;
			this.IntegerNumberStyles = NumberStyles.Integer;
			this.FloatingPointNumberStyles = NumberStyles.Float;
		}

		public ConversionOption DateTimeConversionOption { get; set; }
		public DateTimeStyles DateTimeStyles { get; set; }
		public NumberStyles IntegerNumberStyles { get; set; }
		public NumberStyles FloatingPointNumberStyles { get; set; }

		public string[] GetExpectedDateTimeFormats()
		{
			return _expectedDateTimeFormats == null ? null : (string[]) _expectedDateTimeFormats.Clone();
		}

		public void SetExpectedDateTimeFormats(params string[] formats)
		{
			if (formats == null) throw new ArgumentNullException(nameof(formats));

			var copy = new string[formats.Length];
			Array.Copy(formats, copy, formats.Length);

			Interlocked.Exchange(ref _expectedDateTimeFormats, copy);
		}

		public void SetExpectedDateTimeFormats(IEnumerable<string> formats)
		{
			var list = new List<string>(formats);
			Interlocked.Exchange(ref _expectedDateTimeFormats, list.ToArray());
		}

		private static string TrimWhiteSpaces(string value, TrimmingOptions options)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if ((options & TrimmingOptions.Both) == TrimmingOptions.Both)
					value = value.Trim();
				else if ((options & TrimmingOptions.End) == TrimmingOptions.End)
					value = value.TrimEnd();
				else if ((options & TrimmingOptions.Start) == TrimmingOptions.Start)
					value = value.TrimStart();
			}

			return value;
		}

		#region ConvertFrom

		public string ConvertFrom(object value, string nullString = "", string format = null, IFormatProvider culture = null)
		{
			if (value == null || Convert.IsDBNull(value))
				return nullString;
			else if (value is string)
				return (string) value;
			else
			{
				if (culture == null)
					culture = CultureInfo.CurrentCulture;

				TypeCode typeCode = Type.GetTypeCode(value.GetType());

				switch (typeCode)
				{
					case TypeCode.DateTime:
						if (string.IsNullOrEmpty(format))
							return Convert.ToString((DateTime) value, culture);
						else
							return string.Format(culture, "{0:" + format + "}", value);

					default:
						if (string.IsNullOrEmpty(format))
							return System.Convert.ToString(value, culture);
						else
							return string.Format(culture, "{0:" + format + "}", value);
				}
			}
		}

		#endregion

		#region ConvertTo

		#region Untyped

		public T ConvertTo<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue) => ConvertTo<T>(input, trimWhiteSpaces, defaultValue, null);
		public T ConvertTo<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, IFormatProvider culture) => (T) ConvertTo(input, trimWhiteSpaces, typeof(T), defaultValue, culture);

		public bool TryConvertTo<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, out T value) => TryConvertTo<T>(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertTo<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, IFormatProvider culture, out T value)
		{
			object result;

			if (TryConvertTo(input, trimWhiteSpaces, typeof(T), defaultValue, culture, out result))
			{
				value = (T) result;
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		public object ConvertTo(string input, TrimmingOptions trimWhiteSpaces, Type dataType, object defaultValue) => ConvertTo(input, trimWhiteSpaces, dataType, defaultValue, null);
		public object ConvertTo(string input, TrimmingOptions trimWhiteSpaces, Type dataType, object defaultValue, IFormatProvider culture)
		{
			object value;

			if (TryConvertTo(input, trimWhiteSpaces, dataType, defaultValue, culture, out value))
				return value;
			else
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Text_CannotConvertToDataType, input, dataType), nameof(input));
		}

		public bool TryConvertTo(string input, TrimmingOptions trimWhiteSpaces, Type dataType, object defaultValue, out object value) => TryConvertTo(input, trimWhiteSpaces, dataType, defaultValue, null, out value);
		public bool TryConvertTo(string input, TrimmingOptions trimWhiteSpaces, Type dataType, object defaultValue, IFormatProvider culture, out object value)
		{
			if (dataType == null) throw new ArgumentNullException(nameof(dataType));

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (dataType == typeof(string))
			{
				if (string.IsNullOrEmpty(input))
					value = defaultValue;
				else
					value = input;

				return true;
			}

			TypeCode typeCode;

			Type underlyingType = Nullable.GetUnderlyingType(dataType);

			if (underlyingType != null)
			{
				if (string.IsNullOrEmpty(input))
				{
					value = defaultValue;
					return true;
				}

				dataType = underlyingType;
				typeCode = Type.GetTypeCode(dataType);
			}
			else
			{
				if (string.IsNullOrEmpty(input))
					return false;

				typeCode = Type.GetTypeCode(dataType);
			}

			// Handle the case where the provided default value is null, 
			// so the casts are not failing when calling the specific conversion methods below.
			// The corner case where the value is empty/null and the default value is also null has been handled already in the code above.
			if (defaultValue == null && dataType.IsValueType)
				defaultValue = Activator.CreateInstance(dataType);

			if (dataType.IsEnum)
				return TryConvertToEnum(dataType, input, trimWhiteSpaces, (Enum) defaultValue, culture, out value);
			else if (dataType == typeof(Guid))
			{
				Guid output;

				if (!TryConvertToGuid(input, trimWhiteSpaces, (Guid) defaultValue, culture, out output))
					return false;

				value = output;
				return true;
			}

			switch (typeCode)
			{
				// TypeCode.String has already been handled earlier

				// most probable conversions first

				case TypeCode.Int32:
					{
						Int32 output;
						if (!TryConvertToInt32(input, trimWhiteSpaces, (Int32) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Int64:
					{
						Int64 output;
						if (!TryConvertToInt64(input, trimWhiteSpaces, (Int64) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Double:
					{
						Double output;
						if (!TryConvertToDouble(input, trimWhiteSpaces, (Double) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Decimal:
					{
						Decimal output;
						if (!TryConvertToDecimal(input, trimWhiteSpaces, (Decimal) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.DateTime:
					{
						DateTime output;
						if (!TryConvertToDateTime(input, trimWhiteSpaces, (DateTime) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Boolean:
					{
						Boolean output;
						if (!TryConvertToBoolean(input, trimWhiteSpaces, (Boolean) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Single:
					{
						Single output;
						if (!TryConvertToSingle(input, trimWhiteSpaces, (Single) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Int16:
					{
						Int16 output;
						if (!TryConvertToInt16(input, trimWhiteSpaces, (Int16) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Byte:
					{
						Byte output;
						if (!TryConvertToByte(input, trimWhiteSpaces, (Byte) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.SByte:
					{
						SByte output;
						if (!TryConvertToSByte(input, trimWhiteSpaces, (SByte) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.UInt16:
					{
						UInt16 output;
						if (!TryConvertToUInt16(input, trimWhiteSpaces, (UInt16) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.UInt32:
					{
						UInt32 output;
						if (!TryConvertToUInt32(input, trimWhiteSpaces, (UInt32) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.UInt64:
					{
						UInt64 output;
						if (!TryConvertToUInt64(input, trimWhiteSpaces, (UInt64) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				case TypeCode.Char:
					{
						Char output;
						if (!TryConvertToChar(input, trimWhiteSpaces, (Char) defaultValue, culture, out output))
							return false;

						value = output;
						return true;
					}

				// unsupported conversions
				case TypeCode.Object:
				case TypeCode.DBNull:
				case TypeCode.Empty:
				default:
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Text_ConversionNotSupported, typeof(string), dataType), "dataType");
			}
		}

		#endregion

		#region Typed

		private static T TryOrThrow<T>(bool success, string input, T value)
		{
			if (success)
				return value;
			else
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Text_CannotConvertToDataType, input, typeof(T)), nameof(input));
		}

		#region Boolean

		public Boolean ConvertToBoolean(string input, TrimmingOptions trimWhiteSpaces, Boolean defaultValue) => ConvertToBoolean(input, trimWhiteSpaces, defaultValue, null);
		public Boolean ConvertToBoolean(string input, TrimmingOptions trimWhiteSpaces, Boolean defaultValue, IFormatProvider culture) { Boolean value; return TryOrThrow(TryConvertToBoolean(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Boolean? ConvertToNullableBoolean(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableBoolean(input, trimWhiteSpaces, null);
		public Boolean? ConvertToNullableBoolean(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Boolean? value; return TryOrThrow(TryConvertToNullableBoolean(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToBoolean(string input, TrimmingOptions trimWhiteSpaces, Boolean defaultValue, out Boolean value) => TryConvertToBoolean(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableBoolean(string input, TrimmingOptions trimWhiteSpaces, out Boolean? value) => TryConvertToNullableBoolean(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToBoolean(string input, TrimmingOptions trimWhiteSpaces, Boolean defaultValue, IFormatProvider culture, out Boolean value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else
			{
				int intValue;

				if (Int32.TryParse(input, NumberStyles.None, culture, out intValue))
					value = (intValue != 0);
				else if (!Boolean.TryParse(input, out value))
					return false;
			}

			return true;
		}

		public bool TryConvertToNullableBoolean(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Boolean? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Boolean output;
				int intValue;

				if (Int32.TryParse(input, NumberStyles.None, culture, out intValue))
					output = (intValue != 0);
				else if (!Boolean.TryParse(input, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Char

		public Char ConvertToChar(string input, TrimmingOptions trimWhiteSpaces, Char defaultValue) => ConvertToChar(input, trimWhiteSpaces, defaultValue, null);
		public Char ConvertToChar(string input, TrimmingOptions trimWhiteSpaces, Char defaultValue, IFormatProvider culture) { Char value; return TryOrThrow(TryConvertToChar(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Char? ConvertToNullableChar(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableChar(input, trimWhiteSpaces, null);
		public Char? ConvertToNullableChar(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Char? value; return TryOrThrow(TryConvertToNullableChar(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToChar(string input, TrimmingOptions trimWhiteSpaces, Char defaultValue, out Char value) => TryConvertToChar(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableChar(string input, TrimmingOptions trimWhiteSpaces, out Char? value) => TryConvertToNullableChar(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToChar(string input, TrimmingOptions trimWhiteSpaces, Char defaultValue, IFormatProvider culture, out Char value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Char.TryParse(input, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableChar(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Char? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Char output;
				if (!Char.TryParse(input, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Enumeration

		public T ConvertToEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue) where T : struct => ConvertToEnum<T>(input, trimWhiteSpaces, defaultValue, null);
		public T ConvertToEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, IFormatProvider culture) where T : struct { T value; return TryOrThrow<T>(TryConvertToEnum<T>(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public T ConvertToNullableEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue) where T : struct => ConvertToEnum<T>(input, trimWhiteSpaces, defaultValue, null);
		public T ConvertToNullableEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, IFormatProvider culture) where T : struct { T value; return TryOrThrow<T>(TryConvertToEnum<T>(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public bool TryConvertToEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, out T value) where T : struct => TryConvertToEnum<T>(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableEnum<T>(string input, TrimmingOptions trimWhiteSpaces, out T? value) where T : struct => TryConvertToNullableEnum<T>(input, trimWhiteSpaces, null, out value);

		public object ConvertToEnum(Type enumType, string input, TrimmingOptions trimWhiteSpaces, object defaultValue) => ConvertToEnum(enumType, input, trimWhiteSpaces, defaultValue, null);
		public object ConvertToEnum(Type enumType, string input, TrimmingOptions trimWhiteSpaces, object defaultValue, IFormatProvider culture) { object value; return TryOrThrow(TryConvertToEnum(enumType, input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }

		public bool TryConvertToEnum<T>(string input, TrimmingOptions trimWhiteSpaces, T defaultValue, IFormatProvider culture, out T value)
			where T : struct
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!EnumHelper.TryParse(input, true, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableEnum<T>(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out T? value)
			where T : struct
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				T output;
				if (!EnumHelper.TryParse(input, true, out output))
					return false;

				value = output;
			}

			return true;
		}

		public bool TryConvertToEnum(Type enumType, string input, TrimmingOptions trimWhiteSpaces, object defaultValue, IFormatProvider culture, out object value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!EnumHelper.TryParse(enumType, input, true, out value))
				return false;

			return true;
		}

		#endregion

		#region GUID

		public Guid ConvertToGuid(string input, TrimmingOptions trimWhiteSpaces, Guid defaultValue) => ConvertToGuid(input, trimWhiteSpaces, defaultValue, null);
		public Guid ConvertToGuid(string input, TrimmingOptions trimWhiteSpaces, Guid defaultValue, IFormatProvider culture) { Guid value; return TryOrThrow(TryConvertToGuid(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Guid? ConvertToNullableGuid(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableGuid(input, trimWhiteSpaces, null);
		public Guid? ConvertToNullableGuid(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Guid? value; return TryOrThrow(TryConvertToNullableGuid(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToGuid(string input, TrimmingOptions trimWhiteSpaces, Guid defaultValue, out Guid value) => TryConvertToGuid(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableGuid(string input, TrimmingOptions trimWhiteSpaces, out Guid? value) => TryConvertToNullableGuid(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToGuid(string input, TrimmingOptions trimWhiteSpaces, Guid defaultValue, IFormatProvider culture, out Guid value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Guid.TryParse(input, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableGuid(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Guid? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Guid output;
				if (!TryConvertToGuid(input, TrimmingOptions.None, default(Guid), culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region DateTime

		public DateTime ConvertToDateTime(string input, TrimmingOptions trimWhiteSpaces, DateTime defaultValue) => ConvertToDateTime(input, trimWhiteSpaces, defaultValue, null);
		public DateTime ConvertToDateTime(string input, TrimmingOptions trimWhiteSpaces, DateTime defaultValue, IFormatProvider culture) { DateTime value; return TryOrThrow(TryConvertToDateTime(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public DateTime? ConvertToNullableDateTime(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableDateTime(input, trimWhiteSpaces, null);
		public DateTime? ConvertToNullableDateTime(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { DateTime? value; return TryOrThrow(TryConvertToNullableDateTime(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToDateTime(string input, TrimmingOptions trimWhiteSpaces, DateTime defaultValue, out DateTime value) => TryConvertToDateTime(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableDateTime(string input, TrimmingOptions trimWhiteSpaces, out DateTime? value) => TryConvertToNullableDateTime(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToDateTime(string input, TrimmingOptions trimWhiteSpaces, DateTime defaultValue, IFormatProvider culture, out DateTime value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else
			{
				if (_expectedDateTimeFormats == null || _expectedDateTimeFormats.Length == 0 || !DateTime.TryParseExact(input, _expectedDateTimeFormats, culture, this.DateTimeStyles, out value))
				{
					if (this.DateTimeConversionOption == ConversionOption.Loose)
					{
						if (!DateTime.TryParseExact(input, "G", culture, this.DateTimeStyles, out value))
						{
							if (!DateTime.TryParse(input, culture, this.DateTimeStyles, out value))
								return false;
						}
					}
					else
					{
						value = defaultValue;
						return false;
					}
				}
			}

			return true;
		}

		public bool TryConvertToNullableDateTime(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out DateTime? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				DateTime output;
				if (!TryConvertToDateTime(input, TrimmingOptions.None, DateTime.MinValue, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region DateTimeOffset

		public DateTimeOffset ConvertToDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, DateTimeOffset defaultValue) => ConvertToDateTimeOffset(input, trimWhiteSpaces, defaultValue, null);
		public DateTimeOffset ConvertToDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, DateTimeOffset defaultValue, IFormatProvider culture) { DateTimeOffset value; return TryOrThrow(TryConvertToDateTimeOffset(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public DateTimeOffset? ConvertToNullableDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableDateTime(input, trimWhiteSpaces, null);
		public DateTimeOffset? ConvertToNullableDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { DateTimeOffset? value; return TryOrThrow(TryConvertToNullableDateTimeOffset(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, DateTimeOffset defaultValue, out DateTimeOffset value) => TryConvertToDateTimeOffset(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, out DateTimeOffset? value) => TryConvertToNullableDateTimeOffset(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, DateTimeOffset defaultValue, IFormatProvider culture, out DateTimeOffset value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else
			{
				if (_expectedDateTimeFormats == null || _expectedDateTimeFormats.Length == 0 || !DateTimeOffset.TryParseExact(input, _expectedDateTimeFormats, culture, this.DateTimeStyles, out value))
				{
					if (this.DateTimeConversionOption == ConversionOption.Loose)
					{
						if (!DateTimeOffset.TryParseExact(input, "G", culture, this.DateTimeStyles, out value))
						{
							if (!DateTimeOffset.TryParse(input, culture, this.DateTimeStyles, out value))
								return false;
						}
					}
					else
					{
						value = defaultValue;
						return false;
					}
				}
			}

			return true;
		}

		public bool TryConvertToNullableDateTimeOffset(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out DateTimeOffset? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				DateTimeOffset output;
				if (!TryConvertToDateTimeOffset(input, TrimmingOptions.None, DateTime.MinValue, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region TimeSpan

		public TimeSpan ConvertToTimeSpan(string input, TrimmingOptions trimWhiteSpaces, TimeSpan defaultValue) => ConvertToTimeSpan(input, trimWhiteSpaces, defaultValue, null);
		public TimeSpan ConvertToTimeSpan(string input, TrimmingOptions trimWhiteSpaces, TimeSpan defaultValue, IFormatProvider culture) { TimeSpan value; return TryOrThrow(TryConvertToTimeSpan(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public TimeSpan? ConvertToNullableTimeSpan(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableTimeSpan(input, trimWhiteSpaces, null);
		public TimeSpan? ConvertToNullableTimeSpan(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { TimeSpan? value; return TryOrThrow(TryConvertToNullableTimeSpan(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToTimeSpan(string input, TrimmingOptions trimWhiteSpaces, TimeSpan defaultValue, out TimeSpan value) => TryConvertToTimeSpan(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableTimeSpan(string input, TrimmingOptions trimWhiteSpaces, out TimeSpan? value) => TryConvertToNullableTimeSpan(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToTimeSpan(string input, TrimmingOptions trimWhiteSpaces, TimeSpan defaultValue, IFormatProvider culture, out TimeSpan value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!TimeSpan.TryParse(input, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableTimeSpan(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out TimeSpan? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				TimeSpan output;
				if (!TimeSpan.TryParse(input, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region SByte

		[CLSCompliant(false)] public SByte ConvertToSByte(string input, TrimmingOptions trimWhiteSpaces, SByte defaultValue) => ConvertToSByte(input, trimWhiteSpaces, defaultValue, null);
		[CLSCompliant(false)] public SByte ConvertToSByte(string input, TrimmingOptions trimWhiteSpaces, SByte defaultValue, IFormatProvider culture) { SByte value; return TryOrThrow(TryConvertToSByte(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		[CLSCompliant(false)] public SByte? ConvertToNullableSByte(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableSByte(input, trimWhiteSpaces, null);
		[CLSCompliant(false)] public SByte? ConvertToNullableSByte(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { SByte? value; return TryOrThrow(TryConvertToNullableSByte(input, trimWhiteSpaces, culture, out value), input, value); }
		[CLSCompliant(false)] public bool TryConvertToSByte(string input, TrimmingOptions trimWhiteSpaces, SByte defaultValue, out SByte value) => TryConvertToSByte(input, trimWhiteSpaces, defaultValue, null, out value);
		[CLSCompliant(false)] public bool TryConvertToNullableSByte(string input, TrimmingOptions trimWhiteSpaces, out SByte? value) => TryConvertToNullableSByte(input, trimWhiteSpaces, null, out value);

		[CLSCompliant(false)]
		public bool TryConvertToSByte(string input, TrimmingOptions trimWhiteSpaces, SByte defaultValue, IFormatProvider culture, out SByte value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!SByte.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		[CLSCompliant(false)]
		public bool TryConvertToNullableSByte(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out SByte? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				SByte output;
				if (!SByte.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Byte

		public Byte ConvertToByte(string input, TrimmingOptions trimWhiteSpaces, Byte defaultValue) => ConvertToByte(input, trimWhiteSpaces, defaultValue, null);
		public Byte ConvertToByte(string input, TrimmingOptions trimWhiteSpaces, Byte defaultValue, IFormatProvider culture) { Byte value; return TryOrThrow(TryConvertToByte(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Byte? ConvertToNullableByte(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableByte(input, trimWhiteSpaces, null);
		public Byte? ConvertToNullableByte(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Byte? value; return TryOrThrow(TryConvertToNullableByte(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToByte(string input, TrimmingOptions trimWhiteSpaces, Byte defaultValue, out Byte value) => TryConvertToByte(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableByte(string input, TrimmingOptions trimWhiteSpaces, out Byte? value) => TryConvertToNullableByte(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToByte(string input, TrimmingOptions trimWhiteSpaces, Byte defaultValue, IFormatProvider culture, out Byte value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Byte.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableByte(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Byte? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Byte output;
				if (!Byte.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Int16

		public Int16 ConvertToInt16(string input, TrimmingOptions trimWhiteSpaces, Int16 defaultValue) => ConvertToInt16(input, trimWhiteSpaces, defaultValue, null);
		public Int16 ConvertToInt16(string input, TrimmingOptions trimWhiteSpaces, Int16 defaultValue, IFormatProvider culture) { Int16 value; return TryOrThrow(TryConvertToInt16(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Int16? ConvertToNullableInt16(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableInt16(input, trimWhiteSpaces, null);
		public Int16? ConvertToNullableInt16(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Int16? value; return TryOrThrow(TryConvertToNullableInt16(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToInt16(string input, TrimmingOptions trimWhiteSpaces, Int16 defaultValue, out Int16 value) => TryConvertToInt16(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableInt16(string input, TrimmingOptions trimWhiteSpaces, out Int16? value) => TryConvertToNullableInt16(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToInt16(string input, TrimmingOptions trimWhiteSpaces, Int16 defaultValue, IFormatProvider culture, out Int16 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Int16.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableInt16(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Int16? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Int16 output;
				if (!Int16.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region UInt16

		[CLSCompliant(false)] public UInt16 ConvertToUInt16(string input, TrimmingOptions trimWhiteSpaces, UInt16 defaultValue) => ConvertToUInt16(input, trimWhiteSpaces, defaultValue, null);
		[CLSCompliant(false)] public UInt16 ConvertToUInt16(string input, TrimmingOptions trimWhiteSpaces, UInt16 defaultValue, IFormatProvider culture) { UInt16 value; return TryOrThrow(TryConvertToUInt16(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		[CLSCompliant(false)] public UInt16? ConvertToNullableUInt16(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableUInt16(input, trimWhiteSpaces, null);
		[CLSCompliant(false)] public UInt16? ConvertToNullableUInt16(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { UInt16? value; return TryOrThrow(TryConvertToNullableUInt16(input, trimWhiteSpaces, culture, out value), input, value); }
		[CLSCompliant(false)] public bool TryConvertToUInt16(string input, TrimmingOptions trimWhiteSpaces, UInt16 defaultValue, out UInt16 value) => TryConvertToUInt16(input, trimWhiteSpaces, defaultValue, null, out value);
		[CLSCompliant(false)] public bool TryConvertToNullableUInt16(string input, TrimmingOptions trimWhiteSpaces, out UInt16? value) => TryConvertToNullableUInt16(input, trimWhiteSpaces, null, out value);

		[CLSCompliant(false)]
		public bool TryConvertToUInt16(string input, TrimmingOptions trimWhiteSpaces, UInt16 defaultValue, IFormatProvider culture, out UInt16 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!UInt16.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		[CLSCompliant(false)]
		public bool TryConvertToNullableUInt16(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out UInt16? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				UInt16 output;
				if (!UInt16.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Int32

		public Int32 ConvertToInt32(string input, TrimmingOptions trimWhiteSpaces, Int32 defaultValue) => ConvertToInt32(input, trimWhiteSpaces, defaultValue, null);
		public Int32 ConvertToInt32(string input, TrimmingOptions trimWhiteSpaces, Int32 defaultValue, IFormatProvider culture) { Int32 value; return TryOrThrow(TryConvertToInt32(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Int32? ConvertToNullableInt32(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableInt32(input, trimWhiteSpaces, null);
		public Int32? ConvertToNullableInt32(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Int32? value; return TryOrThrow(TryConvertToNullableInt32(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToInt32(string input, TrimmingOptions trimWhiteSpaces, Int32 defaultValue, out Int32 value) => TryConvertToInt32(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableInt32(string input, TrimmingOptions trimWhiteSpaces, out Int32? value) => TryConvertToNullableInt32(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToInt32(string input, TrimmingOptions trimWhiteSpaces, Int32 defaultValue, IFormatProvider culture, out Int32 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Int32.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableInt32(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Int32? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Int32 output;
				if (!Int32.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region UInt32

		[CLSCompliant(false)] public UInt32 ConvertToUInt32(string input, TrimmingOptions trimWhiteSpaces, UInt32 defaultValue) => ConvertToUInt32(input, trimWhiteSpaces, defaultValue, null);
		[CLSCompliant(false)] public UInt32 ConvertToUInt32(string input, TrimmingOptions trimWhiteSpaces, UInt32 defaultValue, IFormatProvider culture) { UInt32 value; return TryOrThrow(TryConvertToUInt32(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		[CLSCompliant(false)] public UInt32? ConvertToNullableUInt32(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableUInt32(input, trimWhiteSpaces, null);
		[CLSCompliant(false)] public UInt32? ConvertToNullableUInt32(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { UInt32? value; return TryOrThrow(TryConvertToNullableUInt32(input, trimWhiteSpaces, culture, out value), input, value); }
		[CLSCompliant(false)] public bool TryConvertToUInt32(string input, TrimmingOptions trimWhiteSpaces, UInt32 defaultValue, out UInt32 value) => TryConvertToUInt32(input, trimWhiteSpaces, defaultValue, null, out value);
		[CLSCompliant(false)] public bool TryConvertToNullableUInt32(string input, TrimmingOptions trimWhiteSpaces, out UInt32? value) => TryConvertToNullableUInt32(input, trimWhiteSpaces, null, out value);

		[CLSCompliant(false)]
		public bool TryConvertToUInt32(string input, TrimmingOptions trimWhiteSpaces, UInt32 defaultValue, IFormatProvider culture, out UInt32 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!UInt32.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		[CLSCompliant(false)]
		public bool TryConvertToNullableUInt32(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out UInt32? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				UInt32 output;
				if (!UInt32.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Int64

		public Int64 ConvertToInt64(string input, TrimmingOptions trimWhiteSpaces, Int64 defaultValue) => ConvertToInt64(input, trimWhiteSpaces, defaultValue, null);
		public Int64 ConvertToInt64(string input, TrimmingOptions trimWhiteSpaces, Int64 defaultValue, IFormatProvider culture) { Int64 value; return TryOrThrow(TryConvertToInt64(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Int64? ConvertToNullableInt64(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableInt64(input, trimWhiteSpaces, null);
		public Int64? ConvertToNullableInt64(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Int64? value; return TryOrThrow(TryConvertToNullableInt64(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToInt64(string input, TrimmingOptions trimWhiteSpaces, Int64 defaultValue, out Int64 value) => TryConvertToInt64(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableInt64(string input, TrimmingOptions trimWhiteSpaces, out Int64? value) => TryConvertToNullableInt64(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToInt64(string input, TrimmingOptions trimWhiteSpaces, Int64 defaultValue, IFormatProvider culture, out Int64 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Int64.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableInt64(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Int64? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Int64 output;
				if (!Int64.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region UInt64

		[CLSCompliant(false)] public UInt64 ConvertToUInt64(string input, TrimmingOptions trimWhiteSpaces, UInt64 defaultValue) => ConvertToUInt64(input, trimWhiteSpaces, defaultValue, null);
		[CLSCompliant(false)] public UInt64 ConvertToUInt64(string input, TrimmingOptions trimWhiteSpaces, UInt64 defaultValue, IFormatProvider culture) { UInt64 value; return TryOrThrow(TryConvertToUInt64(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		[CLSCompliant(false)] public UInt64? ConvertToNullableUInt64(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableUInt64(input, trimWhiteSpaces, null);
		[CLSCompliant(false)] public UInt64? ConvertToNullableUInt64(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { UInt64? value; return TryOrThrow(TryConvertToNullableUInt64(input, trimWhiteSpaces, culture, out value), input, value); }
		[CLSCompliant(false)] public bool TryConvertToUInt64(string input, TrimmingOptions trimWhiteSpaces, UInt64 defaultValue, out UInt64 value) => TryConvertToUInt64(input, trimWhiteSpaces, defaultValue, null, out value);
		[CLSCompliant(false)] public bool TryConvertToNullableUInt64(string input, TrimmingOptions trimWhiteSpaces, out UInt64? value) => TryConvertToNullableUInt64(input, trimWhiteSpaces, null, out value);

		[CLSCompliant(false)]
		public bool TryConvertToUInt64(string input, TrimmingOptions trimWhiteSpaces, UInt64 defaultValue, IFormatProvider culture, out UInt64 value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!UInt64.TryParse(input, this.IntegerNumberStyles, culture, out value))
				return false;

			return true;
		}

		[CLSCompliant(false)]
		public bool TryConvertToNullableUInt64(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out UInt64? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				UInt64 output;
				if (!UInt64.TryParse(input, this.IntegerNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Single

		public Single ConvertToSingle(string input, TrimmingOptions trimWhiteSpaces, Single defaultValue) => ConvertToSingle(input, trimWhiteSpaces, defaultValue, null);
		public Single ConvertToSingle(string input, TrimmingOptions trimWhiteSpaces, Single defaultValue, IFormatProvider culture) { Single value; return TryOrThrow(TryConvertToSingle(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Single? ConvertToNullableSingle(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableSingle(input, trimWhiteSpaces, null);
		public Single? ConvertToNullableSingle(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Single? value; return TryOrThrow(TryConvertToNullableSingle(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToSingle(string input, TrimmingOptions trimWhiteSpaces, Single defaultValue, out Single value) => TryConvertToSingle(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableSingle(string input, TrimmingOptions trimWhiteSpaces, out Single? value) => TryConvertToNullableSingle(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToSingle(string input, TrimmingOptions trimWhiteSpaces, Single defaultValue, IFormatProvider culture, out Single value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Single.TryParse(input, this.FloatingPointNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableSingle(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Single? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Single output;
				if (!Single.TryParse(input, this.FloatingPointNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Double

		public Double ConvertToDouble(string input, TrimmingOptions trimWhiteSpaces, Double defaultValue) => ConvertToDouble(input, trimWhiteSpaces, defaultValue, null);
		public Double ConvertToDouble(string input, TrimmingOptions trimWhiteSpaces, Double defaultValue, IFormatProvider culture) { Double value; return TryOrThrow(TryConvertToDouble(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Double? ConvertToNullableDouble(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableDouble(input, trimWhiteSpaces, null);
		public Double? ConvertToNullableDouble(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Double? value; return TryOrThrow(TryConvertToNullableDouble(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToDouble(string input, TrimmingOptions trimWhiteSpaces, Double defaultValue, out Double value) => TryConvertToDouble(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableDouble(string input, TrimmingOptions trimWhiteSpaces, out Double? value) => TryConvertToNullableDouble(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToDouble(string input, TrimmingOptions trimWhiteSpaces, Double defaultValue, IFormatProvider culture, out Double value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Double.TryParse(input, this.FloatingPointNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableDouble(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Double? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Double output;
				if (!Double.TryParse(input, this.FloatingPointNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#region Decimal

		public Decimal ConvertToDecimal(string input, TrimmingOptions trimWhiteSpaces, Decimal defaultValue) => ConvertToDecimal(input, trimWhiteSpaces, defaultValue, null);
		public Decimal ConvertToDecimal(string input, TrimmingOptions trimWhiteSpaces, Decimal defaultValue, IFormatProvider culture) { Decimal value; return TryOrThrow(TryConvertToDecimal(input, trimWhiteSpaces, defaultValue, culture, out value), input, value); }
		public Decimal? ConvertToNullableDecimal(string input, TrimmingOptions trimWhiteSpaces) => ConvertToNullableDecimal(input, trimWhiteSpaces, null);
		public Decimal? ConvertToNullableDecimal(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture) { Decimal? value; return TryOrThrow(TryConvertToNullableDecimal(input, trimWhiteSpaces, culture, out value), input, value); }
		public bool TryConvertToDecimal(string input, TrimmingOptions trimWhiteSpaces, Decimal defaultValue, out Decimal value) => TryConvertToDecimal(input, trimWhiteSpaces, defaultValue, null, out value);
		public bool TryConvertToNullableDecimal(string input, TrimmingOptions trimWhiteSpaces, out Decimal? value) => TryConvertToNullableDecimal(input, trimWhiteSpaces, null, out value);

		public bool TryConvertToDecimal(string input, TrimmingOptions trimWhiteSpaces, Decimal defaultValue, IFormatProvider culture, out Decimal value)
		{
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (string.IsNullOrEmpty(input))
				value = defaultValue;
			else if (!Decimal.TryParse(input, this.FloatingPointNumberStyles, culture, out value))
				return false;

			return true;
		}

		public bool TryConvertToNullableDecimal(string input, TrimmingOptions trimWhiteSpaces, IFormatProvider culture, out Decimal? value)
		{
			value = null;
			input = TrimWhiteSpaces(input, trimWhiteSpaces);

			if (!string.IsNullOrEmpty(input))
			{
				Decimal output;
				if (!Decimal.TryParse(input, this.FloatingPointNumberStyles, culture, out output))
					return false;

				value = output;
			}

			return true;
		}

		#endregion

		#endregion

		#endregion
	}
}