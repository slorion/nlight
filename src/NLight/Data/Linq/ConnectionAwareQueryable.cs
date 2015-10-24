// Author(s): Sébastien Lorion

using NLight.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace NLight.Data.Linq
{
	internal class ConnectionAwareQueryable<T>
		: IQueryable<T>
	{
		public ConnectionAwareQueryable(IQueryable<T> inner, DataSession<LinqDataSessionState> session)
		{
			if (inner == null) throw new ArgumentNullException(nameof(inner));
			if (session == null) throw new ArgumentNullException(nameof(session));

			this.Inner = inner;
			this.Session = session;
		}

		private IQueryable<T> Inner { get; }
		private DataSession<LinqDataSessionState> Session { get; }

		#region IQueryable Members

		public Type ElementType => this.Inner.ElementType;

		public Expression Expression => this.Inner.Expression;

		public IQueryProvider Provider => this.Inner.Provider;

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator() => new Enumerator(this.Inner.GetEnumerator(), this.Session);

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

		#endregion

		private class Enumerator
			: IEnumerator<T>
		{
			public Enumerator(IEnumerator<T> inner, DataSession<LinqDataSessionState> session)
			{
				if (inner == null) throw new ArgumentNullException(nameof(inner));
				if (session == null) throw new ArgumentNullException(nameof(session));

				this.Inner = inner;
				this.Session = session;
			}

			private IEnumerator<T> Inner { get; }
			private DataSession<LinqDataSessionState> Session { get; }

			#region IEnumerator<T> Members

			public T Current => this.Inner.Current;

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current => this.Current;

			public bool MoveNext() => this.Inner.MoveNext();

			public void Reset() => this.Inner.Reset();

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				this.Inner?.Dispose();

				if (this.Session != null && this.Session.TransactionContext.Affinity == TransactionContextAffinity.NotSupported)
				{
					Debug.Assert(this.Session.State != null);

					if (this.Session.State.ConnectionHandledByReader)
					{
						this.Session.State.Context.Connection.Close();
						this.Session.State.Context.Connection.Dispose();
					}
				}
			}

			#endregion
		}
	}
}