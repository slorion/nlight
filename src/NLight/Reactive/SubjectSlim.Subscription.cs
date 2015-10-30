// Author(s): Sébastien Lorion

using System;
using System.Diagnostics;

namespace NLight.Reactive
{
	partial class SubjectSlim<T>
	{
		private class Subscription
			: IDisposable
		{
			private SubjectSlim<T> _subject;
			private IObserver<T> _observer;

			public Subscription(SubjectSlim<T> subject, IObserver<T> observer)
			{
#if DEBUG
				_allocStackTrace = new StackTrace();
#endif

				if (subject == null) throw new ArgumentNullException(nameof(subject));
				if (observer == null) throw new ArgumentNullException(nameof(observer));

				_subject = subject;
				_observer = observer;
			}

			#region IDisposable members

#pragma warning disable CS0649
			private readonly StackTrace _allocStackTrace;
#pragma warning restore CS0649

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				try
				{
					var subject = _subject;
					var observer = _observer;

					if (subject != null && observer != null)
						subject.Unsubscribe(observer);

					_subject = null;
					_observer = null;
				}
				catch (Exception ex) when (!disposing)
				{
					Log.Source.TraceEvent(TraceEventType.Error, 0, Resources.LogMessages.Shared_ExceptionDuringFinalization, ex);
				}
			}

			~Subscription()
			{
				Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
				Dispose(false);
			}

			#endregion
		}
	}
}