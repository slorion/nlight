// Author(s): Sébastien Lorion

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace NLight.Core
{
	/// <summary>
	/// Contains useful functions to manipulate and parse <see cref="Enum"/> values.
	/// </summary>
	public static partial class EnumHelper
	{
		private static readonly ConcurrentDictionary<Type, EnumInfo> _cache = new ConcurrentDictionary<Type, EnumInfo>();

		/// <summary>
		/// Ensures <paramref name="enumType"/> is a valid <see cref="Enum"/> type.
		/// </summary>
		/// <param name="enumType">The type to validate.</param>
		private static void CheckEnumType(Type enumType)
		{
			if (enumType == null) throw new ArgumentNullException(nameof(enumType));
			if (!enumType.IsEnum) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Core_NotAnEnumType, enumType), nameof(enumType));
		}

		/// <summary>
		/// Gets the <see cref="EnumInfo"/> structure for the specified <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type to inspect.</param>
		/// <returns>The <see cref="EnumInfo"/> structure for the provided <paramref name="enumType"/>.</returns>
		private static EnumInfo GetEnumInfo(Type enumType)
		{
			CheckEnumType(enumType);

			return _cache.GetOrAdd(enumType,
				type =>
				{
					FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

					var names = new string[fields.Length];
					var lowerCaseNames = new string[fields.Length];
					var values = new UInt64[fields.Length];

					for (int i = 0; i < fields.Length; i++)
					{
						names[i] = fields[i].Name;
						lowerCaseNames[i] = fields[i].Name.ToLowerInvariant();
						values[i] = Convert.ToUInt64(fields[i].GetValue(null), NumberFormatInfo.InvariantInfo);
					}

					// sort values[] and keep in sync names[] and lowerCaseNames[] (insertion sort)
					for (int i = 1; i < values.Length; i++)
					{
						// invariant: array[0..i-1] is sorted
						UInt64 value = values[i];
						string name = names[i];
						string lowerCaseName = lowerCaseNames[i];

						int j = i;

						while (j > 0 && values[j - 1] > value)
						{
							values[j] = values[j - 1];
							names[j] = names[j - 1];
							lowerCaseNames[j] = lowerCaseNames[j - 1];

							j--;
						}

						values[j] = value;
						names[j] = name;
						lowerCaseNames[j] = lowerCaseName;
					}

					var info = new EnumInfo { Names = names, LowerCaseNames = lowerCaseNames, Values = values };

					if (Attribute.IsDefined(type, typeof(FlagsAttribute)))
					{
						info.IsFlags = true;

						for (int i = 0; i < info.Values.Length; i++)
							info.AllFlagsSetMask |= info.Values[i];
					}

					return info;
				});
		}

		/// <summary>
		/// Gets the name associated with the specified <paramref name="value"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <param name="value">The value to inspect.</param>
		/// <returns>The name associated with the specified <paramref name="value"/>.</returns>
		public static string GetName<T>(T value)
			where T : struct
		{
			return GetName(typeof(T), value);
		}

		/// <summary>
		/// Gets the name associated with the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <param name="value">The value to inspect.</param>
		/// <returns>The name associated with the specified <paramref name="value"/>.</returns>
		public static string GetName(Type enumType, object value)
		{
			CheckEnumType(enumType);

			EnumInfo info = GetEnumInfo(enumType);

			int index = Array.BinarySearch(info.Values, (UInt64) value);

			if (index > -1)
				return info.Names[index];
			else
				return null;
		}

		/// <summary>
		/// Gets all the names of all the values of the specified <see cref="Enum"/> type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <returns>All the names of all the values of the specified <see cref="Enum"/> type <typeparamref name="T"/>.</returns>
		public static string[] GetNames<T>()
			where T : struct
		{
			return GetNames(typeof(T));
		}

		/// <summary>
		/// Gets all the names of all the values of the specified <see cref="Enum"/> type <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <returns>All the names of all the values of the specified <see cref="Enum"/> type <paramref name="enumType"/>.</returns>
		public static string[] GetNames(Type enumType)
		{
			CheckEnumType(enumType);

			EnumInfo info = GetEnumInfo(enumType);

			string[] names = new string[info.Names.Length];
			Array.Copy(info.Names, names, names.Length);

			return names;
		}

		/// <summary>
		/// Gets all the values of the specified <see cref="Enum"/> type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <returns>All the values of the specified <see cref="Enum"/> type <typeparamref name="T"/>.</returns>
		public static T[] GetValues<T>()
			where T : struct
		{
			Type enumType = typeof(T);
			CheckEnumType(enumType);

			EnumInfo info = GetEnumInfo(enumType);

			T[] values = new T[info.Values.Length];

			for (int i = 0; i < values.Length; i++)
				values[i] = (T) Enum.ToObject(enumType, info.Values[i]);

			return values;
		}

		/// <summary>
		/// Gets all the values of the specified <see cref="Enum"/> type <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <returns>All the values of the specified <see cref="Enum"/> type <paramref name="enumType"/>.</returns>
		public static Array GetValues(Type enumType)
		{
			CheckEnumType(enumType);

			EnumInfo info = GetEnumInfo(enumType);

			Array values = Array.CreateInstance(enumType, info.Values.Length);

			for (int i = 0; i < values.Length; i++)
				values.SetValue(Enum.ToObject(enumType, info.Values[i]), i);

			return values;
		}

		/// <summary>
		/// Determines whether the specified value is defined in the <see cref="Enum"/> type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <param name="value">The value to inspect.</param>
		/// <returns><c>true</c> if the specified value is defined in the <see cref="Enum"/> type <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
		public static bool IsDefined<T>(object value)
			where T : struct
		{
			return IsDefined(typeof(T), value);
		}

		/// <summary>
		/// Determines whether the specified value is defined in the <see cref="Enum"/> type <paramref name="enumType"/>.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <param name="value">The value to inspect.</param>
		/// <returns><c>true</c> if the specified value is defined in the <see cref="Enum"/> type <paramref name="enumType"/>; otherwise, <c>false</c>.</returns>
		public static bool IsDefined(Type enumType, object value)
		{
			CheckEnumType(enumType);

			EnumInfo info = GetEnumInfo(enumType);

			UInt64 val = Convert.ToUInt64(value, CultureInfo.InvariantCulture);

			int index = Array.BinarySearch(info.Values, val);

			if (index > -1)
				return true;
			else if (info.IsFlags)
				return (val & info.AllFlagsSetMask) == val;
			else
				return false;
		}

		/// <summary>
		/// Parses the specified input string and converts it to the <see cref="Enum"/> type <typeparamref name="T"/>.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <param name="input">The input string to parse.</param>
		/// <param name="ignoreCase">if set to <c>true</c> ignore the case of the <paramref name="input"/>.</param>
		/// <returns>A value of the <see cref="Enum"/> type <typeparamref name="T"/>.</returns>
		public static T Parse<T>(string input, bool ignoreCase)
			where T : struct
		{
			T value;

			if (TryParse(input, ignoreCase, out value))
				return value;
			else
				throw new ArgumentException(string.Empty, "input");
		}

		/// <summary>
		/// Tries to parse the specified input string and then convert it to the <see cref="Enum"/> type <typeparamref name="T"/>.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
		/// <param name="input">The input string to parse.</param>
		/// <param name="ignoreCase">if set to <c>true</c> ignore the case of the <paramref name="input"/>.</param>
		/// <param name="value">A value of the <see cref="Enum"/> type <typeparamref name="T"/>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
		public static bool TryParse<T>(string input, bool ignoreCase, out T value)
		{
			object val;

			if (TryParse(typeof(T), input, ignoreCase, out val))
			{
				value = (T) val;
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		/// <summary>
		/// Parses the specified input string and converts it to the <see cref="Enum"/> type <paramref name="enumType"/>.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <param name="input">The input string to parse.</param>
		/// <param name="ignoreCase">if set to <c>true</c> ignore the case of the <paramref name="input"/>.</param>
		/// <returns>A value of the <see cref="Enum"/> type <paramref name="enumType"/>.</returns>
		public static object Parse(Type enumType, string input, bool ignoreCase)
		{
			object value;

			if (TryParse(enumType, input, ignoreCase, out value))
				return value;
			else
				throw new ArgumentException(string.Empty, "input");
		}

		/// <summary>
		/// Tries to parse the specified input string and then convert it to the <see cref="Enum"/> type <paramref name="enumType"/>.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <param name="enumType">The <see cref="Enum"/> type.</param>
		/// <param name="input">The input string to parse.</param>
		/// <param name="ignoreCase">if set to <c>true</c> ignore the case of the <paramref name="input"/>.</param>
		/// <param name="value">A value of the <see cref="Enum"/> type <paramref name="enumType"/>.</param>
		/// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
		public static bool TryParse(Type enumType, string input, bool ignoreCase, out object value)
		{
			CheckEnumType(enumType);

			if (input == null || (input = input.Trim()).Length == 0)
			{
				value = null;
				return false;
			}

			if ((input[0] >= '0' && input[0] <= '9') || input[0] == '-' || input[0] == '+')
			{
				UInt64 val;

				if (UInt64.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out val))
				{
					EnumInfo info = GetEnumInfo(enumType);

					if (Array.BinarySearch(info.Values, val) < 0)
					{
						if (!info.IsFlags || (val & info.AllFlagsSetMask) != val)
						{
							value = null;
							return false;
						}
					}

					value = Enum.ToObject(enumType, val);
					return true;
				}
				else
				{
					value = null;
					return false;
				}
			}
			else
			{
				EnumInfo info = GetEnumInfo(enumType);
				string[] validNames = ignoreCase ? info.LowerCaseNames : info.Names;

				if (info.IsFlags)
				{
					string[] names = input.Split(',');
					UInt64 val = 0;

					for (int i = 0; i < names.Length; i++)
					{
						string name = ignoreCase ? names[i].Trim().ToLowerInvariant() : names[i].Trim();

						int index = Array.IndexOf(validNames, name);

						if (index > -1)
							val |= info.Values[index];
						else
						{
							value = null;
							return false;
						}
					}

					value = Enum.ToObject(enumType, val);
					return true;
				}
				else
				{
					if (ignoreCase)
						input = input.ToLowerInvariant();

					int index = Array.IndexOf(validNames, input);

					if (index > -1)
					{
						value = Enum.ToObject(enumType, info.Values[index]);
						return true;
					}
					else
					{
						value = null;
						return false;
					}
				}
			}
		}

		/// <summary>
		/// Checks if the specified <paramref name="value"/> is a valid enumeration value.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <param name="value">The value to validate.</param>
		/// <param name="isFlags">Indicates if the provided values be combined as flags.</param>
		/// <param name="throwOnError">Indicates if an <see cref="ArgumentException"/> will be thrown if the <paramref name="value"/> is invalid.</param>
		/// <param name="argumentName">An optional argument name that will be used when throwing the <see cref="ArgumentException"/>.</param>
		/// <param name="validValues">The enumeration values that will be used for the validation.</param>
		/// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
		public static bool CheckEnumerationValue(object value, bool isFlags, bool throwOnError, string argumentName, params object[] validValues)
		{
			argumentName = string.IsNullOrEmpty(argumentName) ? "value" : argumentName;

			if (value == null)
			{
				if (throwOnError)
					throw new ArgumentNullException(argumentName);
				else
					return false;
			}

			UInt64 mask = 0;
			UInt64 v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);

			if (validValues != null)
			{
				for (int i = 0; i < validValues.Length; i++)
				{
					UInt64 valid = Convert.ToUInt64(validValues[i], NumberFormatInfo.InvariantInfo);

					if (v == valid)
						return true;

					mask |= valid;
				}
			}

			if (isFlags && ((v & mask) == v))
				return true;
			else if (throwOnError)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Core_InvalidEnumArgument, value), argumentName);
			else
				return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="value"/> is a valid enumeration value.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <param name="value">The value to validate.</param>
		/// <param name="throwOnError">Indicates if an <see cref="ArgumentException"/> will be thrown if the <paramref name="value"/> is invalid.</param>
		/// <param name="argumentName">An optional argument name that will be used when throwing the <see cref="ArgumentException"/>.</param>
		/// <param name="minValue">The minimum valid enumeration value.</param>
		/// <param name="maxValue">The maximum valid enumeration value.</param>
		/// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
		public static bool CheckEnumerationValueByMinMax(object value, bool throwOnError, string argumentName, object minValue, object maxValue)
		{
			if (minValue == null) throw new ArgumentNullException(nameof(minValue));
			if (maxValue == null) throw new ArgumentNullException(nameof(maxValue));

			argumentName = string.IsNullOrEmpty(argumentName) ? "value" : argumentName;

			if (value == null)
			{
				if (throwOnError)
					throw new ArgumentNullException(argumentName);
				else
					return false;
			}

			UInt64 v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);

			UInt64 min = Convert.ToUInt64(minValue, NumberFormatInfo.InvariantInfo);
			UInt64 max = Convert.ToUInt64(maxValue, NumberFormatInfo.InvariantInfo);

			if (v >= min && v <= max)
				return true;
			else if (throwOnError)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Core_InvalidEnumArgument, value), argumentName);
			else
				return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="value"/> is a valid enumeration value.
		/// Only supports integer enumeration values.
		/// </summary>
		/// <param name="value">The value to validate.</param>
		/// <param name="throwOnError">Indicates if an <see cref="ArgumentException"/> will be thrown if the <paramref name="value"/> is invalid.</param>
		/// <param name="argumentName">An optional argument name that will be used when throwing the <see cref="ArgumentException"/>.</param>
		/// <param name="mask">A bit flag mask of the valid enumeration values.</param>
		/// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
		public static bool CheckEnumerationValueByMask(object value, bool throwOnError, string argumentName, object mask)
		{
			if (mask == null) throw new ArgumentNullException(nameof(mask));

			argumentName = string.IsNullOrEmpty(argumentName) ? "value" : argumentName;

			if (value == null)
			{
				if (throwOnError)
					throw new ArgumentNullException(argumentName);
				else
					return false;
			}

			UInt64 v = Convert.ToUInt64(value, NumberFormatInfo.InvariantInfo);
			UInt64 m = Convert.ToUInt64(mask, NumberFormatInfo.InvariantInfo);

			if ((v & m) == v)
				return true;
			else if (throwOnError)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Core_InvalidEnumArgument, value), argumentName);
			else
				return false;
		}
	}
}