// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace NLight.Tests.Unit.BCL.Data.MockDataProvider
{
	public class MockDataParameterCollection
		: DbParameterCollection
	{
		private readonly List<MockParameter> _list = new List<MockParameter>();
		private readonly Dictionary<string, MockParameter> _parameters = new Dictionary<string, MockParameter>(StringComparer.OrdinalIgnoreCase);

		public override int Count => _parameters.Count;
		public override bool IsFixedSize => false;
		public override bool IsReadOnly => false;
		public override bool IsSynchronized => false;
		public override object SyncRoot => new object();

		public override int Add(object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			var parameter = (MockParameter) value;

			_parameters.Add(parameter.ParameterName, parameter);
			_list.Add(parameter);

			return _parameters.Count - 1;
		}

		public override void AddRange(Array values)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));

			foreach (MockParameter parameter in values)
			{
				_parameters.Add(parameter.ParameterName, parameter);
				_list.Add(parameter);
			}
		}

		public override void Clear()
		{
			_parameters.Clear();
			_list.Clear();
		}

		public override bool Contains(string value) => _parameters.ContainsKey(value);
		public override bool Contains(object value) => _list.Contains((MockParameter) value);

		public override void CopyTo(Array array, int index) => _list.CopyTo((MockParameter[]) array, index);

		public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();

		protected override DbParameter GetParameter(string parameterName) => _parameters[parameterName];
		protected override DbParameter GetParameter(int index) => _list[index];

		public override int IndexOf(string parameterName) => _list.IndexOf(_parameters[parameterName]);
		public override int IndexOf(object value)=> _list.IndexOf((MockParameter) value);

		public override void Insert(int index, object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			var parameter = (MockParameter) value;

			_parameters.Add(parameter.ParameterName, parameter);
			_list.Insert(index, parameter);
		}

		public override void Remove(object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			var parameter = (MockParameter) value;

			_parameters.Remove(parameter.ParameterName);
			_list.Remove(parameter);
		}

		public override void RemoveAt(string parameterName)
		{
			MockParameter parameter;

			if (_parameters.TryGetValue(parameterName, out parameter))
				_list.Remove(parameter);
		}

		public override void RemoveAt(int index)
		{
			MockParameter parameter = _list[index];

			_parameters.Remove(parameter.ParameterName);
			_list.RemoveAt(index);
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			var newParameter = (MockParameter) value;

			MockParameter oldParameter;
			if (_parameters.TryGetValue(parameterName, out oldParameter))
			{
				_parameters.Remove(parameterName);
				_list[_list.IndexOf(oldParameter)] = newParameter;
			}
			else
			{
				_parameters[parameterName] = newParameter;
				_list.Add(newParameter);
			}
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			var newParameter = (MockParameter) value;
			var oldParameter = _list[index];

			_parameters[oldParameter.ParameterName] = newParameter;
			_list[index] = newParameter;
		}
	}
}